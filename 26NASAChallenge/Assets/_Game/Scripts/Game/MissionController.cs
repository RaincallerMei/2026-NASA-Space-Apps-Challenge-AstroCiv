using Mapmakers.Data;
using Mapmakers.Sim;
using UnityEngine;

namespace Mapmakers.Game
{
    public sealed class MissionController : MonoBehaviour
    {
        public GameSession Session { get; private set; }
        public bool AutoTick = true;
        public string Message { get; private set; } = "";
        public int SelectedId { get; private set; }
        public BuildingType? BuildChoice { get; private set; }
        public int Revision { get; private set; }
        public bool DebriefVisible => Session.Finished;
        public Outcome Result => DebriefVisible ? Outcome.Evaluate(Session.Simulator.balance, Session.State) : null;

        void Awake() { ResetMission(); }
        void Update() { if (AutoTick) Advance(Time.unscaledDeltaTime); }
        public void Advance(double seconds) { Session.Tick(seconds); }
        public void SetSpeed(int speed) { Session.SetSpeed(speed); Revision++; }
        public void ResetMission()
        {
            var balance = BalanceLoader.Load();
            Session = new GameSession(new Simulator(balance, TerrainFactory.Load(balance)));
            SelectedId = 0; BuildChoice = null; Message = "Pick a building. Then click the map."; Revision++;
        }
        public void ChooseBuilding(BuildingType? type) { BuildChoice = type; Message = type.HasValue ? "Click the map to place " + Session.Simulator.balance.Def(type.Value).displayName + "." : "Click a building to inspect it."; Revision++; }
        public PlacementResult Place(BuildingType type, int x, int y)
        {
            if (Session.Finished) return PlacementResult.Fail(PlacementReason.Locked, "Mission finished. Choose Retry.");
            var result = Session.Simulator.Place(Session.State, type, x, y);
            Message = result.Ok ? Session.Simulator.balance.Def(type).displayName + " placed." : result.message;
            if (result.Ok) SelectedId = result.building.id;
            Revision++; return result;
        }
        public void ClickTile(int x, int y)
        {
            if (BuildChoice.HasValue) { Place(BuildChoice.Value, x, y); return; }
            var building = Placement.BuildingAt(Session.Simulator.balance, Session.State, x, y);
            SelectedId = building == null ? 0 : building.id; Revision++;
        }
        public bool Toggle(int id)
        {
            if (Session.Finished) return false;
            var building = Session.State.Find(id);
            if (building == null) return false;
            bool ok = Session.Simulator.SetEnabled(Session.State, id, !building.enabled);
            Message = ok ? "System switched " + (building.enabled ? "on." : "off.") : "This critical system must stay on.";
            Revision++; return ok;
        }
        public bool Recall()
        {
            if (Session.Finished) return false;
            bool ok = Session.Simulator.CanRecall(Session.State, out var why) && Session.Simulator.RecallGeologist(Session.State);
            Message = ok ? "Yuki is on the way back." : why; Revision++; return ok;
        }
    }
}
