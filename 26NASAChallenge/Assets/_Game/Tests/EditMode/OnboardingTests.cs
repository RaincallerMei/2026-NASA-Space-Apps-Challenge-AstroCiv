using System;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using Mapmakers.UI;
using NUnit.Framework;

namespace Mapmakers.Tests
{
    // Lessons, Wiki, placement suggestions and onboarding text must match the real rules in Balance.json.
    public class OnboardingTests
    {
        static HelpData Help => HelpData.Load();
        static Balance B => BalanceLoader.Load();

        static string PageText(string id)
        {
            var page = Help.Page(id);
            Assert.IsNotNull(page, id);
            var parts = new List<string> { page.summary, page.best };
            foreach (var row in page.rows) parts.Add(row.text);
            return string.Join(" ", parts);
        }

        [Test]
        public void Help_LoadsWithSectionsIconsAndAPagePerBuilding()
        {
            Assert.Greater(Help.wiki.Length, 20);
            foreach (var page in Help.wiki)
            {
                Assert.Contains(page.section, Help.sections, page.id);
                Assert.IsTrue(Icons.Has(page.icon), page.id + " icon " + page.icon);
                Assert.IsFalse(string.IsNullOrEmpty(page.summary), page.id);
                foreach (var row in page.rows) Assert.IsTrue(Icons.Has(row.icon), page.id + " row icon " + row.icon);
            }
            foreach (BuildingType type in Enum.GetValues(typeof(BuildingType)))
            {
                var page = Help.BuildingPage(type.ToString());
                Assert.IsNotNull(page, "wiki page for " + type);
                Assert.IsFalse(string.IsNullOrEmpty(page.best), type + " has placement advice");
                Assert.AreEqual(B.Def(type).displayName, page.title, "wiki title matches the game name");
            }
            foreach (var id in new[] { "power", "battery", "air", "water", "food", "soil", "crew", "supplies" })
                Assert.IsNotNull(Help.ResourcePage(id), "resource page " + id);
        }

        [Test]
        public void Lessons_AreShort_AndEveryCardHasAKnownIcon()
        {
            foreach (var lesson in Help.lessons)
            {
                int words = 0;
                foreach (var step in lesson.steps)
                {
                    int n = StoryData.WordCount(step.text);
                    Assert.LessOrEqual(n, 25, lesson.id + ": " + step.text);
                    words += n;
                    foreach (var card in step.cards)
                    {
                        Assert.IsTrue(Icons.Has(card.icon), lesson.id + " card icon " + card.icon);
                        Assert.LessOrEqual(StoryData.WordCount(card.text), 8, lesson.id + ": " + card.text);
                    }
                    Assert.LessOrEqual(step.cards.Length, 6);
                    Assert.LessOrEqual(step.replies.Length, 2);
                    var crew = StoryData.Load().Crew(step.speaker);
                    Assert.IsTrue(crew != null || Help.Speaker(step.speaker) != null, lesson.id + " speaker " + step.speaker);
                }
                Assert.LessOrEqual(words, 200, lesson.id + " required part stays short: " + words + " words");
            }
            Assert.IsNull(Help.Lesson("lesson_teacher"), "the crew teaches the teacher's lesson in Tutorial 2");
        }

