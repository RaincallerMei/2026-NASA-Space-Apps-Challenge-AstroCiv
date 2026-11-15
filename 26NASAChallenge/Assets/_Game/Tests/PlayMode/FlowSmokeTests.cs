using System;
using System.Collections;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using Mapmakers.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Object = UnityEngine.Object;

namespace Mapmakers.Tests
{
    // Plays the real scenes. Any error logged during a test fails it (Unity's LogAssert default).
    // Time is driven through the run's own clock (Tick) so the tests stay fast and deterministic.
    public class FlowSmokeTests
    {
        static IEnumerator WaitFor(Func<bool> condition, float seconds = 15)
        {
            float end = Time.realtimeSinceStartup + seconds;
            while (!condition())
            {
                if (Time.realtimeSinceStartup > end) Assert.Fail("timed out waiting");
                yield return null;
            }
        }

        static IEnumerator WaitForScene(string name)
        {
            yield return WaitFor(() => SceneManager.GetActiveScene().name == name);
            yield return null;
            yield return null;
        }

        static GameScreen Game() => Object.FindAnyObjectByType<GameScreen>();

        // The tests change saved progress; the player's own progress in the editor is put back afterwards.
        readonly Dictionary<string, int> savedInts = new Dictionary<string, int>();
        readonly Dictionary<string, string> savedStrings = new Dictionary<string, string>();

        static IEnumerable<string> IntKeys()
        {
            foreach (var level in LevelCatalog.Load().levels)
                foreach (var kind in new[] { "best.", "passed.", "basescore." }) yield return "mapmakers." + kind + level.id;
            foreach (var lesson in HelpData.Load().lessons) yield return "mapmakers.lesson." + lesson.id;
            yield return "mapmakers.best.free";
            yield return "mapmakers.hints";
            yield return "mapmakers.suggest";
        }

        [SetUp]
        public void SaveProgress()
        {
            savedInts.Clear();
            savedStrings.Clear();
            foreach (var key in IntKeys())
                if (PlayerPrefs.HasKey(key)) savedInts[key] = PlayerPrefs.GetInt(key);
            foreach (var level in LevelCatalog.Load().levels)
            {
                string key = "mapmakers.base." + level.id;
                if (PlayerPrefs.HasKey(key)) savedStrings[key] = PlayerPrefs.GetString(key);
            }
        }

        [TearDown]
        public void RestoreProgress()
        {
            if (Wiki.IsOpen) Wiki.Close();
            foreach (var key in IntKeys())
            {
                if (savedInts.TryGetValue(key, out int v)) PlayerPrefs.SetInt(key, v);
                else PlayerPrefs.DeleteKey(key);
            }
            foreach (var level in LevelCatalog.Load().levels)
            {
                string key = "mapmakers.base." + level.id;
                if (savedStrings.TryGetValue(key, out string v)) PlayerPrefs.SetString(key, v);
                else PlayerPrefs.DeleteKey(key);
            }
            PlayerPrefs.Save();
        }

        // Finishes every story picture and lesson until the target scene opens.
        static IEnumerator SkipStory(string until)
        {
            CutsceneScreen last = null;
            for (int i = 0; i < 12; i++)
            {
                yield return WaitFor(() => SceneManager.GetActiveScene().name == until
                    || (Object.FindAnyObjectByType<CutsceneScreen>() is var c && c != null && c != last));
                if (SceneManager.GetActiveScene().name == until) break;
                last = Object.FindAnyObjectByType<CutsceneScreen>();
                yield return null;
                yield return null;
                var lesson = last.GetComponent<LessonScreen>();
                if (lesson != null) lesson.Finish();
                else last.Leave();
            }
            yield return WaitForScene(until);
        }

        static IEnumerator NextLesson(string id)
        {
            yield return WaitFor(() => Object.FindAnyObjectByType<LessonScreen>() is var l && l != null && l.Lesson != null && l.Lesson.id == id && !l.Done);
            yield return null;
        }

        static void CloseWindows(GameScreen game)
        {
            for (int i = 0; i < 20 && game.ModalOpen; i++) game.PressModal(0);
        }

        // Runs the mission at 4x until it ends (or `hours` pass), letting frames render now and then.
        static IEnumerator Play(GameScreen game, double hours = 400, Action<MissionRun> everyHour = null)
        {
            var run = game.Run;
            run.clock.SetSpeed(4);
            for (int i = 0; i < hours * 2 && !run.Ended; i++)
            {
                CloseWindows(game);
                if (everyHour != null && i % 2 == 0) everyHour(run);
                run.Tick(0.25);
                if (i % 8 == 0) yield return null;
            }
            yield return null;
            yield return null;
        }

        static IEnumerator StartFree(double hours = 144)
        {
            Flow.StartFree(new FreeSettings { planet = "Moon", seed = 11, difficulty = Difficulty.Easy, hours = hours });
            yield return WaitForScene(Flow.GameScene);
        }

