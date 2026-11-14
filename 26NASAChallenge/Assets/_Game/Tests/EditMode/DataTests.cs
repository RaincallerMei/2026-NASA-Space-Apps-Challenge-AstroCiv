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
            Assert.AreEqual(6, count);
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
        public void Balance_LoadsAllSevenBuildings()
        {
            var balance = BalanceLoader.Load();
            foreach (BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
                Assert.IsNotNull(balance.Def(type), type.ToString());
            Assert.AreEqual("fictional balance", balance.classification);
            Assert.AreEqual(30.0, balance.Def(BuildingType.Battery).batteryKWh);
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
        public void Story_HasSixHints_FiveCards_AndTheFinalMessage()
        {
            var story = StoryData.Load();
            Assert.AreEqual(6, story.hints.Length);
            Assert.GreaterOrEqual(story.cards.Length, 5);
            foreach (var card in story.cards) Assert.IsFalse(string.IsNullOrEmpty(card.source), card.id);
            Assert.AreEqual("I used to build this world on a computer.", story.Story("e_msg1"));
            Assert.AreEqual("Now we are building it together.", story.Story("e_msg2"));
            StringAssert.Contains("hangs low", story.Story("e_earth"));
            foreach (var line in story.story) StringAssert.DoesNotContain("Earthrise", line.text);
        }
    }
}
