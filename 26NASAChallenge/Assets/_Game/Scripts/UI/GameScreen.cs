using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Map-first play screen (Civ-style). Top: resources and the time controls. Left: crew, the selected building
    // and the Wiki. Right: Information (alerts and messages) above Objectives. Bottom: Buildings.
    // Real time: Update feeds unscaled frame time to the run's clock; counters refresh a few times a second,
    // structural UI only when the run's Revision changes.
    public sealed partial class GameScreen : MonoBehaviour
    {
        public const int TopBar = 96;
        public const int BottomBar = 136;
        public const int Side = 420;
        const float CounterInterval = 0.2f;
        const int Gap = 16;

        MissionRun run;
        RectTransform root;
        StoryData story;
        int shownRevision = -1;
        float nextCounters;

        public MissionRun Run => run;

        sealed class Chip { public Text label, value, rate; public string name, id, detail; }
        readonly List<Chip> chips = new List<Chip>();
        Image hintBox, hintIcon;
        Text hintText;
        Text statusText;
        string status = "";
        bool statusIsError;
        CoachMark coach;
        Button wikiButton;

        void Start()
        {
            story = StoryData.Load();
            run = CreateRun();
            Build();
            ShowStartCard();
            Refresh();
        }

        static MissionRun CreateRun()
        {
            if (Flow.Mode == GameMode.Free) return MissionRun.ForFree(Flow.Free ?? new FreeSettings());
            return MissionRun.ForLevel(Flow.Level ?? LevelCatalog.Load().levels[0]);
        }

        void Build()
        {
            root = UIKit.Canvas("Game Canvas");
            var back = UIKit.Rect(root, "Background").gameObject.AddComponent<Image>();
            back.color = run.IsDrawing ? Palette.Hex("0E1F3D") : Palette.Space;
            back.rectTransform.Stretch();

            BuildMap();
            BuildTopBar();
            BuildCrew();
            BuildWikiButton();
            BuildInformation();
            BuildObjectives();
            BuildSelectionPanel();
            BuildBottomBar();
            BuildHint();
            BuildOverlays();
            coach = CoachMark.Create(root);
        }

        // ---------- top bar ----------

        void BuildTopBar()
        {
            var bar = UIKit.Panel(root, Palette.Ink, "Top Bar", false);
            bar.rectTransform.anchorMin = new Vector2(0, 1);
            bar.rectTransform.anchorMax = new Vector2(1, 1);
            bar.rectTransform.pivot = new Vector2(0.5f, 1);
            bar.rectTransform.anchoredPosition = Vector2.zero;
            bar.rectTransform.sizeDelta = new Vector2(0, TopBar);

            var menu = UIKit.Button(bar.transform, "Menu", OpenPause, Palette.PanelLight, Palette.Hull, 26);
            menu.GetComponent<RectTransform>().TopLeft(14, 16, 108, 64);

            string[] ids = run.IsDrawing ? new[] { "power", "power", "supplies" } : new[] { "power", "battery", "air", "water", "food", "soil", "crew", "supplies" };
            string[] names = run.IsDrawing ? new[] { "Power in", "Power out", "Supplies" } : new[] { "Power", "Battery", "Air", "Water", "Food", "Soil", "Crew safety", "Supplies" };
            for (int i = 0; i < ids.Length; i++)
            {
                var chip = new Chip { name = names[i], id = ids[i] };
                var box = UIKit.Panel(bar.transform, Palette.Alpha(Palette.Panel, 0.9f), "Chip " + names[i]);
                box.rectTransform.TopLeft(134 + i * 152, 8, 146, 80);
                UIKit.Icon(box.transform, ids[i], 40).TopLeftOf(8, 20);
                chip.label = UIKit.Label(box.transform, names[i], 20, Palette.Ice, TextAnchor.MiddleLeft, FontStyle.Bold);
                chip.label.rectTransform.TopLeft(54, 4, 90, 24);
                chip.label.resizeTextForBestFit = true;
                chip.label.resizeTextMinSize = 15;
                chip.label.resizeTextMaxSize = 20;
                chip.value = UIKit.Label(box.transform, "", 30, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
                chip.value.rectTransform.TopLeft(54, 26, 92, 32);
                chip.value.resizeTextForBestFit = true;
                chip.value.resizeTextMinSize = 20;
                chip.value.resizeTextMaxSize = 30;
                chip.rate = UIKit.Label(box.transform, "", 18, Palette.Ice, TextAnchor.MiddleLeft);
                chip.rate.rectTransform.TopLeft(54, 56, 92, 22);
                var c = chip;
                Tip.Add(box, () => HelpText.Resource(c.id, c.detail));
                chips.Add(chip);
            }
            if (run.IsDrawing)
            {
                UIKit.Label(bar.transform, "Your drawing", 30, Palette.Hull, TextAnchor.MiddleRight, FontStyle.Bold)
                    .rectTransform.Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, 0), new Vector2(400, 60));
                return;
            }
            BuildTimeGroup(bar.transform);
        }

        void RefreshTopBar(StepReport power)
        {
            var s = run.State;
            if (run.IsDrawing)
            {
                SetChip(0, Num(power.generationKW) + " kW", "", power.generationKW + 1e-6 >= power.demandKW && power.demandKW > 0 ? Palette.Mint : Palette.Hull,
                    "Power in: what your Solar Arrays make now.");
                SetChip(1, Num(power.demandKW) + " kW", "", power.demandKW > power.generationKW + 1e-6 ? Palette.Amber : Palette.Hull,
                    "Power out: what your buildings use now. Power in must beat power out.");
                SetChip(2, s.supplies.ToString(), "", Palette.Hull, "You have " + s.supplies + " supplies.");
                return;
            }
            var r = s.step > 0 ? s.report : power;
            var b = run.sim.balance;
            double net = r.generationKW - r.servedKW;
            SetChip(0, Signed(net) + " kW", "", net >= 0 ? Palette.Mint : Palette.Amber,
                "Now: Solar Arrays make " + Num(r.generationKW) + " kW and the base uses " + Num(r.servedKW) + " kW.");
            double cap = run.sim.BatteryCapacity(s);
            SetChip(1, cap > 0 ? Num(s.batteryKWh) + "/" + Num(cap) : "none", Rate(r.batteryFlowKW),
                cap > 0 && s.batteryKWh < cap * 0.25 ? Palette.Amber : Palette.Hull,
                cap > 0 ? "Stored: " + Num(s.batteryKWh) + " of " + Num(cap) + " kWh." : "No Battery yet.");
            SetChip(2, Num(s.airHours) + " h", Rate(r.airPerHour), Level(s.airHours, r.airPerHour), "Now: " + Num(s.airHours) + " hours of air left.");
            SetChip(3, Num(s.waterL) + " L", Rate(r.waterPerHour), Level(s.waterL, r.waterPerHour),
                "Now: " + Num(s.waterL) + " L. The crew drinks " + Num(b.CrewWaterPerHour) + " L every hour.");
            SetChip(4, Num(s.meals), Rate(r.foodPerHour), Level(s.meals, r.foodPerHour), "Now: " + Num(s.meals) + " meals left.");
            SetChip(5, Num(s.soil), r.soilPerHour > 0 ? Rate(r.soilPerHour) : "", Palette.Hull,
                "Now: " + Num(s.soil) + " bags. The Greenhouse needs " + b.Def(BuildingType.Greenhouse).soilCost + ".");
            SetChip(6, Num(s.safety) + "%", "", s.safety < 70 ? Palette.Danger : Palette.Mint, "Now: " + Num(s.safety) + "%.");
            SetChip(7, s.supplies.ToString(), "", Palette.Hull, "You have " + s.supplies + " supplies.");
        }

        Color Level(double amount, double rate)
        {
            double hours = MissionRun.HoursLeft(amount, rate);
            var b = run.sim.balance;
            return hours <= b.criticalHours ? Palette.Danger : hours <= b.warnHours ? Palette.Amber : Palette.Hull;
        }

        static string Signed(double v) => (v >= 0 ? "+" : "") + Num(v);
        static string Rate(double perHour) => System.Math.Abs(perHour) < 0.05 ? "" : Signed(perHour) + " /h";

        void SetChip(int i, string value, string rate, Color color, string detail)
        {
            chips[i].value.text = value;
            chips[i].value.color = color;
            chips[i].rate.text = rate;
            chips[i].detail = detail;
        }

        // ---------- time controls (top right) ----------

        Button playButton, speedButton;
        Image playIcon, evalFill;
        Text clockText, evalText;

        void BuildTimeGroup(Transform bar)
        {
            var group = UIKit.Panel(bar, Palette.Alpha(Palette.Panel, 0.9f), "Time Controls");
            group.rectTransform.Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-14, -8), new Vector2(548, 80));

            playButton = UIKit.Button(group.transform, "", TogglePause, Palette.Azure, Palette.Hull, 22);
            playButton.GetComponent<RectTransform>().TopLeft(8, 8, 64, 64);
            playIcon = UIKit.Icon(playButton.transform, "pause", 40);
            playIcon.rectTransform.anchorMin = playIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            playIcon.rectTransform.anchoredPosition = Vector2.zero;
            Tip.Add(playButton, () => new TipContent("Pause / Resume", "pause", "Stop time to think, then press again to go on. Key: Space."), false);

            speedButton = UIKit.Button(group.transform, "1×", CycleSpeed, Palette.PanelLight, Palette.Hull, 30);
            speedButton.GetComponent<RectTransform>().TopLeft(80, 8, 92, 64);
            Tip.Add(speedButton, () => new TipContent("Game speed", "clock", "Now " + run.clock.Speed + "×. Click to change: 1× → 2× → 4× → 1×."), false);

            clockText = UIKit.Label(group.transform, "", 26, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            clockText.rectTransform.TopLeft(184, 2, 356, 34);
            var flag = UIKit.Icon(group.transform, "flag", 28);
            flag.TopLeftOf(184, 38);
            evalText = UIKit.Label(group.transform, "", 22, Palette.Amber, TextAnchor.MiddleLeft, FontStyle.Bold);
            evalText.rectTransform.TopLeft(218, 36, 322, 30);
            var track = UIKit.Panel(group.transform, Palette.Space, "Track", false);
            track.rectTransform.TopLeft(184, 68, 350, 7);
            evalFill = UIKit.Panel(track.transform, Palette.Cyan, "Fill", false);
            evalFill.rectTransform.TopLeft(0, 0, 0, 7);
            var tipBox = UIKit.Rect(group.transform, "Inspection Tip").TopLeft(180, 36, 360, 42).gameObject.AddComponent<Image>();
            tipBox.color = new Color(0, 0, 0, 0);
            Tip.Add(tipBox, () => new TipContent("Inspection", "flag",
                "When the countdown ends, mission control inspects your base and gives a grade (S, A, B or C). Running out of time is not a failure."));
        }

        public void TogglePause()
        {
            if (run.IsDrawing || run.Ended || ModalOpen) return;
            if (run.Phase == RunPhase.Briefing) { run.StartMission(); RefreshTime(); return; }
            run.clock.TogglePause();
            RefreshTime();
        }

        // Plays at this speed (and resumes if the player had paused).
        public void SetSpeed(int speed)
        {
            if (run.Ended) return;
            run.clock.SetSpeed(speed);
            if (run.clock.UserPaused) run.clock.SetPaused(false);
            RefreshTime();
        }

        // The one speed button: 1x -> 2x -> 4x -> 1x. Changing speed never pauses or resumes.
        public void CycleSpeed()
        {
            if (run.Ended) return;
            var speeds = MissionClock.Speeds;
            int i = System.Array.IndexOf(speeds, run.clock.Speed);
            run.clock.SetSpeed(speeds[(i + 1) % speeds.Length]);
            RefreshTime();
        }

        // Countdown follows simulation time, so it stops while paused and speeds up at 2x and 4x.
        void RefreshTime()
        {
            if (run.IsDrawing || clockText == null) return;
            bool paused = run.clock.Paused;
            playIcon.sprite = Icons.Get(paused ? "play" : "pause");
            playButton.image.color = paused ? Palette.Amber : Palette.Azure;
            UIKit.LabelOf(speedButton).text = run.clock.Speed + "×";
            var s = run.State;
            double dayHours = run.Planet.dayHours;
            string clock = run.Planet.dayWord + " " + SimText.Day(s.timeHours + run.sim.balance.startClockHour, dayHours) + " · "
                + SimText.Clock(s.timeHours, run.sim.balance.startClockHour, dayHours);
            clockText.text = clock + (paused ? "   PAUSED" : "");
            clockText.color = paused ? Palette.Amber : Palette.Hull;
            evalText.text = run.Ended ? "Inspection now" : "Inspection in " + SimText.Duration(run.TimeLeft, run.Planet.dayWord, dayHours);
            float done = run.Duration > 0 ? Mathf.Clamp01((float)(s.timeHours / run.Duration)) : 0;
            evalFill.rectTransform.sizeDelta = new Vector2(350 * done, 7);
            evalFill.color = done > 0.85f ? Palette.Amber : Palette.Cyan;
        }

        // ---------- crew (left) ----------

        sealed class CrewRow { public string id; public Text name, role; public Image badge; public string seenWhen = ""; public string postedWhen; }
        readonly List<CrewRow> crewRows = new List<CrewRow>();
        RectTransform crewList;

        public static Color CrewColor(string id) => id == "engineer" ? Palette.Cyan : id == "botanist" ? Palette.Mint : Palette.Peach;
        static string CrewIcon(string id) => id == "engineer" ? "power" : id == "botanist" ? "Greenhouse" : "rover";

        float LeftTop => run.IsDrawing ? TopBar + Gap : TopBar + Gap + 32 + 3 * 72 + Gap;

        void BuildCrew()
        {
            if (run.IsDrawing) return;
            crewList = UIKit.Rect(root, "Crew").TopLeft(20, TopBar + Gap, Side, 32 + 3 * 72);
            UIKit.Label(crewList, "CREW · click to talk", 22, Palette.Ice, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.TopLeft(4, 0, Side, 28);
            for (int i = 0; i < story.crew.Length; i++)
            {
                var member = story.crew[i];
                var row = new CrewRow { id = member.id };
                string id = member.id;
                var button = UIKit.Button(crewList, "", () => OpenChat(id), Palette.Alpha(Palette.Panel, 0.95f));
                button.GetComponent<RectTransform>().TopLeft(0, 32 + i * 72, Side, 64);
                var face = UIKit.Rect(button.transform, "Face").gameObject.AddComponent<Image>();
                face.sprite = UIKit.CircleSprite;
                face.color = CrewColor(member.id);
                face.raycastTarget = false;
                face.rectTransform.TopLeft(10, 8, 48, 48);
                UIKit.Label(face.transform, member.name.Substring(0, 1), 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
                row.name = UIKit.Label(button.transform, member.name, 24, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
                row.name.rectTransform.TopLeft(72, 6, 200, 28);
                row.role = UIKit.Label(button.transform, member.role, 20, Palette.Ice);
                row.role.rectTransform.TopLeft(72, 34, 300, 24);
                UIKit.Icon(button.transform, CrewIcon(member.id), 30).rectTransform.Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-62, 0), new Vector2(30, 30));
                row.badge = UIKit.Rect(button.transform, "New").gameObject.AddComponent<Image>();
                row.badge.sprite = UIKit.CircleSprite;
                row.badge.color = Palette.Amber;
                row.badge.raycastTarget = false;
                row.badge.rectTransform.Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-16, 0), new Vector2(30, 30));
                UIKit.Label(row.badge.transform, "!", 22, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
                var m = member;
                var wiki = HelpData.Load().Page("c_" + member.id);
                Tip.Add(button, () => new TipContent(m.name + " · " + m.role, CrewIcon(m.id), wiki != null ? wiki.summary + "\n" + wiki.rows[wiki.rows.Length > 1 ? 1 : 0].text : ""), false);
                crewRows.Add(row);
            }
        }

        // A crew member with something new to say gets a badge and a message in the Information panel.
        void RefreshCrew()
        {
            foreach (var row in crewRows)
            {
                string when = CrewChat.When(row.id, run);
                row.badge.gameObject.SetActive(when != row.seenWhen);
                var member = story.Crew(row.id);
                bool outside = row.id == "geologist" && run.State.geologist != CrewLocation.Inside;
                row.role.text = outside ? (run.State.geologist == CrewLocation.Returning ? "Coming back..." : "Outside the base!") : member.role;
                row.role.color = outside ? Palette.Amber : Palette.Ice;
                if (row.postedWhen == null) { row.postedWhen = when; continue; }
                if (when == row.postedWhen) continue;
                row.postedWhen = when;
                var entry = story.Chat(row.id, when);
                if (entry != null && when != "default") Post(member.name + ": \"" + entry.line + "\"", CrewIcon(row.id), CrewColor(row.id));
            }
        }

        // ---------- wiki button (bottom left) ----------

        void BuildWikiButton()
        {
            wikiButton = UIKit.Button(root, "", () => OpenWiki(), Palette.Azure, Palette.Hull, 28);
            wikiButton.GetComponent<RectTransform>().Anchor(new Vector2(0, 0), new Vector2(0, 0), new Vector2(20, BottomBar + Gap), new Vector2(Side, 64));
            UIKit.Icon(wikiButton.transform, "info", 40).rectTransform.Anchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(40, 40));
            UIKit.Label(wikiButton.transform, "Wiki & Help  (H)", 28, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.Stretch(72, 0, 8, 0);
            Tip.Add(wikiButton, () => new TipContent("Wiki & Help", "info", "Every building, resource, crew member and event explained. The game pauses while it is open."), false);
        }

        public void OpenWiki(string page = null)
        {
            if (Wiki.IsOpen || EndShowing) return;
            run.clock.Hold();
            Wiki.Open(page ?? WikiPageForNow(), () => { run.clock.Release(); Refresh(); });
        }

        string WikiPageForNow()
        {
            if (run.BuildChoice.HasValue) return "b_" + run.BuildChoice.Value;
            if (run.Selected != null) return "b_" + run.Selected.type;
            if (run.State.StormActive) return "e_storm";
            return "start";
        }

        // ---------- information (right, top): alerts first, then the latest messages ----------

        sealed class Message { public string icon, text; public Color color; }
        readonly List<Message> messages = new List<Message>();
        RectTransform infoList;
        Image infoPanel;
        string shownInfo = "";
        float InfoHeight => 1080 - TopBar - Gap - (BottomBar + Gap + ObjectivesHeight) - Gap;

        public int MessageCount => messages.Count;

        void BuildInformation()
        {
            infoPanel = UIKit.Panel(root, Palette.Alpha(Palette.Panel, 0.95f), "Information");
            infoPanel.rectTransform.Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -(TopBar + Gap)), new Vector2(Side, InfoHeight));
            UIKit.Icon(infoPanel.transform, "alert", 30).TopLeftOf(14, 10);
            var infoTitle = UIKit.Label(infoPanel.transform, "INFORMATION", 24, Palette.Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            infoTitle.rectTransform.TopLeft(54, 8, 300, 34);
            Tip.Add(infoTitle, () => new TipContent("Information", "alert", "Warnings, crew news and messages for the commander. Newest at the top."), false);
            infoList = UIKit.Rect(infoPanel.transform, "List").TopLeft(10, 50, Side - 20, InfoHeight - 60);
        }

        public void Post(string text, string icon = "info", Color? color = null)
        {
            if (string.IsNullOrEmpty(text)) return;
            messages.Insert(0, new Message { text = text, icon = icon, color = color ?? Palette.Ice });
            if (messages.Count > 30) messages.RemoveAt(messages.Count - 1);
            if (infoPanel != null && !UIFx.Calm) UIFx.Pop(infoPanel.transform);
        }

        void RefreshInformation()
        {
            var alerts = run.Alerts();
            string key = messages.Count + "#";
            foreach (var a in alerts) key += a.level + a.text + a.actionEnabled + "|";
            if (key == shownInfo) return;
            shownInfo = key;
            UIKit.Clear(infoList);
            float width = Side - 20, y = 0, max = InfoHeight - 60;
            foreach (var alert in alerts)
            {
                bool hasAction = alert.action != null;
                var card = UIKit.Panel(infoList, Palette.Alpha(Palette.PanelLight, 0.96f), "Alert");
                float h = hasAction ? 128 : 84;
                card.rectTransform.TopLeft(0, y, width, h);
                var stripe = UIKit.Panel(card.transform, AlertColor(alert.level), "Stripe", false);
                stripe.rectTransform.TopLeft(0, 0, 10, h);
                if (alert.level == AlertLevel.Danger) stripe.gameObject.AddComponent<SoftPulse>();
                UIKit.Icon(card.transform, alert.icon, 32).TopLeftOf(22, 14);
                string mark = alert.level == AlertLevel.Danger ? "!! " : alert.level == AlertLevel.Warning ? "! " : "";
                UIKit.Label(card.transform, Keywords.Mark(mark + alert.text), 22, Palette.Hull, TextAnchor.UpperLeft).rectTransform.TopLeft(64, 10, width - 76, 66);
                if (hasAction)
                {
                    var action = alert.action;
                    var button = UIKit.Button(card.transform, alert.actionLabel, () => action(), alert.level == AlertLevel.Danger ? Palette.Danger : Palette.Azure, Palette.Hull, 22);
                    button.GetComponent<RectTransform>().TopLeft(64, 74, 250, 44);
                    button.interactable = alert.actionEnabled;
                }
                y += h + 8;
            }
            if (alerts.Count == 0 && messages.Count == 0)
                UIKit.Label(infoList, "All quiet. Messages from mission control and your crew will appear here.", 20, Palette.Muted, TextAnchor.UpperLeft)
                    .rectTransform.TopLeft(4, 0, width - 8, 60);
            foreach (var m in messages)
            {
                const float h = 62;
                if (y + h > max) break;
                var row = UIKit.Rect(infoList, "Message").TopLeft(0, y, width, h);
                UIKit.Icon(row, m.icon, 28).TopLeftOf(6, 6);
                UIKit.Label(row, Keywords.Mark(m.text), 20, m.color, TextAnchor.UpperLeft).rectTransform.TopLeft(44, 2, width - 48, h - 4);
                var line = UIKit.Panel(row, Palette.Alpha(Palette.Muted, 0.25f), "Line", false);
                line.rectTransform.TopLeft(44, h - 3, width - 48, 1);
                y += h;
            }
        }

        static Color AlertColor(AlertLevel level) => level == AlertLevel.Danger ? Palette.Danger : level == AlertLevel.Warning ? Palette.Amber : Palette.Cyan;

        // ---------- objectives (right, bottom): a checklist that ticks itself ----------

        sealed class ObjectiveRow { public string id; public Image box, check; public Text text; public bool met; }
        readonly List<ObjectiveRow> objectives = new List<ObjectiveRow>();
        Text objectivesTitle;
        const int ObjectiveRowHeight = 46;
        float ObjectivesHeight => 92 + Mathf.Max(3, run.goals.Length) * ObjectiveRowHeight;

        void BuildObjectives()
        {
            var panel = UIKit.Panel(root, Palette.Alpha(Palette.Panel, 0.95f), "Objectives");
            panel.rectTransform.Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, BottomBar + Gap), new Vector2(Side, ObjectivesHeight));
            UIKit.Icon(panel.transform, "check", 30).TopLeftOf(14, 10);
            objectivesTitle = UIKit.Label(panel.transform, "", 24, Palette.Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            objectivesTitle.rectTransform.TopLeft(54, 8, Side - 70, 34);
            Tip.Add(objectivesTitle, () => new TipContent("Objectives", "check", "This level's checklist. Each one gets a tick when it is done. Yellow box = do this next."), false);
            UIKit.Label(panel.transform, LevelCaption(), 18, Palette.Muted, TextAnchor.MiddleLeft).rectTransform.TopLeft(16, 44, Side - 32, 26);
            for (int i = 0; i < run.goals.Length; i++)
            {
                var row = new ObjectiveRow { id = run.goals[i] };
                var rt = UIKit.Rect(panel.transform, "Objective " + row.id).TopLeft(14, 78 + i * ObjectiveRowHeight, Side - 28, ObjectiveRowHeight - 4);
                row.box = UIKit.Panel(rt, Palette.Space, "Box");
                row.box.rectTransform.TopLeft(0, 6, 30, 30);
                row.check = UIKit.Icon(row.box.transform, "check", 26);
                row.check.rectTransform.anchorMin = row.check.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                row.check.rectTransform.anchoredPosition = Vector2.zero;
                row.text = UIKit.Label(rt, "", 21, Palette.Ice, TextAnchor.MiddleLeft);
                row.text.rectTransform.TopLeft(42, 0, Side - 72, ObjectiveRowHeight - 4);
                string id = row.id;
                if (id == "core_base") Tip.Add(row.box, () => new TipContent("Six base parts", "Habitat", CoreBaseDetail()));
                objectives.Add(row);
            }
        }

        string LevelCaption()
        {
            if (run.mode == GameMode.Free) return "FREE BUILD · " + run.Planet.name.ToUpperInvariant() + " · MAP #" + run.free.seed;
            return run.level.Label.ToUpperInvariant() + " · " + run.Title.ToUpperInvariant();
        }

        string CoreBaseDetail()
        {
            var missing = new List<string>();
            foreach (var type in Simulator.MissingCore(run.State)) missing.Add(run.sim.balance.Def(type).displayName);
            return missing.Count == 0 ? "All six are built." : "Still to build: " + string.Join(", ", missing) + ".";
        }

        void RefreshObjectives()
        {
            var catalog = LevelCatalog.Load();
            int met = 0;
            bool currentFound = false;
            foreach (var row in objectives)
            {
                bool now = run.GoalMet(row.id);
                if (now) met++;
                string label = catalog.GoalLabel(row.id);
                if (row.id == "core_base")
                    label += " (" + (Simulator.CoreTypes.Length - Simulator.MissingCore(run.State).Count) + "/" + Simulator.CoreTypes.Length + ")";
                bool current = !now && !currentFound && row.id != Goals.SafeToEnd;
                if (current) currentFound = true;
                row.text.text = Keywords.Mark(label);
                row.text.color = now ? Palette.Muted : current ? Palette.Hull : Palette.Ice;
                row.text.fontStyle = current ? FontStyle.Bold : FontStyle.Normal;
                row.box.color = now ? Palette.Mint : current ? Palette.Amber : Palette.Space;
                row.check.enabled = now;
                if (now && !row.met && shownRevision > 0)
                {
                    UIFx.Pop(row.box.transform);
                    Post("Objective done: " + catalog.GoalLabel(row.id), "check", Palette.Mint);
                }
                row.met = now;
            }
            objectivesTitle.text = "OBJECTIVES  " + met + "/" + objectives.Count;
        }

        // ---------- tips (yellow banner) ----------

        static readonly Dictionary<string, string> hintIcons = new Dictionary<string, string>
        {
            { "h_build", "build" }, { "h_habitat", "Habitat" }, { "h_zone", "Habitat" }, { "h_clock", "clock" }, { "h_sun", "SolarArray" },
            { "h_cable", "Battery" }, { "h_rover", "RoverGarage" }, { "h_power", "power" }, { "h_info", "info" }, { "h_toggle", "Greenhouse" },
            { "n_power", "power" }, { "h_suggest", "check" }
        };

        void BuildHint()
        {
            hintBox = UIKit.Panel(root, Palette.Amber, "Hint");
            hintBox.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -(TopBar + 8)), new Vector2(1000, 84));
            var outline = hintBox.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(3, -3);
            hintIcon = UIKit.Icon(hintBox.transform, "info", 52);
            hintIcon.rectTransform.Anchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(16, 0), new Vector2(52, 52));
            hintText = UIKit.Label(hintBox.transform, "", 28, Palette.Ink, TextAnchor.MiddleLeft);
            hintText.rectTransform.Stretch(84, 0, 160, 0);
            var close = UIKit.Button(hintBox.transform, "Got it", () => run.DismissHint(), Palette.Ink, Palette.Hull, 24);
            close.GetComponent<RectTransform>().Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-14, 0), new Vector2(132, 56));
        }

        void RefreshHint()
        {
            bool show = run.HintsOn && !string.IsNullOrEmpty(run.Hint) && !run.Ended;
            hintBox.gameObject.SetActive(show);
            if (!show) return;
            hintText.text = "<b>Tip:</b> " + Keywords.Mark(run.Hint, true);
            string id = run.HintId ?? "";
            hintIcon.sprite = Icons.Get(hintIcons.TryGetValue(id, out var icon) ? icon : id.StartsWith("n_") ? id.Substring(2) : "info");
        }

        // ---------- onboarding pointer ----------

        public bool CoachShowing => coach != null && coach.Showing;
        public string CoachMessage => coach != null ? coach.Message : "";

        // Points at the next card or the map while the player learns to build. Off when Tips are off.
        void UpdateCoach()
        {
            if (coach == null) return;
            var target = CoachTarget(out string message);
            if (target == null) { coach.Hide(); HighlightBuildings(false); return; }
            coach.Point(target, message);
            HighlightBuildings(run.IsDrawing && run.State.buildings.Count == 0);
        }

        RectTransform CoachTarget(out string message)
        {
            message = "";
            if (!run.HintsOn || run.Ended || ModalOpen || Wiki.IsOpen || run.Phase == RunPhase.Briefing) return null;
            bool learning = run.IsDrawing || (run.level != null && run.level.id == "L2" && run.State.Count(BuildingType.Habitat) == 0);
            if (!learning) return null;
            if (run.BuildChoice.HasValue)
            {
                message = run.BuildChoice == BuildingType.SolarArray ? "Click bright ground. Green glow = good spots" : "Now click the map to place it";
                return map;
            }
            if (run.IsDrawing && run.DrawingReady)
            {
                message = "All done! Press Finish";
                return finishButton != null ? (RectTransform)finishButton.transform : null;
            }
            BuildingType? next = null;
            foreach (var type in new[] { BuildingType.Habitat, BuildingType.SolarArray, BuildingType.Greenhouse })
                if (run.buildable.Contains(type) && run.State.Count(type) == 0) { next = type; break; }
            if (!next.HasValue && run.IsDrawing) next = BuildingType.SolarArray;
            if (!next.HasValue) return null;
            var card = cards.Find(c => c.type == next);
            if (card == null) return null;
            string name = run.sim.balance.Def(next.Value).displayName;
            message = run.IsDrawing && run.State.Count(next.Value) > 0 ? "Add another " + name + ": power in must beat power out" : "Pick the " + name;
            return (RectTransform)card.button.transform;
        }

        // ---------- frame loop ----------

        void Update()
        {
            HandleKeys();
            if (run == null) return;
            run.Tick(Time.unscaledDeltaTime);
            foreach (var e in run.TakeEvents()) HandleEvent(e);
            if (run.Ended) ShowEnd();
            if (run.Revision != shownRevision) Refresh();
            else if (Time.unscaledTime >= nextCounters) RefreshCounters();
            UpdateRover();
            UpdateCoach();
        }

        // Losing focus pauses; the player resumes with the Play button (never automatically).
        void OnApplicationFocus(bool focus)
        {
            if (focus || run == null || !run.Running) return;
            run.clock.FocusLost();
            Refresh();
        }

        void OnApplicationPause(bool paused) => OnApplicationFocus(!paused);

        void HandleKeys()
        {
            var keys = Keyboard.current;
            if (keys == null || run == null) return;
            if (keys.escapeKey.wasPressedThisFrame)
            {
                if (Wiki.IsOpen) { Wiki.Close(); return; }
                if (EndShowing) return;
                if (ModalOpen) { CloseModal(); return; }
                if (run.BuildChoice.HasValue || run.SelectedId != 0) { run.Deselect(); return; }
                OpenPause();
                return;
            }
            if (Wiki.IsOpen) return;
            if (keys.hKey.wasPressedThisFrame || keys.f1Key.wasPressedThisFrame) { if (!ModalOpen) OpenWiki(); return; }
            if (ModalOpen || EndShowing) return;
            if (keys.spaceKey.wasPressedThisFrame) TogglePause();
        }

        public void Refresh()
        {
            shownRevision = run.Revision;
            var power = run.Power;
            RefreshCounters();
            RefreshObjectives();
            RefreshCrew();
            RefreshMap();
            RefreshSelection(power);
            RefreshBottomBar();
            RefreshHint();
            statusText.text = Keywords.Mark(status);
            statusText.color = statusIsError ? Palette.Amber : Palette.Ice;
        }

        // Numbers that move every step: resource chips, clock, countdown, information.
        void RefreshCounters()
        {
            nextCounters = Time.unscaledTime + CounterInterval;
            RefreshTopBar(run.Power);
            RefreshTime();
            RefreshInformation();
            if (run.Selected != null) RefreshSelection(run.Power);
        }

        void SetStatus(string text, bool error = false)
        {
            status = text;
            statusIsError = error;
            if (statusText != null) { statusText.text = Keywords.Mark(text); statusText.color = error ? Palette.Amber : Palette.Ice; }
        }

        static string Num(double v) => SimText.Num(v);
    }

    static class RectExtensions
    {
        public static RectTransform TopLeftOf(this Image image, float x, float y)
        {
            var size = image.rectTransform.sizeDelta;
            return image.rectTransform.TopLeft(x, y, size.x, size.y);
        }
    }
}
