using System;
using System.Collections;
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
    public class FlowSmokeTests
    {
        static IEnumerator WaitFor(Func<bool> condition, float seconds = 10)
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

        static void CloseWindows(GameScreen game)
        {
            for (int i = 0; i < 20 && game.ModalOpen; i++) game.PressModal(0);
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
        public IEnumerator Tutorial_CutsceneThenDrawingThenGrade()
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

            run.Choose(BuildingType.Habitat);
            game.ClickTile(2, 2);
            run.Choose(BuildingType.SolarArray);
            game.ClickTile(3, 10);
            run.Choose(BuildingType.Greenhouse);
            game.ClickTile(9, 3);
            Assert.IsFalse(run.DrawingReady, "one Solar Array cannot power the drawing");
            game.PressNextDay();
            Assert.AreEqual(Flow.GameScene, SceneManager.GetActiveScene().name, "Finish waits for the goals");
            run.Choose(BuildingType.SolarArray);
            game.ClickTile(1, 13);
            Assert.IsTrue(run.DrawingReady);
            game.PressNextDay();

            yield return WaitForScene(Flow.ResultsScene);
            var results = Object.FindAnyObjectByType<ResultsScreen>();
            Assert.IsTrue(results.Result.score.passed);
            Assert.That(results.Result.score.letter, Is.EqualTo("S").Or.EqualTo("A"));
        }

        [UnityTest]
        public IEnumerator FreeBuild_PlaysTenDays_ToAGrade()
        {
            Flow.StartFree(new FreeSettings { planet = "Mars", seed = 11, difficulty = Difficulty.Normal });
            yield return WaitForScene(Flow.GameScene);
            var game = Game();
            CloseWindows(game);
            var run = game.Run;
            Assert.AreEqual("Sol", run.Planet.dayWord);
            StarterBase.Build(run.sim, run.State);
            var bot = new SmartBot();

            for (int day = 0; day < 12 && !run.Ended; day++)
            {
                CloseWindows(game);
                bot.Act(run.sim, run.State);
                game.PressNextDay();
                yield return null;
            }

            yield return WaitForScene(Flow.ResultsScene);
            var results = Object.FindAnyObjectByType<ResultsScreen>();
            Assert.AreEqual(GameMode.Free, results.Result.mode);
            Assert.IsTrue(results.Result.score.passed, "a careful player survives ten sols");
        }
    }
}
