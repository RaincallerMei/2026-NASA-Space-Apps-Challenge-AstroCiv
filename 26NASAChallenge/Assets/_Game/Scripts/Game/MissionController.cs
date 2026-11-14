using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Sim;
using UnityEngine;

namespace Mapmakers.Game
{
    public sealed class MissionController : MonoBehaviour
    {
        public enum Chapter { Intro, Drawing, Teacher, Photos, Departure, Commander, Outpost, Debrief, Ending, Blueprint }
        public bool StartWithTutorial;
        public Chapter Phase { get; private set; }
        public int StoryIndex { get; private set; }
        public int TutorialPlaced { get; private set; }
        public bool HintsEnabled = true;
        public string Hint { get; private set; } = "";
        readonly HashSet<string> seenHints = new HashSet<string>();
        public readonly List<Vector2Int> Drawing = new List<Vector2Int>();
        static List<Vector2Int> pendingDrawing;
        bool transitionLoading;
        static readonly BuildingType[] tutorialTypes = { BuildingType.Habitat, BuildingType.SolarArray, BuildingType.Greenhouse };
        static readonly string[] intro = { "p_night", "p_event", "p_hadfield", "p_seager", "p_wonder", "p_draw" };
        static readonly string[] teacher = { "p_teacher1", "p_teacher2" };
        static readonly string[] photos = { "p_card1", "p_card2", "p_card3" };
        static readonly string[] commander = { "m_drawing", "m_commander", "m_class", "m_air" };
        static readonly string[] ending = { "e_window", "e_sun", "e_earth", "e_photo", "e_msg1", "e_msg2" };
        StoryData story;
        public bool IsDrawing => Phase == Chapter.Drawing;
        public bool CanContinue => !transitionLoading && Phase != Chapter.Drawing && Phase != Chapter.Outpost && Phase != Chapter.Blueprint && !(Phase == Chapter.Debrief && Session.State.failed);
        public string StoryText
        {
            get
            {
                if (Phase == Chapter.Intro) return story.Story(intro[StoryIndex]);
                if (Phase == Chapter.Drawing) return story.Story(new[] { "p_task_habitat", "p_task_solar", "p_task_greenhouse" }[TutorialPlaced]);
                if (Phase == Chapter.Teacher) return story.Story(teacher[StoryIndex]);
                if (Phase == Chapter.Photos) return story.Story(photos[StoryIndex]);
                if (Phase == Chapter.Departure) return story.Story("p_jump");
                if (Phase == Chapter.Commander) return story.Story(commander[StoryIndex]);
                if (Phase == Chapter.Ending) return story.Story(ending[StoryIndex]);
                if (Phase == Chapter.Blueprint) return story.Story("e_kid") + "\n" + story.Debrief("blueprint");
                return "";
            }
        }
        public GameSession Session { get; private set; }
        public bool AutoTick = true;
        public string Message { get; private set; } = "";
        public int SelectedId { get; private set; }
        public BuildingType? BuildChoice { get; private set; }
        public int Revision { get; private set; }
        public bool DebriefVisible => Session.Finished;
        public Outcome Result => DebriefVisible ? Outcome.Evaluate(Session.Simulator.balance, Session.State) : null;