        static void PlaceAndPowerShelter(MissionRun run)
        {
            var shelter = run.State.First(BuildingType.Shelter);
            if (shelter == null)
            {
                run.Choose(BuildingType.Shelter);
                for (int y = 0; y < run.sim.terrain.height && run.State.Count(BuildingType.Shelter) == 0; y++)
                    for (int x = 0; x < run.sim.terrain.width && run.State.Count(BuildingType.Shelter) == 0; x++)
                        if (run.sim.CanPlace(run.State, BuildingType.Shelter, x, y).Ok) run.Place(x, y);
                shelter = run.State.First(BuildingType.Shelter);
            }
            Assert.IsNotNull(shelter, "no room for a Shelter");
            if (!shelter.enabled) run.Toggle(shelter.id);
        }

        [UnityTest]
        public IEnumerator Boot_OpensTheMenu_WithStoryAndFreeBuild()
        {
            SceneManager.LoadScene("Boot");
            yield return WaitForScene(Flow.MenuScene);
            var menu = Object.FindAnyObjectByType<MenuScreen>();
            Assert.IsNotNull(menu);
            Assert.AreEqual("main", menu.Current);
            menu.Show("story");
            Assert.AreEqual("story", menu.Current);
            menu.Show("free");
            Assert.AreEqual("free", menu.Current);
        }

        [UnityTest]
        public IEnumerator Tutorial_IsUntimed_ThenFinishGivesAGrade()
        {
            Flow.StartLevel("L1");
            yield return WaitForScene(Flow.CutsceneScene);
            var cutscene = Object.FindAnyObjectByType<CutsceneScreen>();
            Assert.IsNotNull(cutscene, "story plays in its own scene");
            Assert.IsNull(Game(), "no game UI behind the cutscene");
            cutscene.Advance();
            cutscene.Leave();

            yield return WaitForScene(Flow.GameScene);
            var game = Game();
            Assert.IsTrue(game.ModalOpen, "the level opens with its goals");
            CloseWindows(game);
            var run = game.Run;
            Assert.IsTrue(run.IsDrawing);
            run.Tick(10);
            Assert.AreEqual(0.0, run.State.timeHours, "the drawing has no clock");

            run.Choose(BuildingType.Habitat);
            game.ClickTile(2, 2);
            run.Choose(BuildingType.SolarArray);
            game.ClickTile(3, 10);
            run.Choose(BuildingType.Greenhouse);
            game.ClickTile(9, 3);
            Assert.IsFalse(run.DrawingReady, "one Solar Array cannot power the drawing");
            game.PressFinish();
            Assert.AreEqual(Flow.GameScene, SceneManager.GetActiveScene().name, "Finish waits for the goals");
            run.Choose(BuildingType.SolarArray);
            game.ClickTile(1, 13);
            Assert.IsTrue(run.DrawingReady);
            game.PressFinish();

            yield return WaitForScene(Flow.ResultsScene);
            var results = Object.FindAnyObjectByType<ResultsScreen>();
            Assert.IsTrue(results.Result.canContinue);
            Assert.That(results.Result.score.letter, Is.EqualTo("S").Or.EqualTo("A"));
        }

        [UnityTest]
        public IEnumerator StoryMission_BriefingIsPaused_IdleRunsOutOfAir_RetryStartsFresh()
        {
            Flow.StartLevel("L2");
            yield return SkipStory(Flow.GameScene);
            var game = Game();
            var run = game.Run;
            Assert.AreEqual(RunPhase.Briefing, run.Phase);
            for (int i = 0; i < 10; i++) yield return null;
            Assert.AreEqual(0.0, run.State.timeHours, "nothing moves before Start mission");
            game.PressModal(0);
            Assert.AreEqual(RunPhase.Running, run.Phase);

            yield return Play(game);
            Assert.IsTrue(run.Ended);
            Assert.IsTrue(game.EndShowing);
            Assert.AreEqual("NoAir", game.EndVariant);
            game.PressEnd("Try again");

            yield return WaitFor(() => Game() != null && Game() != game);
            yield return null;
            var again = Game();
            Assert.AreEqual(RunPhase.Briefing, again.Run.Phase);
            Assert.AreEqual(0.0, again.Run.State.timeHours);
            Assert.AreEqual(0, again.Run.State.buildings.Count, "retry starts from the intended base, not the stopped one");
        }

