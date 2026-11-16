using System.Collections.Generic;
using System.Linq;
using Mapmakers.Data;
using Mapmakers.Sim;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    public class DataTests
    {
        static IEnumerable<FixtureHeader> AllHeaders()
        {
            foreach (var name in FixtureLoader.JsonFixtures)
            {
                string text = FixtureLoader.LoadText(name);
                Assert.IsNotNull(text, "missing fixture " + name);
                yield return FixtureLoader.ParseJsonHeader(text);
            }
            foreach (var name in FixtureLoader.CsvFixtures)
            {
                string text = FixtureLoader.LoadText(name);
                Assert.IsNotNull(text, "missing fixture " + name);
                yield return FixtureLoader.ParseCsv(text).header;
            }
        }

        [Test]
        public void EveryFixture_Parses_AndHasRequiredHeaderFields()
        {
            int count = 0;
            foreach (var header in AllHeaders())
            {
                count++;
                Assert.IsFalse(string.IsNullOrEmpty(header.source), "source");
                Assert.IsFalse(string.IsNullOrEmpty(header.retrieved_utc), "retrieved_utc");
                Assert.IsFalse(string.IsNullOrEmpty(header.units), "units");
                Assert.Contains(header.classification, FixtureLoader.Classifications);
                if (!header.IsSynthetic) Assert.IsFalse(string.IsNullOrEmpty(header.query_url), "real data needs a query_url");
            }
            Assert.AreEqual(8, count);
        }

        [Test]
        public void NoFixture_ContainsAnApiKey()
        {
            foreach (var name in FixtureLoader.JsonFixtures.Concat(FixtureLoader.CsvFixtures))
                StringAssert.DoesNotContain("api_key=", FixtureLoader.LoadText(name));
        }

        [Test]
        public void Donki_HasFlareCmeAndParticleEvent()
        {
            var pack = FixtureLoader.Donki();
            Assert.IsFalse(string.IsNullOrEmpty(pack.flare.classType));
            Assert.IsFalse(string.IsNullOrEmpty(pack.flare.peakTime));
            Assert.IsFalse(string.IsNullOrEmpty(pack.cme.startTime));
            Assert.GreaterOrEqual(pack.sep.Length, 1);
            Assert.IsFalse(string.IsNullOrEmpty(pack.sep[0].eventTime));
        }

        [Test]
        public void Radlab_Power_Osdr_Csa_HaveContent()
        {
            var radlab = FixtureLoader.Radlab();
            Assert.GreaterOrEqual(radlab.rows, 30);
            Assert.Greater(radlab.medianUGyPerHour, 0f);

            var power = FixtureLoader.Power();
            Assert.AreEqual(12, power.monthly.Length);
            Assert.Greater(power.annual, 0f);

            var osdr = FixtureLoader.Osdr();
            Assert.GreaterOrEqual(osdr.cards.Length, 1);
            foreach (var card in osdr.cards) Assert.IsFalse(string.IsNullOrEmpty(card.kidLine));

            var csa = FixtureLoader.Csa();
            Assert.GreaterOrEqual(csa.cards.Length, 1);
        }

        [Test]
        public void Illumination_Is20x15_AndMakesValidTerrain()
        {
            var grid = FixtureLoader.Illumination();
            Assert.AreEqual(20, grid.width);
            Assert.AreEqual(15, grid.height);

            var balance = BalanceLoader.Load();
            var terrain = TerrainFactory.FromIllumination(grid, balance.terrain);
            int free = 0;
            for (int y = 0; y < terrain.height; y++)
            {
                for (int x = 0; x < terrain.width; x++)
                {
                    Assert.GreaterOrEqual(terrain.Sun(x, y), 0.5);
                    Assert.LessOrEqual(terrain.Sun(x, y), 1.0);
                    if (!terrain.IsBlocked(x, y)) free++;
                }
            }
            Assert.Greater(free, 200, "most of the map must be buildable");
            Assert.AreEqual(terrain.Sun(0, 14), 0.5 + 0.5 * System.Math.Min(1, grid.percent[0, 0] / balance.terrain.sunFullAtPercent), 1e-9,
                "the first CSV row is the top of the map");
        }

        [Test]
        public void Jezero_IsARealMolaCrop_WithTheCraterInside()
        {
            var grid = FixtureLoader.Jezero();
            Assert.AreEqual(20, grid.width);
            Assert.AreEqual(15, grid.height);
            Assert.AreEqual("simplified", grid.header.classification);
            StringAssert.Contains("MOLA", grid.header.source);
            double min = double.MaxValue, max = double.MinValue;
            foreach (double h in grid.metres) { min = System.Math.Min(min, h); max = System.Math.Max(max, h); }
            Assert.Less(min, -2600.0, "the crater floor is deep");
            Assert.Greater(max - min, 1000.0, "rim and floor differ by more than a kilometre");
            Assert.Less(grid.metres[7, 10], grid.metres[7, 0], "the middle of the map is lower than the plains outside");
        }

        [Test]
        public void Balance_LoadsAllEightBuildings()
        {
            var balance = BalanceLoader.Load();
            foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
                Assert.IsNotNull(balance.Def(type), type.ToString());
            Assert.AreEqual("fictional balance", balance.classification);
            Assert.AreEqual(12.0, balance.Def(BuildingType.Battery).batteryKWh);
            Assert.Greater(balance.Def(BuildingType.Greenhouse).soilCost, 0, "the Greenhouse needs rover soil");
            Assert.Greater(balance.waterPerCrewPerHour, 0.0, "the crew drinks water");
        }

        [Test]
        public void Story_StaysWithinWordLimits()
        {
            var story = StoryData.Load();
            int storyWords = 0;
            foreach (var line in story.story)
            {
                int words = StoryData.WordCount(line.text);
                Assert.LessOrEqual(words, StoryData.MaxStoryLineWords, line.id + ": " + line.text);
                storyWords += words;
            }
            Assert.Less(storyWords, StoryData.MaxStoryWords, "story words: " + storyWords);

            int chatWords = 0;
            foreach (var text in story.ChatTexts())
            {
                int words = StoryData.WordCount(text);
                Assert.LessOrEqual(words, StoryData.MaxChatLineWords, text);
                chatWords += words;
            }
            Assert.Less(storyWords + chatWords, StoryData.MaxStoryAndChatWords, "story + chat words: " + (storyWords + chatWords));
            UnityEngine.Debug.Log("Story words: " + storyWords + ", chat words: " + chatWords);
        }

        [Test]
        public void Chat_HasNoDeadEnds()
        {
            var story = StoryData.Load();
            Assert.AreEqual(3, story.crew.Length);
            foreach (var member in story.crew)
                Assert.IsNotNull(story.Chat(member.id, "default"), member.id + " needs a default chat");

            foreach (var entry in story.chat)
            {
                Assert.IsNotNull(story.Crew(entry.crew), "unknown crew " + entry.crew);
                Assert.That(entry.replies.Length, Is.InRange(2, 3), entry.line);
                foreach (var reply in entry.replies)
                {
                    Assert.IsFalse(string.IsNullOrEmpty(reply.text));
                    Assert.IsFalse(string.IsNullOrEmpty(reply.answer), "every reply ends with an answer: " + reply.text);
                }
            }
            Assert.IsNotNull(story.Chat("geologist", "after_called_first"), "a line must remember the storm choice");
            Assert.IsNotNull(story.Chat("geologist", "after_not_called"));
        }

        [Test]
        public void Story_HasHints_Cards_FailureText_AndTheFinalMessage()
        {
            var story = StoryData.Load();
            Assert.AreEqual(18, story.hints.Length, "10 tips plus 8 \"Next:\" and suggestion tips");
            Assert.GreaterOrEqual(story.cards.Length, 9);
            foreach (var reason in new[] { "NoAir", "NoFood", "NoWater", "PowerLoss", "RadiationOutside", "RadiationNoShelter" })
            {
                Assert.IsFalse(story.Debrief("head_" + reason).StartsWith("["), reason + " headline");
                StringAssert.DoesNotContain("died", story.Debrief("head_" + reason).ToLowerInvariant());
            }
            foreach (var card in story.cards) Assert.IsFalse(string.IsNullOrEmpty(card.source), card.id);
            Assert.AreEqual("I used to build this world on a computer.", story.Story("e_msg1"));
            Assert.AreEqual("Now we are building it together.", story.Story("e_msg2"));
            StringAssert.Contains("hangs low", story.Story("e_earth"));
            foreach (var line in story.story) StringAssert.DoesNotContain("Earthrise", line.text);
        }

        // ---------- science facts ----------

        static readonly string[] FactTriggers =
        {
            "placed_Habitat", "placed_SolarArray", "placed_Battery", "placed_LifeSupport", "placed_Communications",
            "placed_Greenhouse", "placed_Shelter", "placed_RoverGarage",
            "first_harvest", "supply_drop", "shadow_start", "fault", "storm_warning", "storm_onset", "storm_over", "sunrise_start"
        };

        static Dictionary<string, string> StoryValues()
        {
            var values = new Dictionary<string, string>();
            FixtureLoader.Donki().Storm.AddValues(values);
            FixtureLoader.Radlab().AddValues(values);
            return values;
        }

        [Test]
        public void ScienceFacts_AreShort_Sourced_AndFiledInTheWiki()
        {
            var story = StoryData.Load();
            var help = HelpData.Load();
            var values = StoryValues();
            Assert.GreaterOrEqual(story.cards.Length, 20);
            foreach (var card in story.cards)
            {
                Assert.IsFalse(string.IsNullOrEmpty(card.source), card.id + " source");
                StringAssert.StartsWith("https://", card.url, card.id + " url");
                Assert.IsFalse(string.IsNullOrEmpty(card.label), card.id + " label");
                Assert.Contains(card.planet, new[] { "", "Moon", "Mars" }, card.id);
                if (!string.IsNullOrEmpty(card.crew)) Assert.IsNotNull(story.Crew(card.crew), card.id + " crew");
                if (!string.IsNullOrEmpty(card.trigger)) Assert.Contains(card.trigger, FactTriggers, card.id + " trigger");
                var page = help.Page(card.wiki);
                Assert.IsNotNull(page, card.id + " wiki page");
                Assert.AreEqual("Science", page.section, card.id);
                foreach (var line in new[] { card.text, card.text2 })
                {
                    string filled = StoryData.Fill(line, values);
                    Assert.IsNotNull(filled, card.id + " has a value for every {word} in Story Mode");
                    Assert.LessOrEqual(StoryData.WordCount(filled), StoryData.MaxChatLineWords, card.id + ": " + filled);
                }
            }
            CollectionAssert.AllItemsAreUnique(System.Array.ConvertAll(story.cards, c => c.id));
        }

        // The Science Facts collection shows a hint for each fact not found yet. The hint must name the real way to find it.
        [Test]
        public void ScienceFactHints_NameTheRealEventAndPlace()
        {
            var story = StoryData.Load();
            var balance = BalanceLoader.Load();
            var sunriseLevels = LevelCatalog.Load().levels.Where(l => l.events.Any(e => e.kind == "Sunrise")).ToList();
            Assert.AreEqual(1, sunriseLevels.Count, "sunrise happens in one level");
            foreach (var card in story.cards)
            {
                string hint = card.hint;
                Assert.IsFalse(string.IsNullOrEmpty(hint), card.id + " hint");
                Assert.LessOrEqual(StoryData.WordCount(hint), StoryData.MaxChatLineWords, card.id + ": " + hint);
                if (card.planet == "") Assert.IsFalse(hint.Contains("Moon") || hint.Contains("Mars"), card.id + " works on both planets: " + hint);
                else StringAssert.Contains(card.planet, hint, card.id);
                if (card.trigger.StartsWith("placed_"))
                {
                    var type = (BuildingType)System.Enum.Parse(typeof(BuildingType), card.trigger.Substring("placed_".Length));
                    StringAssert.Contains(balance.Def(type).displayName, hint, card.id);
                }
                if (card.trigger == "fault") StringAssert.Contains("Free Build on Normal or Hard", hint, "space rocks never come on Easy or in Story Mode");
                if (card.trigger == "sunrise_start") StringAssert.Contains(sunriseLevels[0].title, hint, card.id);
                if (card.trigger == "") StringAssert.Contains("Why plants?", hint, card.id + " is only shown by the first-harvest button");
            }
            // One fact per event: a second fact for the same event needs that event a second time.
            foreach (string planet in new[] { "Moon", "Mars" })
                foreach (string trigger in FactTriggers)
                {
                    var facts = story.Facts(trigger, planet);
                    for (int i = 0; i < facts.Count; i++)
                        Assert.AreEqual(i > 0, facts[i].hint.Contains("second"), facts[i].id + " on " + planet + ": " + facts[i].hint);
                }
        }

        [Test]
        public void ScienceFacts_EachRealEventHasOne_AndMostPlanetsGetTheirOwn()
        {
            var story = StoryData.Load();
            foreach (var trigger in FactTriggers)
                Assert.Greater(story.Facts(trigger, "Moon").Count + story.Facts(trigger, "Mars").Count, 0, trigger);
            foreach (var trigger in new[] { "placed_SolarArray", "placed_Battery", "placed_LifeSupport", "placed_RoverGarage", "shadow_start" })
            {
                Assert.IsNotEmpty(story.Facts(trigger, "Moon"), trigger + " on the Moon");
                Assert.IsNotEmpty(story.Facts(trigger, "Mars"), trigger + " on Mars");
                Assert.AreNotEqual(story.Facts(trigger, "Moon")[0].id, story.Facts(trigger, "Mars")[0].id, trigger);
            }
            Assert.AreEqual(0, story.Facts("sunrise_start", "Mars").Count, "Mars has no scripted sunrise");
        }

        // Each fact trigger must be an event code the simulation really logs.
        [Test]
        public void ScienceFactTriggers_AreEventsTheSimulatorLogs()
        {
            var sim = SimTestKit.Timed(60,
                ScheduledEvent.Make(2, EventKind.Shadow, 0.8),
                ScheduledEvent.Make(3, EventKind.Fault, 1, 0),
                ScheduledEvent.Make(5, EventKind.SupplyDrop, 1, 10),
                ScheduledEvent.Make(12, EventKind.SolarStorm),
                ScheduledEvent.Make(40, EventKind.Sunrise));
            sim.canFail = false;
            var s = SimTestKit.FullBase(sim, 5);
            SimTestKit.Put(sim, s, BuildingType.Shelter, 0, 9);
            SimTestKit.Put(sim, s, BuildingType.RoverGarage, 6, 9);
            var codes = new HashSet<string>();
            foreach (var e in s.events) codes.Add(e.code);
            while (!s.Finished && s.timeHours < 59)
            {
                s = sim.Step(s);
                foreach (var e in s.events) codes.Add(e.code);
            }
            foreach (var trigger in FactTriggers) Assert.IsTrue(codes.Contains(trigger), trigger + " never logged. Logged: " + string.Join(", ", codes));
        }

        [Test]
        public void StormSet_HoldsDifferentRealXFlares_WithLinkedCloudsAndParticles()
        {
            var set = FixtureLoader.StormSet();
            Assert.AreEqual("NASA-derived", set.classification);
            Assert.That(set.storms.Length, Is.InRange(5, 10));
            var ids = new HashSet<string>();
            foreach (var storm in set.storms)
            {
                Assert.IsTrue(ids.Add(storm.flare.id), "duplicate " + storm.flare.id);
                StringAssert.StartsWith("X", storm.flare.classType);
                Assert.Greater(storm.cme.speedKmS, 0f, storm.flare.id);
                Assert.GreaterOrEqual(storm.sep.Length, 1, storm.flare.id);
                var values = new Dictionary<string, string>();
                storm.AddValues(values);
                foreach (var key in new[] { "flare", "date", "speed", "moonMinutes", "sepDelay" })
                    Assert.IsTrue(values.ContainsKey(key), storm.flare.id + " " + key);
            }
            Assert.AreSame(set.Pick(42, 0), set.Pick(42, 0), "same seed, same storm");

            // c_sep quotes the range of particle delays in this set: "16 minutes to almost 3 days".
            StringAssert.Contains("16 minutes to almost 3 days", StoryData.Load().Card("c_sep").text2);
            double shortest = double.MaxValue, longest = 0;
            foreach (var storm in set.storms)
            {
                var peak = System.DateTime.Parse(storm.flare.peakTime, null, System.Globalization.DateTimeStyles.AdjustToUniversal);
                double first = double.MaxValue;
                foreach (var sep in storm.sep)
                {
                    double minutes = (System.DateTime.Parse(sep.eventTime, null, System.Globalization.DateTimeStyles.AdjustToUniversal) - peak).TotalMinutes;
                    if (minutes >= 0) first = System.Math.Min(first, minutes);
                }
                shortest = System.Math.Min(shortest, first);
                longest = System.Math.Max(longest, first);
            }
            Assert.AreEqual(16, shortest, 0.5);
            Assert.That(longest / 1440, Is.InRange(2.0, 3.0));
            var seen = new HashSet<DonkiStorm>();
            for (int seed = 0; seed < 50; seed++) seen.Add(set.Pick(seed, 0));
            Assert.Greater(seen.Count, 3, "different maps meet different real storms");
        }

        [Test]
        public void StoryStorm_MatchesTheSavedMay2024Event()
        {
            var values = new Dictionary<string, string>();
            FixtureLoader.Donki().Storm.AddValues(values);
            Assert.AreEqual("X5.8", values["flare"]);
            Assert.AreEqual("1263", values["speed"]);
            Assert.AreEqual("5", values["moonMinutes"], "384,400 km / 1263 km/s is about 5 minutes");
            Assert.AreEqual("47 minutes", values["sepDelay"], "flare peak 01:23 UTC, first particles 02:10 UTC");
        }

        [Test]
        public void Fill_ReturnsNull_WhenRealDataIsMissing()
        {
            var values = new Dictionary<string, string> { { "speed", "900" } };
            Assert.AreEqual("900 km/s", StoryData.Fill("{speed} km/s", values));
            Assert.IsNull(StoryData.Fill("Kp {kp}", values));
        }

        [Test]
        public void Osdr_HasRadiationStudies_ThatSayWhereTheyRan()
        {
            var cards = FixtureLoader.Osdr().cards;
            Assert.GreaterOrEqual(System.Array.FindAll(cards, c => c.topic == "radiation").Length, 3);
            foreach (var card in cards)
            {
                Assert.IsFalse(string.IsNullOrEmpty(card.where), card.accession);
                Assert.LessOrEqual(StoryData.WordCount(card.kidLine), StoryData.MaxChatLineWords, card.accession);
            }
        }
    }
}