        void Awake()
        {
            ResetMission();
            if (!StartWithTutorial && pendingDrawing != null)
            {
                Drawing.AddRange(pendingDrawing); pendingDrawing = null;
                Phase = Chapter.Commander; Revision++;
            }
        }
        void Update() { if (AutoTick) Advance(Time.unscaledDeltaTime); }
        public void Advance(double seconds)
        {
            if (Phase != Chapter.Outpost) return;
            Session.Tick(seconds);
            if (Session.Finished) { Phase = Chapter.Debrief; StoryIndex = 0; Revision++; }
        }
        public void SetSpeed(int speed) { Session.SetSpeed(speed); ShowHint("h_speed"); Revision++; }
        public void ResetMission()
        {
            story = StoryData.Load();
            var balance = BalanceLoader.Load();
            Session = new GameSession(new Simulator(balance, TerrainFactory.Load(balance)));
            Phase = StartWithTutorial ? Chapter.Intro : Chapter.Outpost;
            Session.Simulator.missionEnabled = !StartWithTutorial;
            StoryIndex = 0; TutorialPlaced = 0; transitionLoading = false; if (StartWithTutorial) Drawing.Clear(); seenHints.Clear(); Hint = "";
            SelectedId = 0; BuildChoice = null; Message = "Pick a building. Then click the map."; Revision++;
        }
        public void ContinueStory()
        {
            if (!CanContinue) return;
            int length = Phase == Chapter.Intro ? intro.Length : Phase == Chapter.Teacher ? teacher.Length : Phase == Chapter.Photos ? photos.Length : Phase == Chapter.Commander ? commander.Length : Phase == Chapter.Ending ? ending.Length : 1;
            if (++StoryIndex < length) { Revision++; return; }
            StoryIndex = 0;
            switch (Phase)
            {
                case Chapter.Intro: Phase = Chapter.Drawing; ChooseBuilding(tutorialTypes[0]); ShowHint("h_build"); break;
                case Chapter.Teacher: Phase = Chapter.Photos; break;
                case Chapter.Photos: Phase = Chapter.Departure; break;
                case Chapter.Departure:
                    transitionLoading = true;
                    pendingDrawing = new List<Vector2Int>(Drawing);
                    UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Main.unity"); break;
                case Chapter.Commander: Phase = Chapter.Outpost; ShowHint("h_build"); break;
                case Chapter.Debrief: Phase = Chapter.Ending; break;
                case Chapter.Ending: Phase = Chapter.Blueprint; break;
            }
            Revision++;
        }
        void ShowHint(string id) { if (seenHints.Add(id)) Hint = story.Hint(id); }
        public void ReplayTutorial() { if (transitionLoading) return; transitionLoading = true; pendingDrawing = null; UnityEngine.SceneManagement.SceneManager.LoadSceneAsync("Assets/_Game/Scenes/Prologue.unity"); }
        public bool CanChooseBuilding(BuildingType type) => Phase == Chapter.Outpost || (IsDrawing && type == tutorialTypes[TutorialPlaced]);
        public void ChooseBuilding(BuildingType? type) { if (type.HasValue && !CanChooseBuilding(type.Value)) return; if (IsDrawing && !type.HasValue) return; BuildChoice = type; Message = type.HasValue ? "Click the map to place " + Session.Simulator.balance.Def(type.Value).displayName + "." : "Click a building to inspect it."; if (type == BuildingType.SolarArray) ShowHint("h_sun"); if (!type.HasValue) ShowHint("h_info"); Revision++; }
        public PlacementResult Place(BuildingType type, int x, int y)
        {
            if (Phase != Chapter.Outpost && !IsDrawing) return PlacementResult.Fail(PlacementReason.Locked, "Continue the story first.");
            if (IsDrawing && type != tutorialTypes[TutorialPlaced]) return PlacementResult.Fail(PlacementReason.Locked, StoryText);
            if (Session.Finished) return PlacementResult.Fail(PlacementReason.Locked, "Mission finished. Choose Retry.");
            var result = Session.Simulator.Place(Session.State, type, x, y);
            Message = result.Ok ? Session.Simulator.balance.Def(type).displayName + " placed." : result.message;
            if (result.Ok)
            {
                SelectedId = result.building.id;
                if (IsDrawing)
                {
                    Drawing.Add(new Vector2Int(x,y)); TutorialPlaced++;
                    // A drawing has no crew/resource countdown. Step a clone only to preview power.
                    Session.State.report = Session.Simulator.Step(Session.State).report;
                    ShowHint("h_power");
                    if (TutorialPlaced == tutorialTypes.Length) { Phase = Chapter.Teacher; BuildChoice = null; StoryIndex = 0; }
                    else ChooseBuilding(tutorialTypes[TutorialPlaced]);
                }
            }
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
            if (Phase != Chapter.Outpost) return false;
            ShowHint("h_toggle");
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