        [UnityTest]
        public IEnumerator Onboarding_Tutorial_TeacherLesson_MeetTheCrew_ThenTheOutpost()
        {
            foreach (var id in new[] { "L1", "T2", "L2" }) PlayerPrefs.DeleteKey("mapmakers.passed." + id);
            foreach (var lesson in HelpData.Load().lessons) PlayerPrefs.DeleteKey("mapmakers.lesson." + lesson.id);
            Progress.SuggestionsOn = true;

            Flow.StartLevel("L1");
            yield return SkipStory(Flow.GameScene);
            var game = Game();
            CloseWindows(game);
            var run = game.Run;
            run.HintsOn = true;
            yield return null;
            yield return null;
            Assert.IsTrue(game.CoachShowing, "the pointer shows a new player where to start");
            StringAssert.Contains("Habitat", game.CoachMessage);
            run.Choose(BuildingType.Habitat);
            game.ClickTile(2, 2);
            run.Choose(BuildingType.SolarArray);
            yield return null;
            Assert.Greater(game.SuggestedCount, 0, "bright ground glows for the Solar Array");
            game.ClickTile(3, 10);
            run.Choose(BuildingType.Greenhouse);
            game.ClickTile(9, 3);
            run.Choose(BuildingType.SolarArray);
            game.ClickTile(1, 13);
            Assert.IsTrue(run.DrawingReady);
            game.PressFinish();
            yield return WaitForScene(Flow.ResultsScene);
            Flow.Continue();

            yield return NextLesson("lesson_teacher");
            var teacher = Object.FindAnyObjectByType<LessonScreen>();
            Assert.IsFalse(teacher.CanSkip, "the first teacher lesson is required");
            for (int i = 0; i < 30 && !teacher.Done; i++) teacher.Next();
            yield return WaitForScene(Flow.MenuScene);
            Assert.IsTrue(Progress.LessonSeen("lesson_teacher"));
            Assert.IsTrue(Progress.IsUnlocked("T2"));
            Assert.IsFalse(Progress.IsUnlocked("L2"), "the crew comes before the outpost");

            Flow.StartLevel("T2");
            yield return WaitForScene(Flow.CutsceneScene);
            Assert.AreEqual("c_years", Flow.CutsceneId, "the time jump leads into the space agency");
            Object.FindAnyObjectByType<CutsceneScreen>().Leave();
            yield return NextLesson("lesson_crew");
            var crew = Object.FindAnyObjectByType<LessonScreen>();
            Assert.IsFalse(crew.CanSkip, "meeting the crew is required the first time");
            for (int i = 0; i < 30 && !crew.Done; i++) crew.Next();
            yield return WaitForScene(Flow.MenuScene);
            Assert.IsTrue(Progress.Passed("T2"));
            Assert.IsTrue(Progress.IsUnlocked("L2"));

            Flow.StartLevel("T2");
            yield return WaitForScene(Flow.CutsceneScene);
            Object.FindAnyObjectByType<CutsceneScreen>().Leave();
            yield return NextLesson("lesson_crew");
            var replay = Object.FindAnyObjectByType<LessonScreen>();
            Assert.IsTrue(replay.CanSkip, "a seen lesson can be skipped");
            replay.Finish();
            yield return WaitForScene(Flow.MenuScene);

            Flow.StartLevel("L2");
            yield return WaitForScene(Flow.CutsceneScene);
            Assert.AreEqual("c_launch", Flow.CutsceneId, "seen lessons are not forced again");
            yield return SkipStory(Flow.GameScene);
            Assert.IsNotNull(Game());
        }

        [UnityTest]
        public IEnumerator Hud_WikiPauses_SpeedCycles_SuggestionsAndObjectivesUpdate()
        {
            Progress.SuggestionsOn = true;
            yield return StartFree();
            var game = Game();
            CloseWindows(game);
            var run = game.Run;
            run.StartMission();

            Assert.AreEqual(1, run.clock.Speed);
            game.CycleSpeed();
            Assert.AreEqual(2, run.clock.Speed);
            game.CycleSpeed();
            Assert.AreEqual(4, run.clock.Speed);
            game.CycleSpeed();
            Assert.AreEqual(1, run.clock.Speed);
            game.TogglePause();
            Assert.IsTrue(run.clock.Paused);
            game.CycleSpeed();
            Assert.IsTrue(run.clock.Paused, "changing speed keeps a pause");
            game.TogglePause();
            Assert.IsFalse(run.clock.Paused);

            game.OpenWiki("b_Battery");
            Assert.IsTrue(Wiki.IsOpen);
            Assert.AreEqual("b_Battery", Wiki.Page);
            double before = run.State.timeHours;
            run.Tick(1);
            Assert.AreEqual(before, run.State.timeHours, "the mission waits while the Wiki is open");
            Wiki.Show("e_storm");
            Assert.AreEqual("e_storm", Wiki.Page);
            Wiki.Close();
            Assert.IsFalse(run.clock.Paused, "closing the Wiki resumes");

            run.State.supplies = 500;
            run.State.soil = 12;
            Assert.IsTrue(Planner.Place(run.sim, run.State, BuildingType.Habitat, true));
            Assert.IsTrue(Planner.Place(run.sim, run.State, BuildingType.SolarArray, true));
            run.Choose(BuildingType.Battery);
            yield return null;
            Assert.Greater(game.SuggestedCount, 0, "a Battery gets suggested spots next to the Solar Array");
            run.Deselect();

            int messages = game.MessageCount;
            foreach (var type in Simulator.CoreTypes)
                if (run.State.Count(type) == 0) Planner.Place(run.sim, run.State, type, true);
            Assert.IsTrue(run.GoalMet("core_base"));
            run.Deselect();
            yield return null;
            Assert.Greater(game.MessageCount, messages, "a finished objective is announced in Information");
        }

