using System.Collections;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Full-screen story pictures with one short line at a time. Click, Space, Enter or Right continues; Left or Back goes back a line. Skip leaves.
    public sealed class CutsceneScreen : MonoBehaviour
    {
        CutsceneDef cutscene;
        StoryData story;
        Image picture;
        AspectRatioFitter fitter;
        CanvasGroup pictureGroup;
        Text caption;
        Text counter;
        Button backButton;
        int slide, line;
        bool busy, done;

        public int Slide => slide;
        public int Line => line;
        public bool Done => done;

        void Start()
        {
            if (HelpData.Load().Lesson(Flow.CutsceneId) != null)
            {
                gameObject.AddComponent<LessonScreen>();
                enabled = false;
                return;
            }
            cutscene = LevelCatalog.Load().Cutscene(Flow.CutsceneId);
            story = StoryData.Load();
            if (cutscene == null || cutscene.slides.Length == 0)
            {
                Leave();
                return;
            }
            Build();
            ShowSlide(0, false);
        }

        void Build()
        {
            var root = UIKit.Canvas("Cutscene Canvas");
            var back = UIKit.Rect(root, "Black").gameObject.AddComponent<Image>();
            back.color = Color.black;
            back.rectTransform.Stretch();
            var advance = back.gameObject.AddComponent<Button>();
            advance.transition = Selectable.Transition.None;
            advance.onClick.AddListener(Advance);

            var frame = UIKit.Rect(root, "Picture Frame").Stretch(0, 0, 0, 0);
            pictureGroup = frame.gameObject.AddComponent<CanvasGroup>();
            pictureGroup.blocksRaycasts = false;
            picture = UIKit.Rect(frame, "Picture").gameObject.AddComponent<Image>();
            picture.raycastTarget = false;
            fitter = picture.gameObject.AddComponent<AspectRatioFitter>();
            fitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;

            var band = UIKit.Rect(root, "Caption Band").gameObject.AddComponent<Image>();
            band.color = Palette.Alpha(Palette.Space, 0.82f);
            band.raycastTarget = false;
            band.rectTransform.anchorMin = new Vector2(0, 0);
            band.rectTransform.anchorMax = new Vector2(1, 0);
            band.rectTransform.pivot = new Vector2(0.5f, 0);
            band.rectTransform.anchoredPosition = Vector2.zero;
            band.rectTransform.sizeDelta = new Vector2(0, 200);

            caption = UIKit.Label(band.transform, "", 40, Palette.Hull, TextAnchor.MiddleCenter);
            caption.rectTransform.Stretch(160, 20, 160, 50);
            var hint = UIKit.Label(band.transform, "Click or → to continue · ← to go back", 22, Palette.Ice, TextAnchor.LowerRight);
            hint.rectTransform.Stretch(40, 0, 40, 18);
            backButton = UIKit.Button(band.transform, "Back", Back, Palette.PanelLight, Palette.Hull, 24);
            backButton.GetComponent<RectTransform>().Anchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(24, 12), new Vector2(120, 60));
            var next = UIKit.Button(band.transform, "Next", Advance, Palette.Azure, Palette.Hull, 24);
            next.GetComponent<RectTransform>().Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-24, 12), new Vector2(120, 60));
            counter = UIKit.Label(band.transform, "", 20, Palette.Muted, TextAnchor.LowerLeft);
            counter.rectTransform.Stretch(40, 0, 40, 18);

            UIKit.AiWatermark(root, new Vector2(0, 1), SettingsWindow.ButtonWidth + 16);
            SettingsWindow.AddButton(root);
            var skip = UIKit.Button(root, "Skip", Leave, Palette.Alpha(Palette.Panel, 0.85f), Palette.Hull, 24);
            skip.GetComponent<RectTransform>().Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-32, -28), new Vector2(140, 56));
        }

        void Update()
        {
            var keys = Keyboard.current;
            if (keys == null || done || Wiki.IsOpen || SettingsWindow.IsOpen) return;
            if (keys.spaceKey.wasPressedThisFrame || keys.enterKey.wasPressedThisFrame || keys.rightArrowKey.wasPressedThisFrame) Advance();
            if (keys.leftArrowKey.wasPressedThisFrame || keys.backspaceKey.wasPressedThisFrame) Back();
            if (keys.escapeKey.wasPressedThisFrame) Leave();
        }

        public void Advance()
        {
            if (busy || done) return;
            AudioDirector.Play(Sfx.LineNext);
            if (line + 1 < cutscene.slides[slide].lines.Length)
            {
                line++;
                ShowLine();
                return;
            }
            if (slide + 1 < cutscene.slides.Length)
            {
                ShowSlide(slide + 1, true);
                return;
            }
            Leave();
        }

        // Goes back one line; from a slide's first line, back to the last line of the slide before.
        public void Back()
        {
            if (busy || done || (slide == 0 && line == 0)) return;
            AudioDirector.Play(Sfx.LineBack);
            if (line > 0)
            {
                line--;
                ShowLine();
                return;
            }
            ShowSlide(slide - 1, true, cutscene.slides[slide - 1].lines.Length - 1);
        }

        void ShowSlide(int index, bool fade, int startLine = 0)
        {
            slide = index;
            line = Mathf.Max(0, startLine);
            if (fade) StartCoroutine(FadeTo(cutscene.slides[index]));
            else SetPicture(cutscene.slides[index].image);
            ShowLine();
        }

        IEnumerator FadeTo(Slide next)
        {
            busy = true;
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 4) { pictureGroup.alpha = 1 - t; yield return null; }
            SetPicture(next.image);
            for (float t = 0; t < 1; t += Time.unscaledDeltaTime * 4) { pictureGroup.alpha = t; yield return null; }
            pictureGroup.alpha = 1;
            busy = false;
        }

        void SetPicture(string id)
        {
            var sprite = LoadPicture(id);
            picture.sprite = sprite;
            picture.color = sprite ? Color.white : Palette.Panel;
            fitter.aspectRatio = sprite ? sprite.rect.width / sprite.rect.height : 16f / 9f;
        }

        static readonly Dictionary<string, Sprite> pictures = new Dictionary<string, Sprite>();

        public static Sprite LoadPicture(string id)
        {
            if (pictures.TryGetValue(id, out var cached)) return cached;
            var sprite = Resources.Load<Sprite>("Cutscenes/" + id);
            if (!sprite)
            {
                var tex = Resources.Load<Texture2D>("Cutscenes/" + id);
                if (tex) sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            }
            pictures[id] = sprite;
            return sprite;
        }

        void ShowLine()
        {
            caption.text = story.Story(cutscene.slides[slide].lines[line]);
            counter.text = (slide + 1) + " / " + cutscene.slides.Length;
            backButton.interactable = slide > 0 || line > 0;
        }

        public void Leave()
        {
            if (done) return;
            done = true;
            Flow.SceneDone(Flow.CutsceneId);
        }
    }
}
