using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Report: "Evaluation complete" (time ran out, base inspected) or "Mission stopped" (a safety stop),
    // then the grade, the score rows, any cap, what happened, and what to try next.
    public sealed class ResultsScreen : MonoBehaviour
    {
        public MissionResult Result { get; private set; }

        public static Color GradeColor(string letter)
        {
            switch (letter)
            {
                case "S": return Palette.Amber;
                case "A": return Palette.Mint;
                case "B": return Palette.Azure;
                default: return Palette.Danger;
            }
        }

        void Start()
        {
            Result = Flow.LastResult ?? new MissionResult { title = "No result", score = Grader.Drawing(0, 0, 0, 0, 1), evaluation = true };
            var story = StoryData.Load();
            var root = UIKit.Canvas("Results Canvas");
            var back = UIKit.Rect(root, "Background").gameObject.AddComponent<Image>();
            back.color = Palette.Space;
            back.rectTransform.Stretch();

            var r = Result;
            string heading = r.Failed ? story.Debrief("title_fail") : story.Debrief("title_eval");
            UIKit.Label(root, heading, 56, r.Failed ? Palette.Amber : Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -70), new Vector2(1400, 80));
            UIKit.Label(root, r.title, 28, Palette.Ice, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -135), new Vector2(1400, 40));

            BuildGrade(root, r.score);
            BuildRows(root, r.score);
            BuildNotes(root, r);
            BuildButtons(root, r);
        }

        static void BuildGrade(RectTransform root, LevelScore s)
        {
            var box = UIKit.Panel(root, Palette.Panel, "Grade");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-480, 50), new Vector2(460, 560));
            var circle = UIKit.Rect(box.transform, "Circle").gameObject.AddComponent<Image>();
            circle.sprite = UIKit.CircleSprite;
            circle.color = GradeColor(s.letter);
            circle.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(230, 230));
            UIKit.Label(circle.transform, s.letter, 150, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
            UIFx.Pop(circle.transform);
            UIKit.Label(box.transform, s.total + " / 100", 48, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -300), new Vector2(400, 64));
            var stars = UIKit.Rect(box.transform, "Stars");
            stars.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -375), new Vector2(240, 70));
            var row = stars.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 12;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = row.childControlHeight = false;
            for (int i = 0; i < 3; i++) UIKit.Icon(stars, i < s.stars ? "star" : "star_empty", 64);
            if (s.capNote.Length > 0)
                UIKit.Label(box.transform, s.capNote + "\nPoints: " + s.rawTotal + ", grade: " + s.total + ".", 20, Palette.Amber, TextAnchor.UpperCenter)
                    .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -455), new Vector2(420, 60));
            UIKit.Label(box.transform, "S 81+   A 61+   B 31+   C 0+", 20, Palette.Muted, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 20), new Vector2(420, 30));
        }

        static void BuildRows(RectTransform root, LevelScore s)
        {
            var box = UIKit.Panel(root, Palette.Panel, "Score Rows");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, 185), new Vector2(940, 290));
            for (int i = 0; i < s.rows.Count; i++)
            {
                var row = s.rows[i];
                float y = 22 + i * 52;
                UIKit.Label(box.transform, row.label, 24, Palette.Hull).rectTransform.TopLeft(36, y, 330, 40);
                var track = UIKit.Panel(box.transform, Palette.Space, "Track");
                track.rectTransform.TopLeft(370, y + 8, 400, 24);
                var fill = UIKit.Panel(track.transform, row.points == row.max ? Palette.Mint : Palette.Cyan, "Fill");
                fill.rectTransform.TopLeft(0, 0, 400f * row.points / Mathf.Max(1, row.max), 24);
                UIKit.Label(box.transform, row.points + "/" + row.max, 24, Palette.Ice, TextAnchor.MiddleRight, FontStyle.Bold)
                    .rectTransform.TopLeft(780, y, 120, 40);
            }
        }

        static void BuildNotes(RectTransform root, MissionResult r)
        {
            var box = UIKit.Panel(root, Palette.Panel, "Notes");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, -130), new Vector2(940, 320));
            var text = new System.Text.StringBuilder();
            if (r.Failed) text.Append(r.headline).Append(' ').Append(r.cause).Append('\n');
            foreach (var note in r.notes) text.Append(note).Append('\n');
            UIKit.Label(box.transform, text.ToString().TrimEnd(), 22, Palette.Ice, TextAnchor.UpperLeft).rectTransform.TopLeft(36, 20, 870, 120);
            var tips = r.Failed ? r.fix : string.Join("  ", r.tips);
            if (r.blockedWhy.Length > 0) tips = r.blockedWhy + "\n" + tips;
            if (tips.Trim().Length > 0) TipBanner(box.transform, "Next time: " + tips, new Vector2(24, 144), new Vector2(892, 108), 24);
            if (!string.IsNullOrEmpty(r.science))
                UIKit.Label(box.transform, "Science: " + r.science, 22, Palette.Cyan, TextAnchor.UpperLeft, FontStyle.Italic)
                    .rectTransform.TopLeft(36, 260, 870, 50);
        }

        // Tips always use the same big yellow banner as in the game.
        public static void TipBanner(Transform parent, string text, Vector2 topLeft, Vector2 size, int fontSize)
        {
            var banner = UIKit.Panel(parent, Palette.Amber, "Tip");
            banner.rectTransform.TopLeft(topLeft.x, topLeft.y, size.x, size.y);
            UIKit.Label(banner.transform, Data.Keywords.Mark(text, true), fontSize, Palette.Ink, TextAnchor.MiddleLeft, FontStyle.Bold)
                .rectTransform.Stretch(18, 6, 18, 6);
        }

        static void BuildButtons(RectTransform root, MissionResult r)
        {
            var row = UIKit.Rect(root, "Buttons");
            row.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 100), new Vector2(1200, 80));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;

            if (r.mode == GameMode.Free)
            {
                Add(row, "Same map", Flow.Retry, Palette.PanelLight);
                Add(row, "New map", () => Flow.StartFree(new FreeSettings
                { planet = Flow.Free.planet, difficulty = Flow.Free.difficulty, hours = Flow.Free.hours, seed = FreeSettings.NewSeed() }), Palette.Azure);
                Add(row, "Menu", () => Flow.ToMenu("free"), Palette.PanelLight);
            }
            else
            {
                Add(row, r.canContinue ? "Play again" : "Try again", Flow.Retry, r.canContinue ? Palette.PanelLight : Palette.Azure);
                if (r.canContinue) Add(row, "Continue", Flow.Continue, Palette.Azure);
                Add(row, "Menu", () => Flow.ToMenu("story"), Palette.PanelLight);
            }

            UIKit.Label(root, "Game numbers are made up for play. Real NASA and CSA data is marked where it is used.", 20, Palette.Muted, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(1600, 30));
        }

        static void Add(Transform row, string label, UnityEngine.Events.UnityAction action, Color color)
        {
            var b = UIKit.Button(row, label, action, color, Palette.Hull, 30);
            b.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 76);
        }
    }
}