        [UnityTest]
        public IEnumerator EachFailure_HasItsOwnScene_FromARealTrigger()
        {
            var cases = new (FailReason reason, Action<MissionRun> setup)[]
            {
                (FailReason.PowerLoss, run =>
                {
                    run.State.supplies = 500;
                    run.sim.Place(run.State, BuildingType.Habitat, 6, 5);
                    run.sim.Place(run.State, BuildingType.LifeSupport, 9, 5);
                }),
                (FailReason.NoWater, run =>
                {
                    StarterBase.Build(run.sim, run.State);
                    run.State.waterL = 1;
                    run.sim.SetEnabled(run.State, run.State.First(BuildingType.RoverGarage).id, false);
                }),
                (FailReason.NoFood, run =>
                {
                    StarterBase.Build(run.sim, run.State);
                    run.State.meals = 0.5;
                    run.sim.SetEnabled(run.State, run.State.First(BuildingType.Greenhouse).id, false);
                }),
                (FailReason.RadiationNoShelter, run =>
                {
                    StarterBase.Build(run.sim, run.State);
                    run.sim.StartStorm(run.State);
                    run.State.geologist = CrewLocation.Inside;
                }),
                (FailReason.RadiationOutside, run =>
                {
                    StarterBase.Build(run.sim, run.State);
                    run.State.shelterUnlocked = true;
                    run.sim.StartStorm(run.State);
                    run.State.recallIssued = true;
                }),
            };
            foreach (var (reason, setup) in cases)
            {
                yield return StartFree();
                var game = Game();
                CloseWindows(game);
                game.Run.StartMission();
                setup(game.Run);
                if (reason == FailReason.RadiationOutside) PlaceAndPowerShelter(game.Run);
                yield return Play(game, 80);
                Assert.IsTrue(game.EndShowing, reason + " did not end the run");
                Assert.AreEqual(reason.ToString(), game.EndVariant);
                Assert.IsFalse(string.IsNullOrEmpty(game.Run.Result.cause), reason + " needs an explanation");
                game.SkipEndBeat();
                Assert.IsTrue(game.EndBeatDone, "the animation can be skipped at once");
            }
        }

        [UnityTest]
        public IEnumerator FreeBuild_CarefulPlay_ReachesTheEvaluation_ThenTheReport()
        {
            yield return StartFree();
            var game = Game();
            CloseWindows(game);
            game.Run.StartMission();
            var bot = new SmartBot();
            yield return Play(game, 200, run => bot.Act(run.sim, run.State));
            Assert.IsTrue(game.EndShowing);
            Assert.AreEqual("Evaluation", game.EndVariant);
            Assert.AreEqual(MissionEnd.Deadline, game.Run.Result.end);
            game.PressEnd("View report");

            yield return WaitForScene(Flow.ResultsScene);
            var results = Object.FindAnyObjectByType<ResultsScreen>();
            Assert.AreEqual(GameMode.Free, results.Result.mode);
            Assert.IsTrue(results.Result.evaluation);
            Assert.IsFalse(results.Result.Failed);
        }

        [UnityTest]
        public IEnumerator IncompleteBase_GetsAnEvaluation_NotAFailureScene()
        {
            yield return StartFree(12);
            var game = Game();
            CloseWindows(game);
            var run = game.Run;
            run.StartMission();
            run.State.supplies = 500;
            run.sim.Place(run.State, BuildingType.Habitat, 6, 5);
            yield return Play(game, 30, r =>
            {
                if (r.State.Count(BuildingType.LifeSupport) == 0) Planner.Place(r.sim, r.State, BuildingType.LifeSupport, true);
                if (r.State.Count(BuildingType.Battery) == 0) Planner.Place(r.sim, r.State, BuildingType.Battery, true);
                while (r.State.Count(BuildingType.SolarArray) < 4 && Planner.Place(r.sim, r.State, BuildingType.SolarArray, true)) { }
            });
            Assert.AreEqual("Evaluation", game.EndVariant, "a low grade is still an evaluation");
            Assert.LessOrEqual(run.Result.score.total, run.sim.balance.grade.missingCoreCap);
            Assert.IsFalse(run.Result.Failed);
        }
    }
}
