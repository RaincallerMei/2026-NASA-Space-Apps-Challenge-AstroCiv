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

        static UnityEngine.UI.Button ButtonLabelled(string text) =>
            Array.Find(Object.FindObjectsByType<UnityEngine.UI.Button>(), b => UIKit.LabelOf(b) != null && UIKit.LabelOf(b).text == text);

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
            yield return "mapmakers.music";
            yield return "mapmakers.sounds";
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
            SettingsWindow.Close();
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

        static void CloseWindows(GameScreen game)
        {
            for (int i = 0; i < 20 && game.ModalOpen; i++) game.PressModal(0);
        }

        // Runs the mission at 4x until it ends (or `hours` pass), letting frames render now and then.
        static IEnumerator Play(GameScreen game, double hours = 400, Action<MissionRun> everyHour = null)
        {
            var run = game.Run;
            run.clock.SetPaused(false);
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
        public IEnumerator Audio_MusicFollowsTheScene_StoryGoesBack_PlayerSkipsSongs()
        {
            Progress.SoundsOn = true;
            Progress.MusicOn = true;
            SceneManager.LoadScene(Flow.MenuScene);
            yield return WaitForScene(Flow.MenuScene);
            var audio = AudioDirector.Instance;
            Assert.AreEqual(MusicMode.Menu, audio.Mode);
            Assert.IsNotNull(ButtonLabelled("Settings"), "the menu has a Settings button");

            Flow.StartLevel("L1");
            yield return WaitForScene(Flow.CutsceneScene);
            Assert.AreEqual(MusicMode.Cutscene, audio.Mode);
            var cutscene = Object.FindAnyObjectByType<CutsceneScreen>();
            Assert.IsNotNull(ButtonLabelled("Settings"), "story pictures have a Settings button");
            cutscene.Back();
            Assert.AreEqual(0, cutscene.Slide + cutscene.Line, "nothing comes before the first line");
            cutscene.Advance();
            cutscene.Advance();
            Assert.AreEqual(1, cutscene.Slide);
            yield return new WaitForSecondsRealtime(0.7f);
            cutscene.Back();
            Assert.AreEqual(0, cutscene.Slide, "back from a new picture returns to the one before");
            Assert.AreEqual(1, cutscene.Line, "and to its last line");
            yield return new WaitForSecondsRealtime(0.7f);
            cutscene.Back();
            Assert.AreEqual(0, cutscene.Line);
            cutscene.Leave();

            yield return WaitForScene(Flow.GameScene);
            Assert.AreEqual(MusicMode.Game, audio.Mode);
            yield return WaitFor(() => audio.Current != null && Array.IndexOf(AudioDirector.GameTracks, audio.Current) >= 0, 5);
            var first = audio.Current;
            audio.Next();
            yield return WaitFor(() => audio.Current != first, 5);
            audio.TogglePause();
            Assert.IsTrue(audio.UserPaused);
            audio.Previous();
            Assert.IsFalse(audio.UserPaused, "picking a song plays it");

            var game = Game();
            CloseWindows(game);
            game.Run.Choose(BuildingType.Habitat);
            game.ClickTile(2, 2);
            yield return null;
            Assert.AreEqual(Sfx.Place, audio.LastEffect, "placing a building has its own sound");
            game.OpenWiki();
            yield return null;
            Assert.AreEqual(Sfx.WindowOpen, audio.LastEffect, "the Wiki opens like every other window");
            Wiki.Close();
            yield return null;
            Assert.AreEqual(Sfx.WindowClose, audio.LastEffect, "and closes like every other window");
            CloseWindows(game);
            yield return new WaitForSecondsRealtime(0.1f);
            var menu =Array.Find(Object.FindObjectsByType<UnityEngine.UI.Button>(), b => UIKit.LabelOf(b) != null && UIKit.LabelOf(b).text == "Menu");
            menu.onClick.Invoke();
            Assert.IsTrue(game.ModalOpen, "the menu window opened");
            yield return null;
            Assert.AreEqual(Sfx.WindowOpen, audio.LastEffect, "the menu is a window too");
            game.CloseModal();
            yield return null;
            Assert.AreEqual(Sfx.WindowClose, audio.LastEffect);
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
            for (int i = 0; i < 10; i++) yield return null;
            Assert.AreEqual(0.0, run.State.timeHours, "time waits for the player's first Play");
            Assert.IsTrue(game.CoachShowing, "an arrow points at Play");
            StringAssert.Contains("Play", game.CoachMessage);
            game.TogglePause();
            Assert.IsFalse(run.clock.Paused, "Play starts time");
            yield return null;
            Assert.IsFalse(game.CoachShowing && game.CoachMessage.Contains("Play"), "the Play arrow goes away");

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
        public IEnumerator Onboarding_Tutorial_MeetTheCrew_ThenTheOutpost()
        {
            foreach (var id in new[] { "L1", "T2", "L2" }) PlayerPrefs.DeleteKey("mapmakers.passed." + id);
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

            yield return WaitForScene(Flow.MenuScene);
            Assert.IsNull(Object.FindAnyObjectByType<LessonScreen>(), "no text lesson after the drawing");
            Assert.IsTrue(Progress.IsUnlocked("T2"));
            Assert.IsFalse(Progress.IsUnlocked("L2"), "the crew comes before the outpost");

            Flow.StartLevel("T2");
            yield return WaitForScene(Flow.CutsceneScene);
            Assert.AreEqual("c_years", Flow.CutsceneId, "the time jump leads into the space agency");
            Object.FindAnyObjectByType<CutsceneScreen>().Leave();
            yield return WaitForScene(Flow.GameScene);
            yield return null;
            var training = Game();
            Assert.IsTrue(training.Run.IsTraining);
            CloseWindows(training);
            yield return PlayTraining(training);
            Assert.AreEqual("Training complete!", training.ModalTitle);
            Assert.IsTrue(Progress.Passed("T2"));
            Assert.IsTrue(Progress.IsUnlocked("L2"));
            training.PressModal(1);
            yield return WaitForScene(Flow.MenuScene);

            Flow.StartLevel("L2");
            yield return WaitForScene(Flow.CutsceneScene);
            Assert.AreEqual("c_launch", Flow.CutsceneId, "no lesson before the outpost");
            yield return SkipStory(Flow.GameScene);
            Assert.IsNotNull(Game());
        }

        // Follows the crew through the real screen: the banner and the arrow must show every step.
        static IEnumerator PlayTraining(GameScreen game)
        {
            var run = game.Run;
            var unseen = new List<string>();
            string step = "";
            bool guided = false;
            for (int frame = 0; frame < 4000 && !run.Training.Done; frame++)
            {
                if (run.Training.Current.id != step)
                {
                    if (step != "" && !guided) unseen.Add(step);
                    step = run.Training.Current.id;
                    guided = false;
                }
                guided |= game.GuideShowing && game.CoachShowing;
                if (guided) TrainingAct(game, run.Training.Current);
                for (int i = 0; i < 4; i++) run.Tick(0.25);
                yield return null;
                if (Wiki.IsOpen) Wiki.Close();
                for (int i = 0; i < 20 && game.ModalOpen && !run.Training.Done; i++) game.CloseModal();
            }
            Assert.IsTrue(run.Training.Done, "stuck at " + run.Training.Current?.id);
            Assert.IsEmpty(unseen, "steps without banner and arrow");
            Assert.IsFalse(run.Ended);
            yield return null;
        }

        static void TrainingAct(GameScreen game, TrainingStep step)
        {
            var run = game.Run;
            var parts = step.done.Split(':');
            switch (parts[0])
            {
                case "next": game.PressGuideNext(); return;
                case "chat": game.OpenChat(parts[1]); return;
                case "wiki": game.OpenWiki(); return;
                case "speed": game.CycleSpeed(); return;
                case "paused":
                case "halt":
                case "playing": game.TogglePause(); return;
                case "recall": run.Recall(); return;
                case "built": TrainingBuild(game, (BuildingType)Enum.Parse(typeof(BuildingType), parts[1])); return;
                case "power": TrainingBuild(game, BuildingType.SolarArray); return;
                case "select":
                case "off":
                case "on":
                    var b = run.State.First((BuildingType)Enum.Parse(typeof(BuildingType), parts[1]));
                    if (b == null) return;
                    if (parts[0] == "select") game.ClickTile(b.x, b.y);
                    else if (b.enabled == (parts[0] == "off")) run.Toggle(b.id);
                    return;
            }
        }

        static void TrainingBuild(GameScreen game, BuildingType type)
        {
            var run = game.Run;
            if (!run.CanBuild(type, out _)) return;
            int before = run.State.buildings.Count;
            run.Choose(type);
            var spots = Layout.Suggest(run.sim, run.State, type);
            var t = run.sim.terrain;
            for (int y = 0; y < t.height; y++)
                for (int x = 0; x < t.width; x++)
                    spots.Add((x, y));
            foreach (var (x, y) in spots)
            {
                game.ClickTile(x, y);
                if (run.State.buildings.Count > before) return;
            }
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
            Assert.IsTrue(run.clock.Paused, "time waits for Play");
            game.TogglePause();
            Assert.IsFalse(run.clock.Paused, "Play starts time");

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

            game.OpenSettings();
            Assert.IsTrue(SettingsWindow.IsOpen);
            before = run.State.timeHours;
            run.Tick(1);
            Assert.AreEqual(before, run.State.timeHours, "the mission waits while Settings is open");
            SettingsWindow.Close();
            Assert.IsTrue(game.ModalOpen, "closing Settings goes back to the Menu");
            game.CloseModal();
            Assert.IsFalse(run.clock.Paused, "closing the Menu resumes");

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

        // A resource that runs low is circled on the top bar with a pulsing ring and an arrow; a full one is not.
        [UnityTest]
        public IEnumerator LowResource_IsCircled_WithAnArrow()
        {
            yield return StartFree();
            var game = Game();
            CloseWindows(game);
            var run = game.Run;
            Assert.IsTrue(run.clock.Paused, "rates come from the preview, so this works with time stopped");
            yield return null;
            Assert.IsFalse(game.LowMarkOf("air").gameObject.activeSelf, "a full air reserve is not circled");
            run.State.airHours = 3;
            yield return WaitFor(() => game.LowMarkOf("air").gameObject.activeSelf, 2);
            Assert.AreEqual(2, game.LowMarkOf("air").Severity, "3 hours of air is danger");
            Assert.AreEqual("Air low!", game.LowMarkOf("air").Message);
            Assert.IsFalse(game.LowMarkOf("supplies").gameObject.activeSelf);
            run.State.airHours = run.sim.balance.criticalHours + 1;
            yield return WaitFor(() => game.LowMarkOf("air").Severity == 1, 2);
            run.State.airHours = 36;
            yield return WaitFor(() => !game.LowMarkOf("air").gameObject.activeSelf, 2);
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
