using System.Collections;
using Mapmakers.Data;
using Mapmakers.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // A short guided talk: one speaker, one sentence and a row of picture cards per step.
    // First time through, a required lesson cannot be skipped; "Tell me more" and the reply buttons are optional.
    public sealed class LessonScreen : MonoBehaviour
    {
        Lesson lesson;
        StoryData story;
        HelpData help;
        int step;
        bool done;
        RectTransform dialog, cards, options;
        CanvasGroup dialogGroup, fade;
        Image portrait;
        Text portraitLetter, speakerName, speakerRole, body, extra, counter;
        Button next, back;

        public int Step => step;
        public bool Done => done;
        public bool CanSkip { get; private set; }
        public Lesson Lesson => lesson;

        void Start()
        {
            help = HelpData.Load();
            story = StoryData.Load();
            lesson = help.Lesson(Flow.CutsceneId);
            CanSkip = !lesson.required || Progress.LessonSeen(lesson.id);
            Build();
            Show(0);
            StartCoroutine(FadeIn());
        }

        void Build()
        {
            var root = UIKit.Canvas("Lesson Canvas");
            var back0 = UIKit.Rect(root, "Black").gameObject.AddComponent<Image>();
            back0.color = Color.black;
            back0.rectTransform.Stretch();
            var sprite = CutsceneScreen.LoadPicture(lesson.image);
            if (sprite == null && !string.IsNullOrEmpty(lesson.fallback)) sprite = CutsceneScreen.LoadPicture(lesson.fallback);
            if (sprite != null)
            {
                var picture = UIKit.Rect(root, "Picture").gameObject.AddComponent<Image>();
                picture.sprite = sprite;
                picture.color = new Color(0.62f, 0.66f, 0.75f);
                picture.raycastTarget = false;
                var fit = picture.gameObject.AddComponent<AspectRatioFitter>();
                fit.aspectMode = AspectRatioFitter.AspectMode.EnvelopeParent;
                fit.aspectRatio = sprite.rect.width / sprite.rect.height;
                UIKit.AiWatermark(root, new Vector2(0, 1));
            }

            var chip = UIKit.Panel(root, Palette.Alpha(Palette.Ink, 0.92f), "Title");
            chip.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -24), new Vector2(760, 64));
            UIKit.Label(chip.transform, lesson.title, 32, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.Stretch(24, 0, 200, 0);
            counter = UIKit.Label(chip.transform, "", 24, Palette.Cyan, TextAnchor.MiddleRight, FontStyle.Bold);
            counter.rectTransform.Stretch(0, 0, 24, 0);

            var wiki = UIKit.Button(root, "Wiki", () => Wiki.Open(), Palette.Alpha(Palette.Panel, 0.92f), Palette.Hull, 24);
            wiki.GetComponent<RectTransform>().Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-32 - (CanSkip ? 220 : 0), -28), new Vector2(150, 56));
            if (CanSkip)
            {
                var skip = UIKit.Button(root, "Skip lesson", Finish, Palette.Alpha(Palette.Panel, 0.92f), Palette.Hull, 24);
                skip.GetComponent<RectTransform>().Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-32, -28), new Vector2(200, 56));
            }

            var panel = UIKit.Panel(root, Palette.Alpha(Palette.Panel, 0.97f), "Dialog");
            panel.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, 36), new Vector2(1680, 590));
            var outline = panel.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Cyan;
            outline.effectDistance = new Vector2(3, -3);
            dialog = panel.rectTransform;
            dialogGroup = panel.gameObject.AddComponent<CanvasGroup>();

            portrait = UIKit.Rect(dialog, "Portrait").gameObject.AddComponent<Image>();
            portrait.sprite = UIKit.CircleSprite;
            portrait.rectTransform.TopLeft(40, 36, 128, 128);
            portraitLetter = UIKit.Label(portrait.transform, "", 60, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            portraitLetter.rectTransform.Stretch();
            speakerName = UIKit.Label(dialog, "", 28, Palette.Hull, TextAnchor.UpperCenter, FontStyle.Bold);
            speakerName.rectTransform.TopLeft(16, 172, 176, 36);
            speakerRole = UIKit.Label(dialog, "", 22, Palette.Muted, TextAnchor.UpperCenter);
            speakerRole.rectTransform.TopLeft(16, 208, 176, 30);

            body = UIKit.Label(dialog, "", 36, Palette.Hull, TextAnchor.UpperLeft);
            cards = UIKit.Rect(dialog, "Cards");
            var row = cards.gameObject.AddComponent<HorizontalLayoutGroup>();
            row.spacing = 14;
            row.childAlignment = TextAnchor.MiddleLeft;
            row.childControlWidth = row.childControlHeight = false;
            row.childForceExpandWidth = row.childForceExpandHeight = false;
            extra = UIKit.Label(dialog, "", 26, Palette.Ice, TextAnchor.UpperLeft, FontStyle.Italic);

            options = UIKit.Rect(dialog, "Options").TopLeft(220, 506, 1000, 64);
            var optionRow = options.gameObject.AddComponent<HorizontalLayoutGroup>();
            optionRow.spacing = 14;
            optionRow.childControlWidth = optionRow.childControlHeight = false;
            optionRow.childForceExpandWidth = optionRow.childForceExpandHeight = false;

            back = UIKit.Button(dialog, "Back", Back, Palette.PanelLight, Palette.Hull, 28);
            back.GetComponent<RectTransform>().TopLeft(1680 - 470, 506, 170, 64);
            next = UIKit.Button(dialog, "Next", Next, Palette.Azure, Palette.Hull, 30);
            next.GetComponent<RectTransform>().TopLeft(1680 - 284, 506, 248, 64);

            var cover = UIKit.Rect(root, "Fade").gameObject.AddComponent<Image>();
            cover.color = Color.black;
            cover.raycastTarget = false;
            cover.rectTransform.Stretch();
            fade = cover.gameObject.AddComponent<CanvasGroup>();
            fade.blocksRaycasts = false;
        }

        IEnumerator FadeIn()
        {
            if (UIFx.Calm) { fade.alpha = 0; yield break; }
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 3) { fade.alpha = 1 - t; yield return null; }
            fade.alpha = 0;
        }

        void Update()
        {
            var keys = Keyboard.current;
            if (keys == null || done || Wiki.IsOpen) return;
            if (keys.enterKey.wasPressedThisFrame || keys.spaceKey.wasPressedThisFrame || keys.rightArrowKey.wasPressedThisFrame) Next();
            if (keys.backspaceKey.wasPressedThisFrame || keys.leftArrowKey.wasPressedThisFrame) Back();
            if (keys.escapeKey.wasPressedThisFrame && CanSkip) Finish();
        }

        public void Next()
        {
            if (done) return;
            AudioDirector.Play(Sfx.LineNext);
            if (step + 1 < lesson.steps.Length) Show(step + 1);
            else Finish();
        }

        public void Back()
        {
            if (done || step == 0) return;
            AudioDirector.Play(Sfx.LineBack);
            Show(step - 1);
        }

        public void Finish()
        {
            if (done) return;
            done = true;
            Flow.SceneDone(lesson.id);
        }

        void Show(int index)
        {
            step = index;
            var s = lesson.steps[index];
            counter.text = (index + 1) + " / " + lesson.steps.Length;
            SetSpeaker(s.speaker);
            bool hasPortrait = portrait.gameObject.activeSelf;
            float left = hasPortrait ? 220 : 48;
            float width = 1680 - left - 48;
            body.text = Keywords.Mark(s.text);
            body.rectTransform.TopLeft(left, 40, width, 110);

            UIKit.Clear(cards);
            cards.TopLeft(left, 160, width, 200);
            for (int i = 0; i < s.cards.Length; i++)
            {
                if (s.flow && i > 0) FlowArrow();
                Card(s.cards[i], s.cards.Length > 4 ? 200 : 260);
            }

            extra.text = "";
            extra.rectTransform.TopLeft(left, 378, width, 110);
            UIKit.Clear(options);
            options.TopLeft(left, 506, 1680 - left - 500, 64);
            foreach (var reply in s.replies)
            {
                var chosen = reply;
                Option(reply.text, () => extra.text = Keywords.Mark(SpeakerName(s.speaker) + ": \"" + chosen.answer + "\""));
            }
            if (!string.IsNullOrEmpty(s.more)) Option("Tell me more", () => extra.text = Keywords.Mark(s.more));

            back.interactable = index > 0;
            UIKit.LabelOf(next).text = index + 1 < lesson.steps.Length ? "Next" : string.IsNullOrEmpty(s.button) ? "Finish" : s.button;
            if (!UIFx.Calm) StartCoroutine(Appear());
        }

        IEnumerator Appear()
        {
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 6) { dialogGroup.alpha = 0.4f + 0.6f * t; yield return null; }
            dialogGroup.alpha = 1;
        }

        void SetSpeaker(string id)
        {
            var crew = story.Crew(id);
            var speaker = help.Speaker(id);
            bool narrator = crew == null && (speaker == null || string.IsNullOrEmpty(speaker.name));
            portrait.gameObject.SetActive(!narrator);
            speakerName.text = narrator ? "" : crew != null ? crew.name : speaker.name;
            speakerRole.text = narrator ? "" : crew != null ? crew.role : speaker.role;
            if (narrator) return;
            portrait.color = crew != null ? GameScreen.CrewColor(id) : id == "you" ? Palette.Azure : Palette.LunarGray;
            portraitLetter.text = id == "you" ? "You" : crew != null ? crew.name.Substring(0, 1) : speakerRole.text.Substring(0, 1);
            portraitLetter.fontSize = id == "you" ? 36 : 60;
        }

        string SpeakerName(string id)
        {
            var crew = story.Crew(id);
            if (crew != null) return crew.name;
            var speaker = help.Speaker(id);
            return speaker != null && !string.IsNullOrEmpty(speaker.name) ? speaker.name : "";
        }

        void Card(HelpCard card, float width)
        {
            var box = UIKit.Panel(cards, Palette.PanelLight, "Card");
            box.rectTransform.sizeDelta = new Vector2(width, 196);
            var icon = UIKit.Icon(box.transform, card.icon, 72);
            icon.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -12), new Vector2(72, 72));
            UIKit.Label(box.transform, Keywords.Mark(card.title), 24, Palette.Hull, TextAnchor.UpperCenter, FontStyle.Bold)
                .rectTransform.TopLeft(10, 90, width - 20, 32);
            UIKit.Label(box.transform, Keywords.Mark(card.text), 21, Palette.Ice, TextAnchor.UpperCenter)
                .rectTransform.TopLeft(10, 124, width - 20, 70);
            UIFx.Pop(box.transform);
        }

        void FlowArrow()
        {
            var arrow = UIKit.Rect(cards, "Arrow").gameObject.AddComponent<Image>();
            arrow.sprite = UIKit.ArrowSprite;
            arrow.color = Palette.Amber;
            arrow.raycastTarget = false;
            arrow.rectTransform.sizeDelta = new Vector2(44, 44);
            arrow.rectTransform.localRotation = Quaternion.Euler(0, 0, 90);
        }

        void Option(string label, UnityEngine.Events.UnityAction action)
        {
            var b = UIKit.Button(options, label, action, Palette.PanelLight, Palette.Ice, 22);
            float w = Mathf.Clamp(label.Length * 13 + 40, 200, 470);
            b.GetComponent<RectTransform>().sizeDelta = new Vector2(w, 64);
        }
    }
}