        // Every number the Wiki teaches is checked against the simulation settings.
        [Test]
        public void WikiNumbers_MatchTheRealRules()
        {
            var r = B.layout;
            StringAssert.Contains(r.cableGap + " tiles", PageText("b_SolarArray"));
            StringAssert.Contains("power for " + SimText.Num(B.powerLossToleranceHours) + " hours", PageText("ending"));
            StringAssert.Contains("below " + SimText.Num(B.safetyFailThreshold) + "% for " + SimText.Num(B.safetyFailHours) + " hours", PageText("ending"));
            StringAssert.Contains(Math.Round(r.longCableLoss * 100) + "%", PageText("b_SolarArray"));
            StringAssert.Contains(SimText.Num(B.Def(BuildingType.SolarArray).generationKW) + " kW", PageText("b_SolarArray"));
            StringAssert.Contains(r.cableGap + " tiles", PageText("b_Battery"));
            StringAssert.Contains(SimText.Num(B.Def(BuildingType.Battery).batteryRateKW) + " kW", PageText("b_Battery"));
            StringAssert.Contains(r.ductGap + " tile", PageText("b_LifeSupport"));
            StringAssert.Contains(SimText.Num(B.airRefillPerHour) + " hours of air", PageText("b_LifeSupport"));
            StringAssert.Contains(SimText.Num(B.airDrainPerHour) + " hour every hour", PageText("b_LifeSupport"));
            StringAssert.Contains(SimText.Num(r.plantLoopSavesKW) + " kW", PageText("b_Greenhouse"));
            StringAssert.Contains(B.Def(BuildingType.Greenhouse).soilCost + " bags", PageText("b_Greenhouse"));
            StringAssert.Contains(B.harvestSupplyReward.ToString(), PageText("b_Greenhouse"));
            StringAssert.Contains(r.shelterGap + " tiles", PageText("b_Shelter"));
            StringAssert.Contains(r.roverRange + " tiles", PageText("b_RoverGarage"));
            StringAssert.Contains(SimText.Num(r.soilPerTrip) + " bags", PageText("b_RoverGarage"));
            StringAssert.Contains(SimText.Num(r.waterPerTrip) + " L", PageText("b_RoverGarage"));
            StringAssert.Contains(B.layout.baseRadius + " tiles", PageText("b_Habitat"));
            StringAssert.Contains(SimText.Num(B.powerLossToleranceHours) + " hours", PageText("b_Habitat"));
            StringAssert.Contains(SimText.Num(B.airCapHours) + " hours", PageText("r_air"));
            StringAssert.Contains(SimText.Num(r.soilStockCap) + " bags", PageText("r_soil"));
            StringAssert.Contains(SimText.Num(B.safetyFailThreshold) + "%", PageText("r_crew"));
            StringAssert.Contains(SimText.Num(B.storm.onsetHour) + " hours", PageText("e_storm"));
            StringAssert.Contains(SimText.Num(B.storm.selfReturnDelayHours) + " hour", PageText("e_storm"));
            StringAssert.Contains(SimText.Num(B.shadow.warningHours) + " hours", PageText("e_shadow"));
            StringAssert.Contains(B.repairCost + " supplies", PageText("e_other"));
            Assert.AreEqual(9.0, B.CrewMealsPerHour * 24, 1e-9, "food page says the crew eats 9 meals a day");
        }

        [Test]
        public void Wiki_PowerOrder_MatchesShedPriorities()
        {
            int Priority(BuildingType t) => B.Def(t).priority;
            Assert.AreEqual(Priority(BuildingType.Communications), Priority(BuildingType.RoverGarage));
            Assert.Greater(Priority(BuildingType.RoverGarage), Priority(BuildingType.Greenhouse));
            Assert.Greater(Priority(BuildingType.Greenhouse), Priority(BuildingType.Shelter));
            Assert.Greater(Priority(BuildingType.Shelter), Priority(BuildingType.LifeSupport));
            Assert.AreEqual(Priority(BuildingType.Habitat), Priority(BuildingType.LifeSupport));
        }

        [Test]
        public void RenamedBuildings_TellPlayersWhatTheyAreFor()
        {
            Assert.AreEqual("Air Support", B.Def(BuildingType.LifeSupport).displayName);
            Assert.AreEqual("Radio Dish", B.Def(BuildingType.Communications).displayName);
            var story = StoryData.Load();
            foreach (var line in story.debrief) StringAssert.DoesNotContain("Life Support", line.text, line.id);
            foreach (var line in story.hints) StringAssert.DoesNotContain("Life Support", line.text, line.id);
            foreach (var text in story.ChatTexts()) StringAssert.DoesNotContain("Life Support", text);
        }

        [Test]
        public void Keywords_HighlightWholeNamesOnly()
        {
            string marked = Keywords.Mark("Air Support needs power. It was repaired and powered.");
            StringAssert.Contains(">Air Support<", marked);
            StringAssert.Contains(">power<", marked);
            StringAssert.Contains("repaired", marked);
            StringAssert.DoesNotContain(">air<", marked);
            StringAssert.Contains("powered.", marked);
            StringAssert.Contains("<b>", marked, "bold too, so meaning never depends on colour alone");
        }

