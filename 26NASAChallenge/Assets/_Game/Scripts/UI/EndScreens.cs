using System.Collections.Generic;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // The two ways a timed run ends, shown over the frozen map:
    // - Mission stopped: a short story beat for the exact cause (air, food, water, power, crew outside, shelter).
    // - Evaluation complete: an inspection sweep, the blueprint checklist and the grade.
    // Both play on unscaled UI time, can be skipped at once, and use a static picture when Calm motion is on.
    // Art slot: put Resources/EndScenes/<variant>.png (e.g. NoAir.png) to replace the drawn picture.
    public sealed partial class GameScreen
    {
        public const float BeatSeconds = 3f;

        RectTransform endLayer;
        public bool EndShowing => endLayer != null;
        public string EndVariant { get; private set; } = "";
        public bool EndBeatDone { get; private set; }
        readonly List<GameObject> endButtonsGroup = new List<GameObject>();

        void ShowEnd()
        {
            if (EndShowing || run.Result == null) return;
            var result = run.Result;
            Flow.Record(result);
            if (run.IsDrawing) { Flow.ShowReport(result); return; }
            while (ModalOpen) CloseModal();
            modalQueue.Clear();
            run.Deselect();

            endLayer = UIKit.Canvas("End Canvas", 20);
            var dim = UIKit.Blocker(endLayer, 0.55f);
            dim.raycastTarget = true;
            EndVariant = result.Failed ? result.failReason.ToString() : "Evaluation";
            if (result.Failed) BuildFailure(result);
            else BuildEvaluation(result);
        }

        // ---------- shared pieces ----------

        RectTransform Card(Color edge)
        {
            var panel = UIKit.Panel(endLayer, Palette.Alpha(Palette.Panel, 0.98f), "End Card");
            panel.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1180, 860));
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = edge;
            outline.effectDistance = new Vector2(4, -4);
            return panel.rectTransform;
        }

        void EndButtons(RectTransform card, params (string label, UnityEngine.Events.UnityAction action, bool primary)[] buttons)
        {
            var row = UIKit.Rect(card, "Buttons");
            row.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(1060, 80));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;
            foreach (var (label, action, primary) in buttons)
            {
                var b = UIKit.Button(row, label, action, primary ? Palette.Azure : Palette.PanelLight, Palette.Hull, 30);
                b.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 76);
            }
            row.gameObject.SetActive(false);
            endButtonsGroup.Add(row.gameObject);
        }

        // Skips the animation and shows the buttons. Also used by tests.
        public void SkipEndBeat()
        {
            if (!EndShowing) return;
            foreach (var beat in endLayer.GetComponentsInChildren<EndBeat>()) beat.Finish();
            RevealEndButtons();
        }

        void RevealEndButtons()
        {
            EndBeatDone = true;
            foreach (var go in endButtonsGroup) if (go != null) go.SetActive(true);
        }

        public void PressEnd(string label)
        {
            if (!EndShowing) return;
            SkipEndBeat();
            foreach (var b in endLayer.GetComponentsInChildren<Button>())
                if (UIKit.LabelOf(b) != null && UIKit.LabelOf(b).text == label) { b.onClick.Invoke(); return; }
        }

        void SkipButton(RectTransform card)
        {
            var skip = UIKit.Button(card, "Skip", SkipEndBeat, Palette.PanelLight, Palette.Ice, 20);
            skip.GetComponent<RectTransform>().Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -20), new Vector2(110, 44));
            var beat = card.gameObject.AddComponent<EndBeat>();
            beat.seconds = UIFx.Calm ? 0 : BeatSeconds;
            beat.done = () => { RevealEndButtons(); if (skip) skip.gameObject.SetActive(false); };
        }

        // ---------- mission stopped ----------

        void BuildFailure(MissionResult r)
        {
            AudioDirector.Play(Sfx.MissionStopped);
            var card = Card(Palette.Amber);
            UIKit.Label(card, story.Debrief("title_fail"), 52, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(1000, 70));

            var stage = UIKit.Rect(card, "Scene").Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -120), new Vector2(760, 330));
            var back = stage.gameObject.AddComponent<Image>();
            back.color = run.Planet.id == "Mars" ? Palette.Hex("3A1E1C") : Palette.Hex("161F33");
            var art = Resources.Load<Sprite>("EndScenes/" + r.failReason);
            if (art != null)
            {
                var image = UIKit.Rect(stage, "Art").gameObject.AddComponent<Image>();
                image.sprite = art;
                image.preserveAspect = true;
                image.rectTransform.Stretch();
            }
            else DrawFailureScene(stage, r);

            UIKit.Label(card, r.headline, 34, Palette.Amber, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -470), new Vector2(1060, 50));
            UIKit.Label(card, r.cause, 28, Palette.Hull, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -525), new Vector2(1060, 44));
            UIKit.Label(card, "Next time: " + r.fix, 28, Palette.Mint, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -578), new Vector2(1060, 44));
            UIKit.Label(card, story.Debrief("body_fail"), 22, Palette.Muted, TextAnchor.MiddleCenter, FontStyle.Italic)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -630), new Vector2(1060, 60));

            SkipButton(card);
            string menu = run.mode == GameMode.Free ? "free" : "story";
            EndButtons(card, ("Try again", Flow.Retry, true), ("View report", () => Flow.ShowReport(r), false), ("Menu", () => Flow.ToMenu(menu), false));
        }

        // A gentle picture per cause: buildings, a meter that empties, and slow lights. No people are shown hurt.
        void DrawFailureScene(RectTransform stage, MissionResult r)
        {
            var s = run.State;
            Color ground = run.Planet.id == "Mars" ? Palette.MarsRust : Palette.LunarShadow;
            var floor = Shape(stage, "Ground", new Vector2(0, 0), new Vector2(760, 90), ground);
            floor.rectTransform.anchorMin = floor.rectTransform.anchorMax = floor.rectTransform.pivot = new Vector2(0.5f, 0);
            floor.rectTransform.anchoredPosition = Vector2.zero;

            switch (r.failReason)
            {
                case FailReason.NoAir:
                    Building(stage, "Habitat", new Vector2(-170, 30), 150, true);
                    if (s.Count(BuildingType.LifeSupport) > 0) Building(stage, "LifeSupport", new Vector2(40, 30), 110, false);
                    else Missing(stage, new Vector2(40, 30), 110);
                    Meter(stage, "air", r.air, run.sim.balance.airCapHours, Palette.Cyan);
                    Lamp(stage, new Vector2(-170, 210));
                    break;
                case FailReason.NoFood:
                    if (s.Count(BuildingType.Greenhouse) > 0) Building(stage, "Greenhouse", new Vector2(-120, 30), 150, false, r.water <= 0);
                    else Missing(stage, new Vector2(-120, 30), 150);
                    Meter(stage, "food", r.meals, run.sim.balance.startMeals, Palette.Mint);
                    break;
                case FailReason.NoWater:
                    if (s.Count(BuildingType.Greenhouse) > 0) Building(stage, "Greenhouse", new Vector2(-150, 30), 130, false, true);
                    Building(stage, "Habitat", new Vector2(30, 30), 110, true);
                    Meter(stage, "water", r.water, run.sim.balance.startWaterL, Palette.Azure);
                    break;
                case FailReason.PowerLoss:
                    Building(stage, "Habitat", new Vector2(-200, 30), 120, false);
                    if (s.Count(BuildingType.LifeSupport) > 0) Building(stage, "LifeSupport", new Vector2(-50, 30), 100, false);
                    PowerLine(stage, new Vector2(-280, 170), 400);
                    if (r.batteryCap > 0) Meter(stage, "battery", r.battery, r.batteryCap, Palette.Cyan);
                    Lamp(stage, new Vector2(-200, 210));
                    break;
                case FailReason.RadiationOutside:
                    Building(stage, "Habitat", new Vector2(150, 30), 130, true);
                    var crew = Shape(stage, "Crew", new Vector2(-230, 60), new Vector2(40, 40), Palette.Peach);
                    crew.sprite = UIKit.CircleSprite;
                    UIKit.Label(crew.transform, run.sim.geologistName.Substring(0, 1), 22, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
                    var call = UIKit.Panel(stage, Palette.Danger, "Call button");
                    call.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(-230, 150), new Vector2(160, 44));
                    UIKit.Label(call.transform, "Call " + run.sim.geologistName, 20, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
                    call.gameObject.AddComponent<SoftPulse>();
                    Particles(stage);
                    break;
                default:
                    Building(stage, "Habitat", new Vector2(-150, 30), 130, true);
                    var shelter = s.First(BuildingType.Shelter);
                    if (shelter == null) Missing(stage, new Vector2(60, 30), 110);
                    else Building(stage, "Shelter", new Vector2(Layout.GapTo(run.sim.balance, s, shelter, BuildingType.Habitat) > run.sim.balance.layout.shelterGap ? 230 : 60, 30),
                        110, false);
                    Particles(stage);
                    break;
            }
        }

        static Image Shape(RectTransform parent, string name, Vector2 pos, Vector2 size, Color color)
        {
            var image = UIKit.Rect(parent, name).gameObject.AddComponent<Image>();
            image.color = color;
            image.raycastTarget = false;
            image.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), pos + new Vector2(0, 60), size);
            return image;
        }

        static void Building(RectTransform stage, string type, Vector2 pos, float size, bool lit, bool dry = false)
        {
            var box = UIKit.Panel(stage, BuildingColor((BuildingType)System.Enum.Parse(typeof(BuildingType), type)), type);
            box.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), pos + new Vector2(0, 60), new Vector2(size, size * 0.8f));
            var icon = UIKit.Icon(box.transform, type, size * 0.55f);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icon.rectTransform.anchoredPosition = Vector2.zero;
            var fade = box.gameObject.AddComponent<EndBeat>();
            fade.seconds = UIFx.Calm ? 0 : 2f;
            Color start = box.color;
            Color end = lit ? start : Color.Lerp(start, Palette.Space, dry ? 0.35f : 0.6f);
            if (dry) end = Color.Lerp(end, Palette.Hex("8A6A3C"), 0.4f);
            fade.step = k => box.color = Color.Lerp(start, end, k);
        }

        static void Missing(RectTransform stage, Vector2 pos, float size)
        {
            var outline = UIKit.Rect(stage, "Missing").gameObject.AddComponent<Image>();
            outline.sprite = UIFx.RingSprite;
            outline.type = Image.Type.Sliced;
            outline.color = Palette.Alpha(Palette.Hull, 0.6f);
            outline.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), pos + new Vector2(0, 60), new Vector2(size, size * 0.8f));
            UIKit.Label(outline.transform, "?", 48, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
        }

        // A meter that drains from where it was to the final reading.
        static void Meter(RectTransform stage, string icon, double value, double max, Color color)
        {
            var frame = UIKit.Panel(stage, Palette.Ink, "Meter");
            frame.rectTransform.Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-40, 110), new Vector2(70, 190));
            UIKit.Icon(frame.transform, icon, 40).rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(40, 40));
            var fill = UIKit.Panel(frame.transform, color, "Fill", false);
            var rt = fill.rectTransform;
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(1, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.offsetMin = new Vector2(8, 8);
            float final = Mathf.Clamp01((float)(value / System.Math.Max(1e-6, max)));
            var beat = frame.gameObject.AddComponent<EndBeat>();
            beat.seconds = UIFx.Calm ? 0 : 2.4f;
            beat.step = k => rt.sizeDelta = new Vector2(-16, Mathf.Lerp(0.35f, final, k) * 174);
            beat.step(0);
        }

        // A soft amber emergency light (slow pulse, never a fast flash).
        static void Lamp(RectTransform stage, Vector2 pos)
        {
            var lamp = Shape(stage, "Lamp", pos, new Vector2(26, 26), Palette.Amber);
            lamp.sprite = UIKit.CircleSprite;
            lamp.gameObject.AddComponent<SoftPulse>();
        }

        // Dots that flow along a cable, then stop.
        static void PowerLine(RectTransform stage, Vector2 start, float length)
        {
            var line = Shape(stage, "Cable", start + new Vector2(length / 2, 0), new Vector2(length, 6), Palette.Ink);
            for (int i = 0; i < 6; i++)
            {
                var dot = Shape(stage, "Spark", start, new Vector2(10, 10), Palette.Amber);
                var beat = dot.gameObject.AddComponent<EndBeat>();
                beat.seconds = UIFx.Calm ? 0 : 2.2f;
                float offset = i / 6f;
                var rt = dot.rectTransform;
                beat.step = k =>
                {
                    float x = ((offset + k * 1.5f) % 1f) * length;
                    rt.anchoredPosition = start + new Vector2(x, 60);
                    dot.color = Palette.Alpha(Palette.Amber, 1 - k);
                };
            }
            line.color = Palette.Ink;
        }

        // Slow drifting particles for the solar storm (small, soft, no flashing).
        static void Particles(RectTransform stage)
        {
            var rng = new System.Random(7);
            for (int i = 0; i < 24; i++)
            {
                var p = Shape(stage, "Particle", new Vector2(rng.Next(-360, 360), rng.Next(0, 250)), new Vector2(6, 6), Palette.Alpha(Palette.Amber, 0.8f));
                var drift = p.gameObject.AddComponent<Drift>();
                drift.velocity = new Vector2(-30 - rng.Next(30), -20 - rng.Next(20));
            }
        }

        // ---------- evaluation complete ----------

        void BuildEvaluation(MissionResult r)
        {
            var scan = UIKit.Panel(endLayer, Palette.Alpha(Palette.Cyan, 0.35f), "Inspection scan", false);
            scan.rectTransform.anchorMin = new Vector2(0, 1);
            scan.rectTransform.anchorMax = new Vector2(1, 1);
            scan.rectTransform.pivot = new Vector2(0.5f, 0.5f);
            scan.rectTransform.sizeDelta = new Vector2(0, 14);
            var sweep = scan.gameObject.AddComponent<EndBeat>();
            sweep.seconds = UIFx.Calm ? 0 : 1.2f;
            sweep.step = k => scan.rectTransform.anchoredPosition = new Vector2(0, -k * 1080);
            sweep.done = () => scan.gameObject.SetActive(false);

            var card = Card(Palette.Cyan);
            UIKit.Label(card, story.Debrief("title_eval"), 52, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(1000, 70));
            UIKit.Label(card, "Mission control inspected your base.", 26, Palette.Ice, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -105), new Vector2(1000, 36));

            var list = UIKit.Rect(card, "Checklist").TopLeft(60, 160, 640, 560);
            UIKit.Label(list, "BLUEPRINT CHECKLIST", 22, Palette.Cyan, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.TopLeft(0, 0, 640, 30);
            int n = Mathf.Min(r.checklist.Count, 13);
            for (int i = 0; i < n; i++)
            {
                var item = r.checklist[i];
                var row = UIKit.Rect(list, "Item").TopLeft(0, 40 + i * 40, 640, 36);
                var box = UIKit.Panel(row, item.met ? Palette.Mint : Palette.Alpha(Palette.Danger, 0.25f), "Box");
                box.rectTransform.TopLeft(0, 4, 28, 28);
                UIKit.Label(box.transform, item.met ? "+" : "", 22, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
                var label = UIKit.Label(row, item.met ? item.label : item.label + "  (missing)", 23, item.met ? Palette.Hull : Palette.Amber);
                label.rectTransform.TopLeft(42, 0, 600, 36);
                var appear = row.gameObject.AddComponent<EndBeat>();
                appear.seconds = UIFx.Calm ? 0 : 1.3f + i * 0.12f;
                var group = row.gameObject.AddComponent<CanvasGroup>();
                group.alpha = UIFx.Calm ? 1 : 0;
                appear.done = () => group.alpha = 1;
            }

            var grade = UIKit.Rect(card, "Grade").Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-70, -170), new Vector2(360, 440));
            var circle = UIKit.Rect(grade, "Letter").gameObject.AddComponent<Image>();
            circle.sprite = UIKit.CircleSprite;
            circle.color = ResultsScreen.GradeColor(r.score.letter);
            circle.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), Vector2.zero, new Vector2(220, 220));
            UIKit.Label(circle.transform, r.score.letter, 140, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
            UIKit.Label(grade, r.score.total + " / 100", 44, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -235), new Vector2(360, 60));
            if (r.score.capNote.Length > 0)
                UIKit.Label(grade, r.score.capNote, 20, Palette.Amber, TextAnchor.UpperCenter)
                    .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -300), new Vector2(360, 60));
            if (r.tips.Count > 0) ResultsScreen.TipBanner(grade, "Next time: " + r.tips[0], new Vector2(-10, 352), new Vector2(380, 100), 22);
            var reveal = grade.gameObject.AddComponent<EndBeat>();
            reveal.seconds = UIFx.Calm ? 0 : BeatSeconds;
            var gradeGroup = grade.gameObject.AddComponent<CanvasGroup>();
            gradeGroup.alpha = UIFx.Calm ? 1 : 0;
            reveal.done = () => { gradeGroup.alpha = 1; UIFx.Pop(circle.transform); AudioDirector.Play(Sfx.MissionDone); };

            SkipButton(card);
            string menu = run.mode == GameMode.Free ? "free" : "story";
            EndButtons(card, ("View report", () => Flow.ShowReport(r), true), ("Try again", Flow.Retry, false), ("Menu", () => Flow.ToMenu(menu), false));
        }
    }

    // A timed beat on unscaled time: calls step(0..1) each frame, then done once. Finish() jumps to the end.
    public sealed class EndBeat : MonoBehaviour
    {
        public float seconds = 1;
        public System.Action<float> step;
        public System.Action done;
        float t;
        bool finished;

        void Update()
        {
            if (finished) return;
            t += Time.unscaledDeltaTime;
            float k = seconds <= 0 ? 1 : Mathf.Clamp01(t / seconds);
            step?.Invoke(k);
            if (k >= 1) Finish();
        }

        public void Finish()
        {
            if (finished) return;
            finished = true;
            step?.Invoke(1);
            done?.Invoke();
        }
    }

    public sealed class Drift : MonoBehaviour
    {
        public Vector2 velocity;
        void Update()
        {
            if (UIFx.Calm) return;
            var rt = (RectTransform)transform;
            var p = rt.anchoredPosition + velocity * Time.unscaledDeltaTime;
            if (p.x < -380) p.x += 760;
            if (p.y < 60) p.y += 260;
            rt.anchoredPosition = p;
        }
    }
}
