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
    public class MissionSmokeTests
    {
        [UnityTest]
        public IEnumerator Main_PlacementStormDebriefAndRetry()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Main.unity");
            var game=Object.FindAnyObjectByType<MissionController>();
            var view=Object.FindAnyObjectByType<MissionView>();
            Assert.IsNotNull(game); Assert.IsNotNull(view);
            game.AutoTick=false; game.ResetMission();
            yield return null;
            Place(game,view,BuildingType.Habitat);
            for(int i=0;i<4;i++) Place(game,view,BuildingType.SolarArray);
            foreach(var type in new[]{BuildingType.LifeSupport,BuildingType.Battery,BuildingType.Communications,BuildingType.Greenhouse})Place(game,view,type);
            Assert.IsTrue(Simulator.HasCoreBase(game.Session.State));
            game.ChooseBuilding(null);
            var habitat=game.Session.State.buildings.Find(b=>b.type==BuildingType.Habitat);
            view.TileButtons[habitat.x,habitat.y].onClick.Invoke();
            Assert.AreEqual(habitat.id,game.SelectedId);
            game.SetSpeed(0);game.Advance(10);Assert.AreEqual(0,game.Session.State.step);
            game.SetSpeed(4);game.Advance(0.25);Assert.AreEqual(0.2,game.Session.State.timeHours,1e-9);
            game.SetSpeed(1);
            bool recalled=false,sheltered=false;
            for(int i=0;i<1600&&!game.Session.Finished;i++)
            {
                var s=game.Session.State;
                if(s.stormPhase!=StormPhase.None&&s.stormPhase!=StormPhase.Over)
                {
                    if(!recalled&&s.geologist==CrewLocation.Outside)
                    {
                        yield return null;
                        Assert.IsTrue(view.RecallButton.interactable);
                        view.RecallButton.onClick.Invoke();
                        recalled=game.Session.State.recallIssued;
                    }
                    if(s.Count(BuildingType.Shelter)==0)Place(game,view,BuildingType.Shelter);
                    var shelter=s.buildings.Find(b=>b.type==BuildingType.Shelter);
                    if(s.StormRelHour>=game.Session.Simulator.balance.storm.onsetHour-0.5&&!shelter.enabled){Assert.IsTrue(game.Toggle(shelter.id));sheltered=true;}
                    var greenhouse=s.buildings.Find(b=>b.type==BuildingType.Greenhouse);
                    bool particles=s.stormPhase==StormPhase.Particles||s.stormPhase==StormPhase.Decay;
                    if(greenhouse.enabled==particles)Assert.IsTrue(game.Toggle(greenhouse.id));
                }
                game.Advance(0.25);
                if(i%80==0)yield return null;
            }
            yield return null;
            Assert.IsTrue(recalled);Assert.IsTrue(sheltered);
            Assert.AreEqual(MissionStage.Done,game.Session.State.stage);
            Assert.AreEqual(OutcomeKind.Win,game.Result.kind);
            Assert.IsTrue(game.DebriefVisible);
            view.RetryButton.onClick.Invoke();yield return null;
            Assert.IsFalse(game.DebriefVisible);Assert.AreEqual(0,game.Session.State.buildings.Count);
            Assert.AreEqual(150,game.Session.State.supplies);
            LogAssert.NoUnexpectedReceived();
        }

        [UnityTest]
        public IEnumerator Main_IdleFailureExplainsCauseAndRetryRestoresState()
        {
            yield return SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Main.unity");
            var game=Object.FindAnyObjectByType<MissionController>();game.AutoTick=false;game.ResetMission();
            game.Advance(200);yield return null;
            Assert.IsTrue(game.DebriefVisible);Assert.AreEqual(FailReason.NoAir,game.Result.failReason);
            Object.FindAnyObjectByType<MissionView>().RetryButton.onClick.Invoke();yield return null;
            Assert.AreEqual(MissionStage.Build,game.Session.State.stage);Assert.IsFalse(game.Session.State.failed);
            LogAssert.NoUnexpectedReceived();
        }

        static void Place(MissionController game,MissionView view,BuildingType type)
        {
            var sim=game.Session.Simulator;var s=game.Session.State;var def=sim.balance.Def(type);
            int bx=-1,by=-1;double best=-1;
            for(int y=0;y<sim.terrain.height;y++)for(int x=0;x<sim.terrain.width;x++)
            {
                if(!sim.CanPlace(s,type,x,y).Ok)continue;
                double sunlight=def.generationKW>0?sim.terrain.AverageSun(x,y,def.width,def.height):0;
                if(sunlight<=best)continue;best=sunlight;bx=x;by=y;
            }
            Assert.GreaterOrEqual(bx,0,"No valid site for "+type);
            game.ChooseBuilding(type);
            int before=s.buildings.Count;view.TileButtons[bx,by].onClick.Invoke();
            Assert.AreEqual(before+1,s.buildings.Count,game.Message);
        }
    }
}
