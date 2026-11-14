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

            var layer = UIKit.Canvas("Window Canvas", 10);
            modalLayer = layer;
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

        void ShowModal(ModalSpec spec)
        {
            if (openModal != null) { modalQueue.Enqueue(spec); return; }
            Render(spec);
        }

        public void CloseModal()
        {
            if (openModal != null) Destroy(openModal);
            openModal = null;
            ModalTitle = "";
            if (modalQueue.Count > 0) Render(modalQueue.Dequeue());
        }

        // Presses the first button of the open window (used by tests and the Enter key).
        public void PressModal(int index = 0)
        {
            if (openModal == null) return;
            var buttons = openModal.GetComponentsInChildren<Button>();
            if (index < buttons.Length) buttons[index].onClick.Invoke();
        }

        void Render(ModalSpec spec)
        {
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

        // ---------- start of a level ----------

        void ShowStartCard()
        {
            var body = new StringBuilder(run.mode == GameMode.Free ? run.Subtitle + "\nEvents come at random times. Watch the alerts." : run.Subtitle);
            body.Append("\n");
            var catalog = LevelCatalog.Load();
            foreach (var goal in run.goals) body.Append("\n•  ").Append(catalog.GoalLabel(goal));
            string title = run.mode == GameMode.Free ? run.Title : "Level " + (catalog.IndexOf(run.level.id) + 1) + ": " + run.Title;
            var spec = Modal(title, body.ToString(), run.IsDrawing ? "Habitat" : run.Planet.id == "Mars" ? "SolarArray" : "Habitat", new ModalButton("Start"));
            string card = run.level == null ? null : run.level.id == "L1" ? "c_sun_home" : run.level.id == "L2" ? "c_ridge" : null;
            if (card != null) spec.buttons.Add(new ModalButton("Sun facts", () => ShowCard(card), false));
            ShowModal(spec);
        }

        // ---------- Next Day ----------

        public void PressNextDay()
        {
            if (ModalOpen || run.Ended) return;
            if (run.IsDrawing)
            {
                if (!run.DrawingReady) { SetStatus("First: " + run.CurrentGoal, true); return; }
                Finish();
                return;
            }
            var report = run.NextDay();
            if (report == null) return;
            SetStatus("");
            if (run.State.Finished) { Finish(); return; }
            var notes = new List<string>();
            foreach (var e in report.events) HandleEvent(e, notes);
            ShowToast(DaySummary(report, notes));
        }

        string DaySummary(DayReport r, List<string> notes)
        {
            var sb = new StringBuilder();
            sb.Append(run.Planet.dayWord).Append(' ').Append(r.day).Append(" is done.   ")
              .Append("Battery ").Append(Delta(r.batteryEnd - r.batteryStart))
              .Append("  ·  Food ").Append(Delta(r.mealsEnd - r.mealsStart))
              .Append("  ·  Water ").Append(Delta(r.waterEnd - r.waterStart));
            for (int i = 0; i < notes.Count && i < 2; i++) sb.Append('\n').Append(notes[i]);
            return sb.ToString();
        }

        static string Delta(double v)
        {
            double r = Math.Round(v);
            return (r > 0 ? "+" : r < 0 ? "-" : "") + SimText.Num(Math.Abs(r));
        }

        void HandleEvent(SimEvent e, List<string> notes)
        {
            switch (e.code)
            {
                case "shadow_warning":
                    ShowModal(Modal(run.Planet.dimmingName + " tomorrow", "Less sun means less power.\nRest the Greenhouse or save battery power.", "power"));
                    break;
                case "battery_unlock":
                    ShowModal(Modal("New: a second Battery", "You got through the dark.\nA second Battery can save more power.", "battery"));
                    break;
                case "storm_warning":
                    var warning = Modal("Solar storm warning!", "Particles arrive tomorrow.\nCall Yuki inside. Turn on the Shelter.\nThe Sun still shines. Solar Arrays keep working.", "alert");
                    warning.badge = StormBadge();
                    ShowModal(warning);
                    break;
                case "storm_onset":
                    ShowModal(Modal("Particles have arrived", "Stay in the Shelter today.", "alert",
                        new ModalButton("OK"), new ModalButton("A real reading", () => ShowCard("c_iss"), false)));
                    break;
                case "first_harvest":
                    ShowModal(Modal("First harvest!", e.text, "food",
                        new ModalButton("OK"), new ModalButton("Why plants?", () => ShowCard("c_plants"), false)));
                    break;
                case "fault":
                    ShowModal(Modal("A Solar Array broke", "A tiny space rock hit it.\nFix it for " + run.sim.balance.repairCost + " supplies.", "SolarArray",
                        new ModalButton("Fix now", FixFirstBroken), new ModalButton("Later", null, false)));
                    break;
                case "sunrise_start":
                    ShowModal(Modal("Sunrise!", e.text, "power"));
                    break;
                case "brownout":
                    if (!notes.Contains(BrownoutNote)) notes.Add(BrownoutNote);
                    break;
                case "shadow_start":
                case "shadow_end":
                case "storm_decay":
                case "storm_over":
                case "supply_drop":
                case "self_return":
                case "geologist_inside":
                    notes.Add(e.text);
                    break;
            }
        }

        const string BrownoutNote = "Power ran out. Some systems switched off.";

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

        void Finish()
        {
            var result = run.Finish();
            Flow.FinishMission(result);
        }

        // ---------- crew chat ----------

        public void OpenChat(string crewId)
        {
            if (ModalOpen || run.IsDrawing) return;
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
            Refresh();
        }

        void Answer(CrewMember member, ChatReply reply)
        {
            bool ok = CrewChat.Apply(reply, run);
            string answer = ok ? reply.answer : "I can't hear you. The radio needs power.";
            ShowModal(Modal(member.name, "\"" + answer + "\"", null, new ModalButton("Close")));
        }

        // ---------- pause ----------

        void OpenPause()
        {
            if (ModalOpen) return;
            var spec = Modal("Paused", "", null);
            spec.vertical = true;
            spec.buttons.Clear();
            spec.buttons.Add(new ModalButton("Back to the base"));
            spec.buttons.Add(new ModalButton("Restart this level", Flow.Retry, false));
            spec.buttons.Add(new ModalButton("Tips: " + (run.HintsOn ? "On" : "Off"), ToggleHints, false));
            spec.buttons.Add(new ModalButton("Data & Credits", () => ShowModal(Modal("Data & Credits", MenuScreen.CreditsText, null)), false));
            spec.buttons.Add(new ModalButton("Quit to menu", () => Flow.ToMenu(run.mode == GameMode.Free ? "free" : "story"), false));
            ShowModal(spec);
        }

        void ToggleHints()
        {
            run.HintsOn = !run.HintsOn;
            Progress.HintsOn = run.HintsOn;
            Refresh();
            OpenPause();
        }
    }
}
