using System;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Mapmakers.Data
{
    [Serializable] public class HelpRow { public string icon, text; }
    [Serializable] public class HelpCard { public string icon, title, text; }
    [Serializable] public class LessonReply { public string text, answer; }
    [Serializable] public class Speaker { public string id, name, role; }

    [Serializable]
    public class LessonStep
    {
        public string speaker, text, more, button;
        public bool flow;
        public HelpCard[] cards = new HelpCard[0];
        public LessonReply[] replies = new LessonReply[0];
    }

    [Serializable]
    public class Lesson
    {
        public string id, title, image, fallback;
        public bool required;
        public LessonStep[] steps = new LessonStep[0];
    }

    [Serializable]
    public class WikiPage
    {
        public string id, section, title, icon, summary, best, building, resource, crew, lesson;
        public HelpRow[] rows = new HelpRow[0];
    }

    // Wiki pages and onboarding lessons from Help.json. Story.json keeps the story and its word limits.
    [Serializable]
    public class HelpData
    {
        public string note;
        public string[] sections;
        public Speaker[] speakers;
        public Lesson[] lessons;
        public WikiPage[] wiki;

        public const string ResourcePath = "Help";

        static HelpData cached;
        public static HelpData Load()
        {
            if (cached == null) cached = JsonUtility.FromJson<HelpData>(Resources.Load<TextAsset>(ResourcePath).text);
            return cached;
        }

        public Lesson Lesson(string id) => string.IsNullOrEmpty(id) ? null : Array.Find(lessons, l => l.id == id);
        public WikiPage Page(string id) => Array.Find(wiki, p => p.id == id);
        public WikiPage BuildingPage(string type) => Array.Find(wiki, p => p.building == type);
        public WikiPage ResourcePage(string id) => Array.Find(wiki, p => p.resource == id);
        public Speaker Speaker(string id) => Array.Find(speakers, s => s.id == id);
    }

    // Shows building and resource names in the core blue, in bold, so meaning never depends on colour alone.
    public static class Keywords
    {
        public const string OnDark = "#60D5E5";
        public const string OnLight = "#123E73";

        static Regex pattern;

        static Regex Pattern
        {
            get
            {
                if (pattern != null) return pattern;
                string[] words =
                {
                    "Air Support", "Radio Dish", "Rover Garage", "Solar Arrays", "Solar Array", "Batteries", "Battery",
                    "Habitat", "Greenhouse", "Shelter", "Crew safety", "power", "air", "water", "food", "soil", "supplies"
                };
                pattern = new Regex(@"\b(" + string.Join("|", Array.ConvertAll(words, Regex.Escape)) + @")\b", RegexOptions.IgnoreCase);
                return pattern;
            }
        }

        public static string Mark(string text, bool onLight = false)
        {
            if (string.IsNullOrEmpty(text)) return text;
            string color = onLight ? OnLight : OnDark;
            return Pattern.Replace(text, m => "<b><color=" + color + ">" + m.Value + "</color></b>");
        }
    }
}
