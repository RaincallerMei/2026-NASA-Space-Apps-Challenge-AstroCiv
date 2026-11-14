using System;
using System.Collections.Generic;
using UnityEngine;

namespace Mapmakers.Data
{
    [Serializable] public class StoryLine { public string id, text; }
    [Serializable] public class CrewMember { public string id, name, role, owns; }
    [Serializable] public class ChatReply { public string text, answer, action; public int heart; }
    [Serializable] public class ChatEntry { public string crew, when, line; public ChatReply[] replies; }
    [Serializable] public class ScienceCard { public string id, title, text, text2, source, label; }

    [Serializable]
    public class StoryData
    {
        public string note;
        public CrewMember[] crew;
        public StoryLine[] story;
        public ChatEntry[] chat;
        public StoryLine[] hints;
        public ScienceCard[] cards;
        public StoryLine[] debrief;

        public const string ResourcePath = "Story";
        public const int MaxStoryLineWords = 12;
        public const int MaxChatLineWords = 14;
        public const int MaxStoryWords = 300;
        public const int MaxStoryAndChatWords = 650;

        public static StoryData Parse(string json) => JsonUtility.FromJson<StoryData>(json);
        public static StoryData Load() => Parse(Resources.Load<TextAsset>(ResourcePath).text);

        public string Story(string id) => Find(story, id);
        public string Hint(string id) => Find(hints, id);
        public string Debrief(string id) => Find(debrief, id);

        public ScienceCard Card(string id)
        {
            foreach (var card in cards)
                if (card.id == id) return card;
            return null;
        }

        public CrewMember Crew(string id)
        {
            foreach (var member in crew)
                if (member.id == id) return member;
            return null;
        }

        public ChatEntry Chat(string crewId, string when)
        {
            foreach (var entry in chat)
                if (entry.crew == crewId && entry.when == when) return entry;
            return null;
        }

        static string Find(StoryLine[] lines, string id)
        {
            foreach (var line in lines)
                if (line.id == id) return line.text;
            return "[" + id + "]";
        }

        public static int WordCount(string text)
        {
            if (string.IsNullOrEmpty(text)) return 0;
            return text.Split(new[] { ' ', '\n', '\t' }, StringSplitOptions.RemoveEmptyEntries).Length;
        }

        public IEnumerable<string> ChatTexts()
        {
            foreach (var entry in chat)
            {
                yield return entry.line;
                foreach (var reply in entry.replies)
                {
                    yield return reply.text;
                    yield return reply.answer;
                }
            }
        }
    }
}
