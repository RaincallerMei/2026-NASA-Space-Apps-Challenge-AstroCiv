using System;
using Mapmakers.Game;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Every option in one window: Tips, Calm motion, Music, Sound effects, Wiki & Help and Data & Credits.
    // Opens over any screen from the Settings button (top left) or the in-game Menu. Close or Esc shuts it.
    public sealed class SettingsWindow : MonoBehaviour
    {
        public const float ButtonWidth = 180;
        const float Width = 1000;

        static SettingsWindow open;
        static int closedFrame = -1;

        Action closed, wikiOpened;
        RectTransform panel;
        int openedFrame;

        // Also true in the frame it closed, so the same Esc press does not reach the screen underneath.
        public static bool IsOpen => open != null || closedFrame == Time.frameCount;

        public static Button AddButton(Transform root)
        {
            var button = UIKit.Button(root, "Settings", () => Open(), Palette.Alpha(Palette.Panel, 0.92f), Palette.Hull, 24);
            button.GetComponent<RectTransform>().Anchor(new Vector2(0, 1), new Vector2(0, 1), new Vector2(24, -24), new Vector2(ButtonWidth, 56));
            return button;
        }

        public static void Open(Action onClose = null, Action onWiki = null)
        {
            if (open != null) return;
            AudioDirector.Play(Sfx.WindowOpen);
            var root = UIKit.Canvas("Settings Canvas", 45);
            open = root.gameObject.AddComponent<SettingsWindow>();
            open.closed = onClose;
            open.wikiOpened = onWiki;
            open.openedFrame = Time.frameCount;
            UIKit.Blocker(root, 0.8f);

            var window = UIKit.Panel(root, Palette.Panel, "Settings");
            window.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(Width, 0));
            var outline = window.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Cyan;
            outline.effectDistance = new Vector2(3, -3);
            var column = window.gameObject.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(48, 48, 36, 40);
            column.spacing = 14;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            window.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            UIFx.Pop(window.transform);
            open.panel = window.rectTransform;
            open.ShowMain();
        }

        public static void Close()
        {
            if (open == null) return;
            AudioDirector.Play(Sfx.WindowClose);
            var callback = open.closed;
            Shut();
            callback?.Invoke();
        }

        static void Shut()
        {
            Destroy(open.gameObject);
            open = null;
            closedFrame = Time.frameCount;
        }

        void Update()
        {
            if (open != this || Time.frameCount == openedFrame) return;
            var keys = Keyboard.current;
            if (keys != null && keys.escapeKey.wasPressedThisFrame) Close();
        }

        void ShowMain()
        {
            Clear();
            Title("Settings");
            Section("OPTIONS");
            Toggle("Tips", () => Progress.HintsOn, v => Progress.HintsOn = v);
            Toggle("Calm motion", () => Progress.ReducedMotion, v => Progress.ReducedMotion = v);
            Toggle("Music", () => Progress.MusicOn, v => Progress.MusicOn = v);
            Toggle("Sound effects", () => Progress.SoundsOn, v => Progress.SoundsOn = v);
            Section("HELP");
            Row("Wiki & Help", OpenWiki, Palette.PanelLight);
            Row("Data & Credits", ShowCredits, Palette.PanelLight);
            Row("Close", Close, Palette.Azure);
        }

        void ShowCredits()
        {
            Clear();
            Title("Data & Credits");
            UIKit.Label(panel, MenuScreen.CreditsText + "\n\n" + MenuScreen.AudioCreditsText, 22, Palette.Ice, TextAnchor.UpperLeft);
            Row("Back", ShowMain, Palette.Azure);
        }

        // The Wiki opens in place of this window and brings it back when it closes.
        void OpenWiki()
        {
            var onClose = closed;
            var onWiki = wikiOpened;
            Shut();
            onWiki?.Invoke();
            Wiki.Open(null, () => Open(onClose, onWiki), SceneManager.GetActiveScene().name == Flow.MenuScene);
        }

        void Clear()
        {
            for (int i = panel.childCount - 1; i >= 0; i--)
            {
                var child = panel.GetChild(i).gameObject;
                child.SetActive(false);
                Destroy(child);
            }
        }

        void Title(string text)
        {
            var head = UIKit.Rect(panel, "Head");
            head.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
            UIKit.Label(head, text, 40, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.Stretch();
        }

        void Section(string text)
        {
            var label = UIKit.Label(panel, text, 22, Palette.Cyan, TextAnchor.LowerLeft, FontStyle.Bold);
            label.gameObject.AddComponent<LayoutElement>().preferredHeight = 34;
        }

        Button Row(string text, UnityEngine.Events.UnityAction action, Color color)
        {
            var button = UIKit.Button(panel, text, action, color, Palette.Hull, 28);
            button.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
            return button;
        }

        // Shows "Name: On" or "Name: Off" and updates in place, so the window never closes for a toggle.
        void Toggle(string name, Func<bool> get, Action<bool> set)
        {
            Button button = null;
            button = Row("", () => { set(!get()); Paint(button, name, get()); }, Palette.PanelLight);
            Paint(button, name, get());
        }

        static void Paint(Button button, string name, bool on)
        {
            UIKit.LabelOf(button).text = name + ": " + (on ? "On" : "Off");
            button.image.color = on ? Palette.Azure : Palette.PanelLight;
        }
    }
}
