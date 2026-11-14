using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Map-first play screen (Civ-style): top resource bar, goal card and crew on the left,
    // alerts on the right, build bar and Next Day at the bottom. Text is short on purpose.
    public sealed partial class GameScreen : MonoBehaviour
    {
        public const int TopBar = 76;
        public const int BottomBar = 124;
        public const int Side = 420;

        MissionRun run;
        RectTransform root;
        StoryData story;
        int shownRevision = -1;

        public MissionRun Run => run;

        sealed class Chip { public Text label, value; public string detail; }
        readonly List<Chip> chips = new List<Chip>();
        Text dayText;
        Text goalCaption, goalText, goalCount;
        RectTransform alertList, crewList;
        Image hintBox;
        Text hintText;
        Text statusText;
        string status = "";
        bool statusIsError;

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
            BuildGoalCard();
            BuildCrew();
            BuildAlerts();
            BuildSelectionPanel();
            BuildBottomBar();
            BuildHint();
            BuildOverlays();
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

            var menu = UIKit.Button(bar.transform, "Menu", OpenPause, Palette.PanelLight, Palette.Hull, 24);
            menu.GetComponent<RectTransform>().TopLeft(14, 12, 110, 52);

            string[] ids = run.IsDrawing ? new[] { "power", "power", "supplies" } : new[] { "power", "battery", "air", "water", "food", "crew", "supplies" };
            string[] names = run.IsDrawing ? new[] { "Power in", "Power out", "Supplies" } : new[] { "Power", "Battery", "Air", "Water", "Food", "Crew safety", "Supplies" };
            for (int i = 0; i < ids.Length; i++)
            {
                var chip = new Chip();
                var box = UIKit.Rect(bar.transform, "Chip " + names[i]).TopLeft(150 + i * 200, 6, 190, 64);
                var click = box.gameObject.AddComponent<Image>();
                click.color = new Color(0, 0, 0, 0);
                var button = box.gameObject.AddComponent<Button>();
                button.transition = Selectable.Transition.None;
                button.onClick.AddListener(() => ShowToast(chip.detail));
                UIKit.Icon(box, ids[i], 40).TopLeftOf(0, 12);
                chip.label = UIKit.Label(box, names[i], 18, Palette.Muted);
                chip.label.rectTransform.TopLeft(50, 4, 140, 24);
                chip.value = UIKit.Label(box, "", 28, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
                chip.value.rectTransform.TopLeft(50, 26, 140, 34);
                chips.Add(chip);
            }

            dayText = UIKit.Label(bar.transform, "", 30, Palette.Hull, TextAnchor.MiddleRight, FontStyle.Bold);
            dayText.rectTransform.Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-28, 0), new Vector2(300, 60));
        }

        void RefreshTopBar(StepReport power)
        {
            var s = run.State;
            if (run.IsDrawing)
            {
                SetChip(0, Num(power.generationKW) + " kW", power.generationKW + 1e-6 >= power.demandKW && power.demandKW > 0 ? Palette.Mint : Palette.Hull,
                    "Power in: Solar Arrays make power from sunlight.");
                SetChip(1, Num(power.demandKW) + " kW", power.demandKW > power.generationKW + 1e-6 ? Palette.Amber : Palette.Hull,
                    "Power out: what your buildings use.");
                SetChip(2, s.supplies.ToString(), Palette.Hull, "Supplies pay for new buildings.");
                dayText.text = "Your drawing";
                return;
            }
            double net = power.generationKW - power.demandKW;
            SetChip(0, (net >= 0 ? "+" : "") + Num(net) + " kW", net >= 0 ? Palette.Mint : Palette.Amber,
                "Solar makes " + Num(power.generationKW) + " kW. The base uses " + Num(power.demandKW) + " kW.");
            double cap = run.sim.BatteryCapacity(s);
            SetChip(1, cap > 0 ? Num(s.batteryKWh) + "/" + Num(cap) : "none", cap > 0 && s.batteryKWh < cap * 0.25 ? Palette.Amber : Palette.Hull,
                "Batteries save extra power for later (kWh).");
            SetChip(2, Num(s.airHours) + " h", s.airHours < 8 ? Palette.Danger : Palette.Hull, "Hours of air. Life Support makes more.");
            SetChip(3, Num(s.waterL) + " L", s.waterL < 20 ? Palette.Amber : Palette.Hull, "Water. The Greenhouse recycles most of it.");
            SetChip(4, Num(s.meals), s.meals < 5 ? Palette.Amber : Palette.Hull, "Meals left. The Greenhouse grows more.");
            SetChip(5, Num(s.safety) + "%", s.safety < 70 ? Palette.Danger : Palette.Mint, "How safe the crew is. Keep it high.");
            SetChip(6, s.supplies.ToString(), Palette.Hull, "Supplies pay for buildings and repairs.");
            dayText.text = run.Planet.dayWord + " " + Mathf.Min(s.day, run.Days) + " of " + run.Days;
        }

        void SetChip(int i, string value, Color color, string detail)
        {
            chips[i].value.text = value;
            chips[i].value.color = color;
            chips[i].detail = detail;
        }

        // ---------- goal card ----------

        void BuildGoalCard()
        {
            var card = UIKit.Panel(root, Palette.Alpha(Palette.Panel, 0.95f), "Goal Card");
            card.rectTransform.TopLeft(20, TopBar + 16, Side, 150);
            goalCaption = UIKit.Label(card.transform, "", 20, Palette.Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            goalCaption.rectTransform.TopLeft(20, 12, Side - 40, 26);
            goalText = UIKit.Label(card.transform, "", 28, Palette.Hull, TextAnchor.UpperLeft, FontStyle.Bold);
            goalText.rectTransform.TopLeft(20, 44, Side - 40, 70);
            goalCount = UIKit.Label(card.transform, "", 20, Palette.Muted);
            goalCount.rectTransform.TopLeft(20, 114, Side - 40, 26);
        }

        void RefreshGoalCard()
        {
            goalCaption.text = run.mode == GameMode.Free
                ? "FREE BUILD · " + run.Planet.name.ToUpperInvariant() + " · MAP #" + run.free.seed
                : "LEVEL " + (LevelCatalog.Load().IndexOf(run.level.id) + 1) + " · " + run.Title.ToUpperInvariant();
            goalText.text = run.CurrentGoal;
            goalCount.text = "Goals: " + run.GoalsMet + " of " + run.goals.Length;
        }

        // ---------- crew ----------

        sealed class CrewRow { public string id; public Text name, role; public Image badge; public string seenWhen = ""; }
        readonly List<CrewRow> crewRows = new List<CrewRow>();

        static Color CrewColor(string id) => id == "engineer" ? Palette.Cyan : id == "botanist" ? Palette.Mint : Palette.Peach;

        void BuildCrew()
        {
            if (run.IsDrawing) return;
            crewList = UIKit.Rect(root, "Crew").TopLeft(20, TopBar + 184, Side, 250);
            UIKit.Label(crewList, "CREW · click to talk", 20, Palette.Muted, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.TopLeft(4, 0, Side, 26);
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
                row.role = UIKit.Label(button.transform, member.role, 18, Palette.Muted);
                row.role.rectTransform.TopLeft(72, 34, 300, 24);
                row.badge = UIKit.Rect(button.transform, "New").gameObject.AddComponent<Image>();
                row.badge.sprite = UIKit.CircleSprite;
                row.badge.color = Palette.Amber;
                row.badge.raycastTarget = false;
                row.badge.rectTransform.Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-16, 0), new Vector2(30, 30));
                UIKit.Label(row.badge.transform, "!", 22, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
                crewRows.Add(row);
            }
        }

        void RefreshCrew()
        {
            foreach (var row in crewRows)
            {
                string when = CrewChat.When(row.id, run);
                row.badge.gameObject.SetActive(when != row.seenWhen);
                var member = story.Crew(row.id);
                bool outside = row.id == "geologist" && run.State.geologist != CrewLocation.Inside;
                row.role.text = outside ? (run.State.geologist == CrewLocation.Returning ? "Coming back..." : "Outside the base!") : member.role;
                row.role.color = outside ? Palette.Amber : Palette.Muted;
            }
        }

        // ---------- alerts ----------

        void BuildAlerts()
        {
            alertList = UIKit.Rect(root, "Alerts").TopLeft(1920 - Side - 20, TopBar + 16, Side, 400);
        }

        void RefreshAlerts()
        {
            UIKit.Clear(alertList);
            var alerts = run.Alerts();
            float y = 0;
            foreach (var alert in alerts)
            {
                bool hasAction = alert.action != null;
                var card = UIKit.Panel(alertList, Palette.Alpha(Palette.Panel, 0.96f), "Alert");
                float h = hasAction ? 128 : 84;
                card.rectTransform.TopLeft(0, y, Side, h);
                var stripe = UIKit.Panel(card.transform, AlertColor(alert.level), "Stripe", false);
                stripe.rectTransform.TopLeft(0, 0, 10, h);
                UIKit.Icon(card.transform, "alert", 30).TopLeftOf(24, 14);
                var text = UIKit.Label(card.transform, alert.text, 23, Palette.Hull, TextAnchor.UpperLeft);
                text.rectTransform.TopLeft(66, 12, Side - 84, 64);
                if (hasAction)
                {
                    var action = alert.action;
                    var button = UIKit.Button(card.transform, alert.actionLabel, () => action(), alert.level == AlertLevel.Danger ? Palette.Danger : Palette.Azure, Palette.Hull, 22);
                    button.GetComponent<RectTransform>().TopLeft(66, 74, 250, 44);
                    button.interactable = alert.actionEnabled;
                }
                y += h + 10;
            }
        }

        static Color AlertColor(AlertLevel level) => level == AlertLevel.Danger ? Palette.Danger : level == AlertLevel.Warning ? Palette.Amber : Palette.Cyan;

        // ---------- hint ----------

        void BuildHint()
        {
            hintBox = UIKit.Panel(root, Palette.Alpha(Palette.Azure, 0.95f), "Hint");
            hintBox.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -(TopBar + 10)), new Vector2(980, 50));
            hintText = UIKit.Label(hintBox.transform, "", 24, Palette.Hull, TextAnchor.MiddleLeft);
            hintText.rectTransform.Stretch(20, 0, 130, 0);
            var close = UIKit.Button(hintBox.transform, "Got it", () => run.DismissHint(), Palette.Ink, Palette.Hull, 20);
            close.GetComponent<RectTransform>().Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-8, 0), new Vector2(110, 38));
        }

        void RefreshHint()
        {
            bool show = run.HintsOn && !string.IsNullOrEmpty(run.Hint);
            hintBox.gameObject.SetActive(show);
            hintText.text = "Tip: " + run.Hint;
        }

        // ---------- refresh ----------

        void Update()
        {
            HandleKeys();
            UpdateToast();
            if (run == null || run.Revision == shownRevision) return;
            Refresh();
        }

        void HandleKeys()
        {
            var keys = Keyboard.current;
            if (keys == null || run == null) return;
            if (keys.escapeKey.wasPressedThisFrame)
            {
                if (ModalOpen) { CloseModal(); return; }
                if (run.BuildChoice.HasValue || run.SelectedId != 0) { run.Deselect(); return; }
                OpenPause();
                return;
            }
            if (ModalOpen) return;
            if (keys.enterKey.wasPressedThisFrame) PressNextDay();
        }

        public void Refresh()
        {
            shownRevision = run.Revision;
            var power = run.Power;
            RefreshTopBar(power);
            RefreshGoalCard();
            RefreshCrew();
            RefreshAlerts();
            RefreshMap();
            RefreshSelection(power);
            RefreshBottomBar();
            RefreshHint();
            statusText.text = status;
            statusText.color = statusIsError ? Palette.Amber : Palette.Ice;
        }

        void SetStatus(string text, bool error = false)
        {
            status = text;
            statusIsError = error;
            if (statusText != null) { statusText.text = text; statusText.color = error ? Palette.Amber : Palette.Ice; }
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
