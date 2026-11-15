using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Title, Story Mode level list, Free Build setup and credits. One panel shows at a time.
    public sealed class MenuScreen : MonoBehaviour
    {
        public const string CreditsText =
            "Data: NASA DONKI, NASA OSDR RadLab, NASA POWER, LRO/LOLA and MGS/MOLA (PDS), CSA. Not endorsed by NASA or CSA.\n" +
            "CSA content © Canadian Space Agency.\n\n" +
            "All game art is AI-created for this project:\n" +
            "story pictures, AstroCiv pixel sprites and code-drawn shapes.\n\n" +
            "Power, cost and safety numbers are made up for the game.\n" +
            "Chris Hadfield and Sara Seager are named as inspiration only.\nThey do not endorse this game.";

        public const string AudioCreditsText =
            "Music: \"Dream Culture\", \"Space Jazz\" and \"Cipher\" by Kevin MacLeod (incompetech.com).\n" +
            "Licensed under Creative Commons: By Attribution 4.0 License. creativecommons.org/licenses/by/4.0/\n" +
            "Changes: volume levelled and compressed for the web; gameplay songs play in random order.\n" +
            "Music: \"Cat caffe\" and \"Cue\" by TAD (opengameart.org/content/lofi-compilation). CC0.\n" +
            "Sound effects: \"Interface Sounds\" by Kenney (kenney.nl). CC0.";

        RectTransform root;
        readonly Dictionary<string, GameObject> panels = new Dictionary<string, GameObject>();
        readonly FreeSettings free = new FreeSettings { seed = FreeSettings.NewSeed() };
        Text seedLabel;
        readonly List<(Button button, string value)> planetButtons = new List<(Button, string)>();
        readonly List<(Button button, Difficulty value)> difficultyButtons = new List<(Button, Difficulty)>();

        public string Current { get; private set; }

        void Start()
        {
            root = UIKit.Canvas("Menu Canvas");
            var back = UIKit.Rect(root, "Background").gameObject.AddComponent<Image>();
            back.color = Palette.Space;
            back.rectTransform.Stretch();
            var art = UIKit.Rect(root, "Art").gameObject.AddComponent<Image>();
            art.sprite = CutsceneScreen.LoadPicture("colony");
            art.color = art.sprite ? new Color(0.35f, 0.4f, 0.55f, 1) : Palette.Space;
            art.raycastTarget = false;
            var fit = art.gameObject.AddComponent<AspectRatioFitter>();
            fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
            fit.aspectRatio = art.sprite ? art.sprite.rect.width / art.sprite.rect.height : 16f / 9f;

            if (art.sprite) UIKit.AiWatermark(root, new Vector2(1, 1));
            panels["main"] = BuildMain();
            panels["story"] = BuildStory();
            panels["free"] = BuildFree();
            panels["credits"] = BuildCredits();
            Show(Flow.MenuPanel);
            Flow.MenuPanel = "main";
        }

        public void Show(string name)
        {
            if (!panels.ContainsKey(name)) name = "main";
            if (name == "story") RefreshStory();
            foreach (var pair in panels) pair.Value.SetActive(pair.Key == name);
            Current = name;
        }

        GameObject BuildMain()
        {
            var panel = UIKit.Rect(root, "Main").Stretch();
            var title = UIKit.Label(panel, "Mapmakers of the Moon", 84, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold);
            title.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -170), new Vector2(1400, 110));
            var tag = UIKit.Label(panel, "Every great civilization starts with a dream.", 32, Palette.Ice, TextAnchor.MiddleCenter, FontStyle.Italic);
            tag.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -280), new Vector2(1400, 50));

            MenuButton(panel, "Story Mode", -430, () => Show("story"), Palette.Azure);
            MenuButton(panel, "Free Build", -530, () => Show("free"), Palette.Azure);
            MenuButton(panel, "Wiki & Help", -630, () => Wiki.Open(null, null, true), Palette.PanelLight);
            MenuButton(panel, "Data & Credits", -730, () => Show("credits"), Palette.PanelLight);

            SoundToggle(panel, 0, "Music", () => Progress.MusicOn, v => Progress.MusicOn = v);
            SoundToggle(panel, 1, "Sounds", () => Progress.SoundsOn, v => Progress.SoundsOn = v);

            var foot = UIKit.Label(panel, "Not endorsed by NASA or CSA.", 20, Palette.Muted, TextAnchor.MiddleCenter);
            foot.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 40), new Vector2(800, 30));
            return panel.gameObject;
        }

        static void SoundToggle(Transform parent, int slot, string name, System.Func<bool> get, System.Action<bool> set)
        {
            Button button = null;
            button = UIKit.Button(parent, name + ": " + (get() ? "On" : "Off"), () =>
            {
                set(!get());
                UIKit.LabelOf(button).text = name + ": " + (get() ? "On" : "Off");
            }, Palette.PanelLight, Palette.Hull, 22);
            button.GetComponent<RectTransform>().Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-40 - slot * 196, 40), new Vector2(180, 52));
        }

        static void MenuButton(Transform parent, string text, float y, UnityEngine.Events.UnityAction action, Color color)
        {
            var b = UIKit.Button(parent, text, action, color, Palette.Hull, 34);
            b.GetComponent<RectTransform>().Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, y), new Vector2(460, 80));
        }

        // ---------- Story Mode ----------

        readonly List<(LevelDef level, Button play, Text grade, Transform stars, Image card)> levelCards = new List<(LevelDef, Button, Text, Transform, Image)>();

        GameObject BuildStory()
        {
            var panel = UIKit.Rect(root, "Story").Stretch();
            Header(panel, "Story Mode", "Two short tutorials, then three missions. Each one unlocks the next.");
            var levels = LevelCatalog.Load().levels;
            float width = 344, gap = 22, start = -(levels.Length * width + (levels.Length - 1) * gap) / 2 + width / 2;
            for (int i = 0; i < levels.Length; i++)
            {
                var level = levels[i];
                var card = UIKit.Panel(panel, Palette.Alpha(Palette.Panel, 0.94f), "Level " + level.id);
                card.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(start + i * (width + gap), -10), new Vector2(width, 470));

                bool tutorial = level.IsDrawing || level.IsTraining;
                var number = UIKit.Panel(card.transform, level.planet == "Mars" ? Palette.MarsRust : tutorial ? Palette.Leaf : Palette.Azure, "Number");
                number.rectTransform.TopLeft(24, 24, 72, 72);
                UIKit.Label(number.transform, level.Label.Substring(level.Label.LastIndexOf(' ') + 1), 40, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold).rectTransform.Stretch();
                UIKit.Label(card.transform, level.Label, 24, level.planet == "Mars" ? Palette.MarsSand : Palette.Cyan, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.TopLeft(112, 28, 220, 30);
                string when = level.IsTraining ? "Practice run" : level.IsDrawing ? "Learn to build" : "About " + Mathf.CeilToInt((float)(level.durationHours * BalanceLoader.Load().realSecondsPerGameHour / 60)) + " min at 1×";
                UIKit.Label(card.transform, level.planet + " · " + when, 20, Palette.Ice).rectTransform.TopLeft(112, 62, 220, 30);

                UIKit.Label(card.transform, level.title, 36, Palette.Hull, TextAnchor.UpperLeft, FontStyle.Bold).rectTransform.TopLeft(24, 120, width - 48, 50);
                UIKit.Label(card.transform, level.subtitle, 24, Palette.Ice, TextAnchor.UpperLeft).rectTransform.TopLeft(24, 176, width - 48, 70);

                var grade = UIKit.Label(card.transform, "", 30, Palette.Amber, TextAnchor.MiddleLeft, FontStyle.Bold);
                grade.rectTransform.TopLeft(24, 270, width - 48, 40);
                var stars = UIKit.Rect(card.transform, "Stars").TopLeft(24, 316, 200, 40);
                var row = stars.gameObject.AddComponent<HorizontalLayoutGroup>();
                row.spacing = 8;
                row.childControlWidth = row.childControlHeight = false;
                row.childForceExpandWidth = row.childForceExpandHeight = false;

                string id = level.id;
                var play = UIKit.Button(card.transform, "Play", () => Flow.StartLevel(id), Palette.Azure, Palette.Hull, 30);
                play.GetComponent<RectTransform>().TopLeft(24, 384, width - 48, 64);
                levelCards.Add((level, play, grade, stars, card));
            }

            var reset = UIKit.Button(panel, "Reset progress", () => { Progress.ResetAll(); RefreshStory(); }, Palette.PanelLight, Palette.Ice, 20);
            reset.GetComponent<RectTransform>().Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-40, 40), new Vector2(220, 52));
            BackButton(panel);
            return panel.gameObject;
        }

        void RefreshStory()
        {
            foreach (var (level, play, grade, stars, card) in levelCards)
            {
                bool open = Progress.IsUnlocked(level.id);
                int best = Progress.Best(level.id);
                play.interactable = open;
                UIKit.LabelOf(play).text = open ? (best >= 0 ? "Play again" : "Play") : "Locked";
                card.color = Palette.Alpha(open ? Palette.Panel : Palette.Space, 0.94f);
                var catalog = LevelCatalog.Load();
                int index = catalog.IndexOf(level.id);
                string previous = index > 0 ? catalog.levels[index - 1].Label : "";
                grade.text = !open ? "Finish " + previous + " first" : level.IsTraining ? (Progress.Passed(level.id) ? "Done!" : "New!")
                    : best >= 0 ? "Best: " + Grader.Letter(best) + "  " + best + "/100" : "New!";
                if (level.IsTraining && open) UIKit.LabelOf(play).text = Progress.Passed(level.id) ? "Play again" : "Start";
                grade.fontSize = open ? 30 : 22;
                grade.color = open ? Palette.Amber : Palette.Muted;
                UIKit.Clear(stars);
                if (best < 0) continue;
                int count = Grader.Stars(Grader.Letter(best));
                for (int s = 0; s < 3; s++) UIKit.Icon(stars, s < count ? "star" : "star_empty", 36);
            }
        }

        // ---------- Free Build ----------

        GameObject BuildFree()
        {
            var panel = UIKit.Rect(root, "Free").Stretch();
            Header(panel, "Free Build", "No story. A new map and new surprises every time.");
            var box = UIKit.Panel(panel, Palette.Alpha(Palette.Panel, 0.94f), "Settings");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -20), new Vector2(900, 470));

            UIKit.Label(box.transform, "World", 28, Palette.Ice).rectTransform.TopLeft(40, 40, 200, 64);
            for (int i = 0; i < 2; i++)
            {
                string planet = i == 0 ? "Moon" : "Mars";
                var b = UIKit.Button(box.transform, planet, () => { free.planet = planet; RefreshFree(); }, Palette.PanelLight, Palette.Hull, 26);
                b.GetComponent<RectTransform>().TopLeft(260 + i * 210, 40, 190, 64);
                planetButtons.Add((b, planet));
            }

            UIKit.Label(box.transform, "Challenge", 28, Palette.Ice).rectTransform.TopLeft(40, 130, 200, 64);
            var levels = new[] { Difficulty.Easy, Difficulty.Normal, Difficulty.Hard };
            for (int i = 0; i < levels.Length; i++)
            {
                var d = levels[i];
                var b = UIKit.Button(box.transform, d.ToString(), () => { free.difficulty = d; RefreshFree(); }, Palette.PanelLight, Palette.Hull, 26);
                b.GetComponent<RectTransform>().TopLeft(260 + i * 210, 130, 190, 64);
                difficultyButtons.Add((b, d));
            }

            UIKit.Label(box.transform, "Map", 28, Palette.Ice).rectTransform.TopLeft(40, 220, 200, 64);
            seedLabel = UIKit.Label(box.transform, "", 28, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            seedLabel.rectTransform.TopLeft(260, 220, 190, 64);
            var reroll = UIKit.Button(box.transform, "New map", () => { free.seed = FreeSettings.NewSeed(); RefreshFree(); }, Palette.PanelLight, Palette.Hull, 26);
            reroll.GetComponent<RectTransform>().TopLeft(470, 220, 190, 64);

            UIKit.Label(box.transform, Sim.SimText.Duration(free.hours) + ". Storms, shadows and surprises come at random times.", 22, Palette.Muted)
                .rectTransform.TopLeft(40, 300, 820, 40);
            var start = UIKit.Button(box.transform, "Start", () => Flow.StartFree(new FreeSettings
            { planet = free.planet, difficulty = free.difficulty, seed = free.seed, hours = free.hours }), Palette.Azure, Palette.Hull, 32);
            start.GetComponent<RectTransform>().TopLeft(260, 360, 400, 76);

            BackButton(panel);
            RefreshFree();
            return panel.gameObject;
        }

        void RefreshFree()
        {
            foreach (var (button, value) in planetButtons) button.image.color = value == free.planet ? Palette.Azure : Palette.PanelLight;
            foreach (var (button, value) in difficultyButtons) button.image.color = value == free.difficulty ? Palette.Azure : Palette.PanelLight;
            seedLabel.text = "#" + free.seed;
        }

        // ---------- Credits ----------

        GameObject BuildCredits()
        {
            var panel = UIKit.Rect(root, "Credits").Stretch();
            Header(panel, "Data & Credits", "");
            var box = UIKit.Panel(panel, Palette.Alpha(Palette.Panel, 0.94f), "Credits Box");
            box.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), new Vector2(0, -10), new Vector2(1560, 740));
            UIKit.Label(box.transform, CreditsText + "\n\n" + AudioCreditsText, 24, Palette.Hull, TextAnchor.MiddleCenter).rectTransform.Stretch(40, 30, 40, 30);
            BackButton(panel);
            return panel.gameObject;
        }

        // ---------- shared ----------

        static void Header(Transform panel, string title, string subtitle)
        {
            UIKit.Label(panel, title, 60, Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -90), new Vector2(1400, 80));
            if (subtitle.Length == 0) return;
            UIKit.Label(panel, subtitle, 28, Palette.Ice, TextAnchor.MiddleCenter)
                .rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -160), new Vector2(1400, 44));
        }

        void BackButton(Transform panel)
        {
            var back = UIKit.Button(panel, "Back", () => Show("main"), Palette.PanelLight, Palette.Hull, 26);
            back.GetComponent<RectTransform>().Anchor(new Vector2(0, 0), new Vector2(0, 0), new Vector2(40, 40), new Vector2(180, 60));
        }
    }
}
