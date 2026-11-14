using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Grade screen: letter (S/A/B/C), score out of 100, stars, why, and what to do next.
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
            Result = Flow.LastResult ?? new MissionResult { title = "No result", score = Grader.Drawing(0, 0, 0, 0, 1) };
            var story = StoryData.Load();
            var root = UIKit.Canvas("Results Canvas");
            var back = UIKit.Rect(root, "Background").gameObject.AddComponent<Image>();
            back.color = Palette.Space;
            back.rectTransform.Stretch();

            var r = Result;
            var s = r.score;
            string heading = r.failed ? story.Debrief("title_fail") : s.passed ? (r.mode == GameMode.Free ? "Run complete!" : "Level complete!") : "Not finished yet.";
            UIKit.Label(root, heading, 56, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -80), new Vector2(1400, 80));
            UIKit.Label(root, r.title, 28, Palette.Ice, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -145), new Vector2(1400, 40));

            BuildGrade(root, s);
            BuildRows(root, s);
            BuildNotes(root, r);
            BuildButtons(root, r);
        }

        static void BuildGrade(RectTransform root, LevelScore s)
        {
            var box = UIKit.Panel(root, Palette.Panel, "Grade");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(-480, 60), new Vector2(460, 520));
            var circle = UIKit.Rect(box.transform, "Circle").gameObject.AddComponent<Image>();
            circle.sprite = UIKit.CircleSprite;
            circle.color = GradeColor(s.letter);
            circle.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(230, 230));
            UIKit.Label(circle.transform, s.letter, 150, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
            UIKit.Label(box.transform, s.total + " / 100", 48, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -300), new Vector2(400, 64));
            var stars = UIKit.Rect(box.transform, "Stars");
            stars.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -385), new Vector2(240, 70));
            var row = stars.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 12;
            row.childAlignment = TextAnchor.MiddleCenter;
            row.childControlWidth = row.childControlHeight = false;
            for (int i = 0; i < 3; i++) UIKit.Icon(stars, i < s.stars ? "star" : "star_empty", 64);
            UIKit.Label(box.transform, "S 81+   A 61+   B 31+   C 0+", 20, Palette.Muted, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 30), new Vector2(420, 30));
        }

        static void BuildRows(RectTransform root, LevelScore s)
        {
            var box = UIKit.Panel(root, Palette.Panel, "Score Rows");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, 165), new Vector2(940, 310));
            for (int i = 0; i < s.rows.Count; i++)
            {
                var row = s.rows[i];
                float y = 30 + i * 66;
                UIKit.Label(box.transform, row.label, 26, Palette.Hull).rectTransform.TopLeft(36, y, 330, 40);
                var track = UIKit.Panel(box.transform, Palette.Space, "Track");
                track.rectTransform.TopLeft(370, y + 8, 400, 26);
                var fill = UIKit.Panel(track.transform, row.points == row.max ? Palette.Mint : Palette.Cyan, "Fill");
                fill.rectTransform.TopLeft(0, 0, 400f * row.points / Mathf.Max(1, row.max), 26);
                UIKit.Label(box.transform, row.points + "/" + row.max, 26, Palette.Ice, TextAnchor.MiddleRight, FontStyle.Bold)
                    .rectTransform.TopLeft(780, y, 120, 40);
            }
        }

        static void BuildNotes(RectTransform root, MissionResult r)
        {
            var box = UIKit.Panel(root, Palette.Panel, "Notes");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(250, -110), new Vector2(940, 210));
            var text = string.Join("\n", r.notes);
            UIKit.Label(box.transform, text, 24, Palette.Ice, TextAnchor.UpperLeft).rectTransform.TopLeft(36, 24, 870, 110);
            if (!string.IsNullOrEmpty(r.science))
                UIKit.Label(box.transform, "Science: " + r.science, 24, Palette.Cyan, TextAnchor.UpperLeft, FontStyle.Italic)
                    .rectTransform.TopLeft(36, 140, 870, 60);
        }

        static void BuildButtons(RectTransform root, MissionResult r)
        {
            var row = UIKit.Rect(root, "Buttons");
            row.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 120), new Vector2(1200, 80));
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 24;
            layout.childAlignment = TextAnchor.MiddleCenter;
            layout.childControlWidth = layout.childControlHeight = false;

            if (r.mode == GameMode.Free)
            {
                Add(row, "Same map", Flow.Retry, Palette.PanelLight);
                Add(row, "New map", () => Flow.StartFree(new FreeSettings
                { planet = Flow.Free.planet, difficulty = Flow.Free.difficulty, days = Flow.Free.days, seed = FreeSettings.NewSeed() }), Palette.Azure);
                Add(row, "Menu", () => Flow.ToMenu("free"), Palette.PanelLight);
            }
            else
            {
                Add(row, r.score.passed ? "Play again" : "Try again", Flow.Retry, r.score.passed ? Palette.PanelLight : Palette.Azure);
                if (r.score.passed) Add(row, "Continue", Flow.Continue, Palette.Azure);
                Add(row, "Menu", () => Flow.ToMenu("story"), Palette.PanelLight);
            }

            UIKit.Label(root, "Game numbers are made up for play. Real NASA and CSA data is marked where it is used.", 20, Palette.Muted, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 50), new Vector2(1600, 30));
        }

        static void Add(Transform row, string label, UnityEngine.Events.UnityAction action, Color color)
        {
            var b = UIKit.Button(row, label, action, color, Palette.Hull, 30);
            b.GetComponent<RectTransform>().sizeDelta = new Vector2(300, 76);
        }
    }
}
