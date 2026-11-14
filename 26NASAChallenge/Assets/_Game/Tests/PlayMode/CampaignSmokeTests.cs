using System.Collections;
using Mapmakers.Game;
using Mapmakers.Sim;
using Mapmakers.UI;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Mapmakers.Tests
{
    public class CampaignSmokeTests
    {
        [UnityTest]
        public IEnumerator Boot_TutorialPhotosOutpostStormDebriefEndingBlueprint_InOrder()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Boot.unity");
            for(int i=0;i<300 && SceneManager.GetActiveScene().name!="Prologue";i++)yield return null;
            yield return null;
            Assert.AreEqual("Prologue",SceneManager.GetActiveScene().name);
            var game=Object.FindAnyObjectByType<MissionController>();var view=Object.FindAnyObjectByType<MissionView>();
            game.AutoTick=false;
            Assert.AreEqual(MissionController.Chapter.Intro,game.Phase);
            game.Advance(10000);Assert.AreEqual(0,game.Session.State.timeHours);
            for(int i=0;i<6;i++){Assert.IsTrue(game.CanContinue);view.ContinueButton.onClick.Invoke();}
            Assert.AreEqual(MissionController.Chapter.Drawing,game.Phase);
            Assert.IsFalse(game.Place(BuildingType.Greenhouse,0,0).Ok,"Tutorial order must be enforced.");
            foreach(var type in new[]{BuildingType.Habitat,BuildingType.SolarArray,BuildingType.Greenhouse})
            {
                Assert.IsFalse(game.CanContinue,"Cannot skip the current placement task.");
                MissionSmokeTests.Place(game,view,type);
                game.Advance(10000);
            }
            Assert.AreEqual(0,game.Session.State.timeHours);
            Assert.AreEqual(20,game.Session.State.meals);
            Assert.Greater(game.Session.State.report.generationKW,0);
            Assert.Greater(game.Session.State.report.demandKW,game.Session.State.report.generationKW);
            Assert.AreEqual(MissionController.Chapter.Teacher,game.Phase);
            view.ContinueButton.onClick.Invoke();view.ContinueButton.onClick.Invoke();
            Assert.AreEqual(MissionController.Chapter.Photos,game.Phase);
            for(int i=0;i<3;i++){Assert.AreEqual(i,game.StoryIndex);if(i==2)StringAssert.Contains("not a promise",game.StoryText);view.ContinueButton.onClick.Invoke();}
            Assert.AreEqual(MissionController.Chapter.Departure,game.Phase);
            view.ContinueButton.onClick.Invoke();
            Assert.IsFalse(game.CanContinue,"Double clicks must not start a second scene transition.");
            game.ContinueStory();
            for(int i=0;i<300 && SceneManager.GetActiveScene().name!="Main";i++)yield return null;
            yield return null;
            game=Object.FindAnyObjectByType<MissionController>();view=Object.FindAnyObjectByType<MissionView>();game.AutoTick=false;
            Assert.AreEqual(MissionController.Chapter.Commander,game.Phase);
            Assert.AreEqual(3,game.Drawing.Count);
            Assert.AreEqual(150,game.Session.State.supplies);Assert.AreEqual(0,game.Session.State.buildings.Count);
            Assert.IsTrue(game.Session.Simulator.missionEnabled);
            game.Advance(10000);Assert.AreEqual(0,game.Session.State.timeHours);
            for(int i=0;i<4;i++)view.ContinueButton.onClick.Invoke();
            Assert.AreEqual(MissionController.Chapter.Outpost,game.Phase);
            var bot=new ShedLoadsBot();bool storm=false,sunrise=false;
            for(int i=0;i<1600&&!game.Session.Finished;i++)
            {
                bot.Act(game.Session.Simulator,game.Session.State);game.Advance(.25);
                storm |= game.Session.State.stormPhase!=StormPhase.None;
                sunrise |= game.Session.State.stage==MissionStage.Sunrise;
                if(i%80==0)yield return null;
            }
            yield return null;
            Assert.IsTrue(storm);Assert.IsTrue(sunrise);
            Assert.AreEqual(OutcomeKind.Win,game.Result.kind);
            Assert.AreEqual(MissionController.Chapter.Debrief,game.Phase);
            game.Advance(10000);Assert.AreEqual(MissionController.Chapter.Debrief,game.Phase);
            view.ContinueButton.onClick.Invoke();
            Assert.AreEqual(MissionController.Chapter.Ending,game.Phase);
            for(int i=0;i<6;i++){Assert.IsNotEmpty(game.StoryText);view.ContinueButton.onClick.Invoke();}
            Assert.AreEqual(MissionController.Chapter.Blueprint,game.Phase);
            StringAssert.Contains("Blueprint v2",game.StoryText);
            Assert.IsFalse(game.CanContinue);
            game.ResetMission();
            Assert.AreEqual(MissionController.Chapter.Outpost,game.Phase);
            Assert.AreEqual(3,game.Drawing.Count,"Retry must preserve the childhood blueprint, not its spent construction supplies.");
            Assert.AreEqual(150,game.Session.State.supplies);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Prologue_ResetRestartsTutorial_FailureCannotEnterEnding()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Prologue.unity");yield return null;
            var game=Object.FindAnyObjectByType<MissionController>();game.AutoTick=false;
            for(int i=0;i<6;i++)game.ContinueStory();
            game.ResetMission();Assert.AreEqual(MissionController.Chapter.Intro,game.Phase);Assert.AreEqual(0,game.TutorialPlaced);
            yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Main.unity");yield return null;
            game=Object.FindAnyObjectByType<MissionController>();game.AutoTick=false;game.ResetMission();game.Advance(200);
            Assert.IsTrue(game.Session.State.failed);Assert.AreEqual(MissionController.Chapter.Debrief,game.Phase);
            Assert.IsFalse(game.CanContinue);game.ContinueStory();Assert.AreEqual(MissionController.Chapter.Debrief,game.Phase);
            game.ResetMission();Assert.AreEqual(MissionController.Chapter.Outpost,game.Phase);
            LogAssert.NoUnexpectedReceived();
        }
    }
}
