using System;
using System.Collections.Generic;
using System.Text;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    public sealed partial class GameScreen
    {
        sealed class ModalButton
        {
            public string label;
            public Action action;
            public bool primary = true;
            public ModalButton(string label, Action action = null, bool primary = true) { this.label = label; this.action = action; this.primary = primary; }
        }

        sealed class ModalSpec
        {
            public string title, body, badge, icon;
            public Color iconColor = Color.white;
            public bool vertical;
            public List<ModalButton> buttons = new List<ModalButton>();
        }

        RectTransform modalLayer;
        GameObject openModal;
        readonly Queue<ModalSpec> modalQueue = new Queue<ModalSpec>();
        CanvasGroup toast;
        Text toastText;
        float toastUntil;
        readonly HashSet<string> explained = new HashSet<string>();

        public bool ModalOpen => openModal != null;
        public string ModalTitle { get; private set; } = "";

        void BuildOverlays()
        {
            toast = UIKit.Panel(root, Palette.Alpha(Palette.Ink, 0.95f), "Toast").gameObject.AddComponent<CanvasGroup>();
            var rt = (RectTransform)toast.transform;
            rt.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -(TopBar + 72)), new Vector2(760, 110));
            toast.blocksRaycasts = false;
            toastText = UIKit.Label(toast.transform, "", 25, Palette.Hull, TextAnchor.MiddleCenter);
            toastText.rectTransform.Stretch(24, 10, 24, 10);
            toast.alpha = 0;

            modalLayer = UIKit.Canvas("Window Canvas", 10);
        }

        void ShowToast(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            toastText.text = text;
            toast.alpha = 1;
            toastUntil = Time.unscaledTime + 3.5f;
        }

        void UpdateToast()
        {
            if (toast == null || toast.alpha <= 0) return;
            if (Time.unscaledTime > toastUntil) toast.alpha = Mathf.Max(0, toast.alpha - Time.unscaledDeltaTime * 2);
        }

        // ---------- modal windows (one at a time, the rest wait in a queue) ----------
        // Every open window holds the mission clock; closing it releases the hold, so the earlier
        // play/pause state and speed come back. Nested or queued windows keep the mission paused.

        void ShowModal(ModalSpec spec)
        {
            if (openModal != null) { modalQueue.Enqueue(spec); return; }
            Render(spec);
        }

        public void CloseModal()
        {
            if (openModal == null) return;
            Destroy(openModal);
            openModal = null;
            ModalTitle = "";
            run.clock.Release();
            if (modalQueue.Count > 0) Render(modalQueue.Dequeue());
            Refresh();
        }

        // Presses a button of the open window (used by tests).
        public void PressModal(int index = 0)
        {
            if (openModal == null) return;
            var buttons = openModal.GetComponentsInChildren<Button>();
            if (index < buttons.Length) buttons[index].onClick.Invoke();
        }

        void Render(ModalSpec spec)
        {
            run.clock.Hold();
            var holder = UIKit.Rect(modalLayer, "Window " + spec.title).Stretch();
            openModal = holder.gameObject;
            ModalTitle = spec.title;
            UIKit.Blocker(holder);

            var panel = UIKit.Panel(holder, Palette.Panel, "Window");
            panel.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(860, 0));
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Cyan;
            outline.effectDistance = new Vector2(3, -3);
            var column = panel.gameObject.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(48, 48, 40, 40);
            column.spacing = 20;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            panel.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIFx.Pop(panel.transform);

            var head = UIKit.Rect(panel.transform, "Head");
            head.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
            if (!string.IsNullOrEmpty(spec.icon))
            {
                var icon = UIKit.Icon(head, spec.icon, 56);
                icon.color = spec.iconColor;
                icon.rectTransform.Anchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(56, 56));
            }
            var title = UIKit.Label(head, spec.title, 40, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            title.rectTransform.Stretch(string.IsNullOrEmpty(spec.icon) ? 0 : 76, 0, 0, 0);

            if (!string.IsNullOrEmpty(spec.body)) UIKit.Label(panel.transform, spec.body, 28, Palette.Ice, TextAnchor.UpperLeft);
            if (!string.IsNullOrEmpty(spec.badge)) UIKit.Label(panel.transform, spec.badge, 20, Palette.Cyan, TextAnchor.UpperLeft);

            var row = UIKit.Rect(panel.transform, "Buttons");
            HorizontalOrVerticalLayoutGroup group = spec.vertical
                ? (HorizontalOrVerticalLayoutGroup)row.gameObject.AddComponent<VerticalLayoutGroup>()
                : row.gameObject.AddComponent<HorizontalLayoutGroup>();
            group.spacing = 16;
            group.childControlWidth = group.childControlHeight = true;
            group.childForceExpandWidth = true;
            group.childForceExpandHeight = false;
            row.gameObject.AddComponent<LayoutElement>().preferredHeight = spec.vertical ? spec.buttons.Count * 80 : 70;
            foreach (var b in spec.buttons)
            {
                var action = b.action;
                var button = UIKit.Button(row, b.label, () => { CloseModal(); action?.Invoke(); },
                    b.primary ? Palette.Azure : Palette.PanelLight, Palette.Hull, 28);
                button.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
            }
        }

        static ModalSpec Modal(string title, string body, string icon = null, params ModalButton[] buttons)
        {
            var spec = new ModalSpec { title = title, body = body, icon = icon };
            spec.buttons.AddRange(buttons.Length > 0 ? buttons : new[] { new ModalButton("OK") });
            return spec;
        }

        void ShowCard(string id)
        {
            var card = story.Card(id);
            if (card == null) return;
            var spec = Modal(card.title, card.text + "\n\n" + card.text2, "star");
            spec.badge = "Source: " + card.source + "  (" + card.label + ")";
            ShowModal(spec);
        }

        // ---------- briefing (paused until Start) ----------

        void ShowStartCard()
        {
            var catalog = LevelCatalog.Load();
            var body = new StringBuilder(run.mode == GameMode.Free ? run.Subtitle + "\nEvents come at random times. Watch the alerts." : run.Subtitle);
            if (!run.IsDrawing)
                body.Append("\nInspection in ").Append(SimText.Duration(run.Duration, run.Planet.dayWord, run.Planet.dayHours))
                    .Append(". Keep the crew safe until then.");
            if (run.UsedStarterBase) body.Append("\n").Append(story.Debrief("starter_base"));
            body.Append("\n");
            foreach (var goal in run.goals) body.Append("\n•  ").Append(catalog.GoalLabel(goal));
            string title = run.mode == GameMode.Free ? run.Title : "Level " + (catalog.IndexOf(run.level.id) + 1) + ": " + run.Title;
            string label = run.IsDrawing ? "Start drawing" : "Start mission";
            var spec = Modal(title, body.ToString(), run.IsDrawing ? "Habitat" : run.Planet.id == "Mars" ? "SolarArray" : "Habitat",
                new ModalButton(label, run.StartMission));
            string card = BriefingCard();
            if (card != null) spec.buttons.Add(new ModalButton("Science fact", () => { ShowCard(card); }, false));
            ShowModal(spec);
        }

        string BriefingCard()
        {
            if (run.level == null) return null;
            switch (run.level.id)
            {
                case "L1": return "c_sun_home";
                case "L2": return "c_ridge";
                case "L4": return "c_jezero";
                default: return null;
            }
        }

        // ---------- events during play ----------

        void HandleEvent(SimEvent e)
        {
            switch (e.code)
            {
                case "shadow_warning":
                    ShowToast(e.text);
                    break;
                case "shadow_start":
                    if (Explain("dim")) ShowModal(Modal(run.Planet.dimmingName, "Less sun means less power.\nRest the Greenhouse or use battery power.", "power"));
                    else ShowToast(e.text);
                    break;
                case "battery_unlock":
                    ShowModal(Modal("New: a second Battery", "You got through the dark.\nPut it next to a Solar Array.", "battery"));
                    break;
                case "storm_warning":
                    var warning = Modal("Solar storm warning!", "Particles arrive in " + SimText.Num(run.sim.balance.storm.onsetHour) + " hours.\nCall "
                        + run.sim.geologistName + " inside. Turn on the Shelter.\nThe Sun still shines. Solar Arrays keep working.", "alert");
                    warning.badge = StormBadge();
                    ShowModal(warning);
                    break;
                case "storm_onset":
                    ShowToast("Particles have arrived. Stay in the Shelter.");
                    break;
                case "first_harvest":
                    var cardId = run.Planet.id == "Mars" ? "c_moxie" : "c_plants";
                    ShowModal(Modal("First harvest!", e.text, "food", new ModalButton("OK"), new ModalButton("Why plants?", () => ShowCard(cardId), false)));
                    break;
                case "fault":
                    ShowModal(Modal("A Solar Array broke", "A tiny space rock hit it.\nFix it for " + run.sim.balance.repairCost + " supplies.", "SolarArray",
                        new ModalButton("Fix now", FixFirstBroken), new ModalButton("Later", null, false)));
                    break;
                case "sunrise_start":
                    ShowModal(Modal("Sunrise!", e.text, "power"));
                    break;
                case "critical_out":
                    if (Explain("critical")) ShowModal(Modal("Life Support has no power!", "Air stops when Life Support has no power.\n"
                        + "Switch off what can wait, or add Solar Arrays near a Battery.", "power"));
                    break;
                case "brownout":
                    ShowToast("Power ran short. Some systems switched off.");
                    break;
                case "placed_RoverGarage":
                    if (Explain("rover") && run.sim.terrain.AnyWater()) ShowCard(run.Planet.id == "Mars" ? "c_mars_sun" : "c_ice");
                    break;
                case "shadow_end":
                case "storm_decay":
                case "storm_over":
                case "supply_drop":
                case "self_return":
                case "geologist_inside":
                    ShowToast(e.text);
                    break;
            }
        }

        // True the first time a topic comes up in this run, so explanations never repeat.
        bool Explain(string topic) => explained.Add(topic);

        void FixFirstBroken()
        {
            foreach (var b in run.State.buildings)
                if (b.broken) { run.Repair(b.id); return; }
        }

        static string StormBadge()
        {
            var donki = FixtureLoader.Donki();
            string date = donki.flare.beginTime.Length >= 10 ? donki.flare.beginTime.Substring(0, 10) : donki.flare.beginTime;
            return donki.Badge + " · NASA DONKI: a real " + donki.flare.classType + " flare on " + date + ". The timing in this game is made up.";
        }

        // ---------- crew chat ----------

        public void OpenChat(string crewId)
        {
            if (ModalOpen || run.IsDrawing || run.Ended) return;
            var member = story.Crew(crewId);
            var entry = CrewChat.Entry(story, crewId, run);
            var row = crewRows.Find(r => r.id == crewId);
            if (row != null) row.seenWhen = entry.when;
            var spec = Modal(member.name + " · " + member.role, "\"" + entry.line + "\"", null);
            spec.buttons.Clear();
            spec.vertical = true;
            foreach (var reply in entry.replies)
            {
                var chosen = reply;
                spec.buttons.Add(new ModalButton(reply.text, () => Answer(member, chosen), false));
            }
            ShowModal(spec);
        }

        void Answer(CrewMember member, ChatReply reply)
        {
            bool ok = CrewChat.Apply(reply, run);
            string answer = ok ? reply.answer : "I can't hear you. The radio needs power.";
            ShowModal(Modal(member.name, "\"" + answer + "\"", null, new ModalButton("Close")));
        }

        // ---------- pause menu ----------

        void OpenPause()
        {
            if (ModalOpen || EndShowing) return;
            var spec = Modal("Menu", "The mission waits while this menu is open.", null);
            spec.vertical = true;
            spec.buttons.Clear();
            spec.buttons.Add(new ModalButton("Back to the base"));
            spec.buttons.Add(new ModalButton("Restart this level", Flow.Retry, false));
            spec.buttons.Add(new ModalButton("Tips: " + (run.HintsOn ? "On" : "Off"), ToggleHints, false));
            spec.buttons.Add(new ModalButton("Calm motion: " + (Progress.ReducedMotion ? "On" : "Off"), ToggleCalm, false));
            spec.buttons.Add(new ModalButton("Data & Credits", () => ShowModal(Modal("Data & Credits", MenuScreen.CreditsText, null)), false));
            spec.buttons.Add(new ModalButton("Quit to menu", () => Flow.ToMenu(run.mode == GameMode.Free ? "free" : "story"), false));
            ShowModal(spec);
        }

        void ToggleHints()
        {
            run.HintsOn = !run.HintsOn;
            Progress.HintsOn = run.HintsOn;
            OpenPause();
        }

        void ToggleCalm()
        {
            Progress.ReducedMotion = !Progress.ReducedMotion;
            OpenPause();
        }
    }
}