        [Test]
        public void HabitatAlone_PopupNamesOnlyTheHabitat()
        {
            var run = MissionRun.ForLevel(LevelCatalog.Load().Level("L2"));
            run.StartMission();
            run.Choose(BuildingType.Habitat);
            Assert.IsTrue(run.Place(8, 6).Ok || run.Place(4, 4).Ok || run.Place(10, 8).Ok, "room for a Habitat");
            run.clock.SetPaused(false);
            run.clock.SetSpeed(1);
            for (int i = 0; i < 20 && run.State.events.Find(e => e.code == "critical_out") == null; i++) run.Tick(0.25);
            Assert.IsNotNull(run.State.events.Find(e => e.code == "critical_out"), "the Habitat alone has no power");
            CollectionAssert.AreEqual(new[] { "Habitat" }, run.UnpoweredCritical());
            StringAssert.DoesNotContain("Air Support", run.NoPowerText());
            StringAssert.Contains("Habitat", run.NoPowerText());
            StringAssert.StartsWith("Next: Air Support", run.Hint, "the next tip points at Air Support");
        }

        [Test]
        public void Suggestions_AreAllowedSpots_ThatMeetEveryLink()
        {
            var level = LevelCatalog.Load().Level("L2");
            var sim = LevelSetup.CreateSimulator(level);
            var s = sim.NewGame();
            s.supplies = 1000;
            Assert.IsEmpty(Layout.Suggest(sim, s, BuildingType.Battery), "nothing to suggest before the Habitat");
            StarterBase.Build(sim, s);
            s.supplies = 1000;
            var battery = s.First(BuildingType.Battery);
            sim.Remove(s, battery.id);

            var spots = Layout.Suggest(sim, s, BuildingType.Battery);
            Assert.IsNotEmpty(spots, "a Battery can go near the Solar Arrays");
            foreach (var (x, y) in spots)
            {
                Assert.IsTrue(Placement.Fits(sim, s, BuildingType.Battery, x, y), "suggested spots are allowed");
                foreach (var c in Layout.Checks(sim.balance, sim.terrain, s, BuildingType.Battery, x, y)) Assert.IsTrue(c.met, c.Text);
            }
            foreach (var (x, y) in Layout.Suggest(sim, s, BuildingType.Shelter))
                Assert.LessOrEqual(Layout.GapTo(sim.balance, s, BuildingType.Habitat, x, y, 2, 2), sim.balance.layout.shelterGap);
            Assert.IsEmpty(Layout.Suggest(sim, s, BuildingType.Communications), "no link to suggest for the Radio Dish");
        }

        [Test]
        public void SolarSuggestions_PickTheBrightestGround()
        {
            var level = LevelCatalog.Load().Level("L1");
            var sim = LevelSetup.CreateSimulator(level);
            var s = sim.NewGame();
            var spots = Layout.Suggest(sim, s, BuildingType.SolarArray);
            Assert.IsNotEmpty(spots);
            double best = 0, worstSuggested = 1;
            for (int y = 0; y < sim.terrain.height; y++)
                for (int x = 0; x < sim.terrain.width; x++)
                    if (Placement.Fits(sim, s, BuildingType.SolarArray, x, y)) best = Math.Max(best, sim.terrain.AverageSun(x, y, 2, 2));
            foreach (var (x, y) in spots) worstSuggested = Math.Min(worstSuggested, sim.terrain.AverageSun(x, y, 2, 2));
            Assert.Greater(worstSuggested, best - 0.1);
        }

        [Test]
        public void OpeningCutscene_IsDaytimeMississauga_WithoutUofT()
        {
            var story = StoryData.Load();
            var opening = LevelCatalog.Load().Cutscene("c_dream");
            string first = story.Story(opening.slides[0].lines[0]);
            StringAssert.Contains("10-year-old", first);
            StringAssert.Contains("Mississauga, Ontario", first);
            foreach (var line in story.story)
            {
                StringAssert.DoesNotContain("University of Toronto", line.text, line.id);
                StringAssert.DoesNotContain("Night", line.text, line.id);
            }
        }

        [Test]
        public void Onboarding_RunsDrawingThenCrewThenOutpost()
        {
            var catalog = LevelCatalog.Load();
            Assert.AreEqual("", catalog.Level("L1").after, "no text lesson after the drawing");
            CollectionAssert.IsEmpty(catalog.Level("T2").needs);
            Assert.IsTrue(catalog.Level("T2").IsTraining);
            CollectionAssert.IsEmpty(catalog.Level("L2").needs);
            Assert.Less(catalog.IndexOf("L1"), catalog.IndexOf("T2"));
            Assert.Less(catalog.IndexOf("T2"), catalog.IndexOf("L2"));
        }
    }
}
