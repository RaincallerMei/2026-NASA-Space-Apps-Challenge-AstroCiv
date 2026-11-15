using System;
using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    public struct TipContent
    {
        public string title, icon, body;
        public TipContent(string title, string icon, string body) { this.title = title; this.icon = icon; this.body = body; }
    }

    // One shared explanation box per scene. Hover shows it; a tap pins it for a few seconds (touch screens).
    public sealed class TooltipView : MonoBehaviour
    {
        const float Width = 480;
        static TooltipView instance;
        RectTransform canvas, panel;
        Image icon;
        Text title, body;
        float hideAt = -1;
        object owner;

        static TooltipView Get()
        {
            if (instance) return instance;
            var c = UIKit.Canvas("Tooltip Canvas", 60);
            instance = c.gameObject.AddComponent<TooltipView>();
            instance.Build(c);
            return instance;
        }

        void Build(RectTransform c)
        {
            canvas = c;
            var bg = UIKit.Panel(c, Palette.Alpha(Palette.Ink, 0.98f), "Tooltip");
            bg.raycastTarget = false;
            panel = bg.rectTransform;
            panel.anchorMin = panel.anchorMax = Vector2.zero;
            panel.pivot = new Vector2(0, 1);
            panel.sizeDelta = new Vector2(Width, 100);
            var outline = bg.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Cyan;
            outline.effectDistance = new Vector2(2, -2);
            var column = bg.gameObject.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(18, 18, 14, 16);
            column.spacing = 8;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            bg.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;

            var head = UIKit.Rect(panel, "Head");
            head.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;
            icon = UIKit.Icon(head, "info", 40);
            icon.rectTransform.Anchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(40, 40));
            title = UIKit.Label(head, "", 26, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            title.rectTransform.Stretch(52, 0, 0, 0);
            body = UIKit.Label(panel, "", 22, Palette.Ice, TextAnchor.UpperLeft);
            panel.gameObject.SetActive(false);
        }

        public static void Show(object owner, TipContent content, Vector2 screenPos, float seconds = -1)
        {
            if (string.IsNullOrEmpty(content.title) && string.IsNullOrEmpty(content.body)) return;
            var view = Get();
            view.owner = owner;
            view.title.text = content.title;
            view.icon.sprite = Icons.Get(string.IsNullOrEmpty(content.icon) ? "info" : content.icon);
            view.body.text = Keywords.Mark(content.body);
            view.hideAt = seconds > 0 ? Time.unscaledTime + seconds : -1;
            view.panel.gameObject.SetActive(true);
            view.panel.SetAsLastSibling();
            LayoutRebuilder.ForceRebuildLayoutImmediate(view.panel);
            view.Place(screenPos);
        }

        void Place(Vector2 screenPos)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas, screenPos, null, out var local);
            var size = canvas.rect.size;
            var p = local + size / 2 + new Vector2(24, -24);
            float h = panel.rect.height;
            if (p.x + Width > size.x - 8) p.x = Mathf.Max(8, local.x + size.x / 2 - Width - 24);
            if (p.y - h < 8) p.y = Mathf.Min(size.y - 8, local.y + size.y / 2 + h + 24);
            panel.anchoredPosition = p;
        }

        public static void Hide(object owner)
        {
            if (instance && instance.owner == owner) instance.panel.gameObject.SetActive(false);
        }

        public static bool Visible => instance && instance.panel.gameObject.activeSelf;

        void Update()
        {
            if (hideAt > 0 && Time.unscaledTime > hideAt) { hideAt = -1; panel.gameObject.SetActive(false); }
        }
    }

    // Add to any UI element: hover explains it, a tap pins the explanation (no hover on touch screens).
    public sealed class Tip : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
    {
        public Func<TipContent> content;
        public bool pinOnClick = true;

        public void OnPointerEnter(PointerEventData e) { if (content != null) TooltipView.Show(this, content(), e.position); }
        public void OnPointerExit(PointerEventData e) => TooltipView.Hide(this);
        public void OnPointerClick(PointerEventData e) { if (pinOnClick && content != null) TooltipView.Show(this, content(), e.position, 5); }
        void OnDisable() => TooltipView.Hide(this);

        public static Tip Add(Component target, Func<TipContent> content, bool pinOnClick = true)
        {
            var tip = target.gameObject.AddComponent<Tip>();
            tip.content = content;
            tip.pinOnClick = pinOnClick;
            var graphic = target.GetComponent<Graphic>();
            if (graphic != null) graphic.raycastTarget = true;
            return tip;
        }
    }

    // Short explanations built from Help.json plus live numbers from Balance.json, so text and rules never drift.
    public static class HelpText
    {
        public static List<string> Facts(BuildingDef def, bool soilRules)
        {
            var facts = new List<string>();
            facts.Add("Cost: " + def.cost + " supplies" + (soilRules && def.soilCost > 0 ? " + " + def.soilCost + " soil" : "") + ".");
            if (def.generationKW > 0) facts.Add("Makes up to " + SimText.Num(def.generationKW) + " kW of power in full sun.");
            if (def.demandKW > 0) facts.Add("Uses " + SimText.Num(def.demandKW) + " kW of power" + (def.optional ? ". You can turn it off." : ". Always on: the crew needs it."));
            if (def.batteryKWh > 0) facts.Add("Stores " + SimText.Num(def.batteryKWh) + " kWh of power.");
            facts.Add("Size: " + def.width + " × " + def.height + " tiles.");
            return facts;
        }

        public static TipContent Building(Balance balance, BuildingType type, bool soilRules = true)
        {
            var def = balance.Def(type);
            var page = HelpData.Load().BuildingPage(type.ToString());
            var lines = new List<string>();
            lines.Add(page != null ? page.summary : def.blurb);
            lines.AddRange(Facts(def, soilRules));
            if (page != null && !string.IsNullOrEmpty(page.best)) lines.Add("Best spot: " + page.best);
            return new TipContent(def.displayName, type.ToString(), string.Join("\n", lines));
        }

        public static TipContent Resource(string id, string live)
        {
            var page = HelpData.Load().ResourcePage(id);
            if (page == null) return new TipContent(id, id, live);
            var lines = new List<string> { live, page.summary };
            if (page.rows.Length > 0) lines.Add(page.rows[0].text);
            return new TipContent(page.title, page.icon, string.Join("\n", lines.FindAll(l => !string.IsNullOrEmpty(l))));
        }
    }

    // The in-game reference. Opens over any screen; the game screen pauses its clock while it is open.
    public static class Wiki
    {
        static GameObject open;
        static Action closed;
        static RectTransform content;
        static bool replay;
        static readonly List<(string id, Image image)> entries = new List<(string, Image)>();

        public static bool IsOpen => open != null;
        public static string Page { get; private set; } = "";

        public static void Open(string pageId = null, Action onClose = null, bool allowReplay = false)
        {
            if (IsOpen) Close();
            closed = onClose;
            replay = allowReplay;
            var help = HelpData.Load();
            var root = UIKit.Canvas("Wiki Canvas", 40);
            open = root.gameObject;
            UIKit.Blocker(root, 0.8f);

            var window = UIKit.Panel(root, Palette.Panel, "Wiki");
            window.rectTransform.Anchor(new Vector2(0.5f, 0.5f), new Vector2(0.5f, 0.5f), Vector2.zero, new Vector2(1600, 920));
            var outline = window.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Cyan;
            outline.effectDistance = new Vector2(3, -3);
            UIFx.Pop(window.transform);

            UIKit.Icon(window.transform, "info", 52).TopLeftOf(32, 26);
            UIKit.Label(window.transform, "Wiki & Help", 42, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.TopLeft(100, 22, 600, 60);
            UIKit.Label(window.transform, "Pick a topic on the left. Come back any time.", 22, Palette.Muted, TextAnchor.MiddleLeft)
                .rectTransform.TopLeft(640, 34, 640, 40);
            var close = UIKit.Button(window.transform, "Close", Close, Palette.Azure, Palette.Hull, 26);
            close.GetComponent<RectTransform>().TopLeft(1600 - 200, 22, 168, 60);

            var list = Scroll(window.rectTransform, new Vector2(32, 104), new Vector2(460, 784));
            entries.Clear();
            foreach (var section in help.sections)
            {
                var head = UIKit.Label(list, section.ToUpperInvariant(), 22, Palette.Cyan, TextAnchor.LowerLeft, FontStyle.Bold);
                head.gameObject.AddComponent<LayoutElement>().preferredHeight = 44;
                foreach (var page in help.wiki)
                {
                    if (page.section != section) continue;
                    string id = page.id;
                    var button = UIKit.Button(list, "", () => Show(id), Palette.PanelLight);
                    button.gameObject.AddComponent<LayoutElement>().preferredHeight = 56;
                    var icon = UIKit.Icon(button.transform, page.icon, 36);
                    icon.rectTransform.Anchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f), new Vector2(12, 0), new Vector2(36, 36));
                    UIKit.Label(button.transform, page.title, 24, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.Stretch(60, 0, 8, 0);
                    entries.Add((id, button.image));
                }
            }
            content = Scroll(window.rectTransform, new Vector2(520, 104), new Vector2(1048, 784));
            Show(string.IsNullOrEmpty(pageId) || help.Page(pageId) == null ? help.wiki[0].id : pageId);
        }

        public static void Close()
        {
            if (!IsOpen) return;
            UnityEngine.Object.Destroy(open);
            open = null;
            content = null;
            var callback = closed;
            closed = null;
            callback?.Invoke();
        }

        public static void Show(string id)
        {
            var page = HelpData.Load().Page(id);
            if (page == null || content == null) return;
            Page = id;
            foreach (var (entry, image) in entries) image.color = entry == id ? Palette.Azure : Palette.PanelLight;
            UIKit.Clear(content);

            var head = UIKit.Rect(content, "Title");
            head.gameObject.AddComponent<LayoutElement>().preferredHeight = 84;
            UIKit.Icon(head, page.icon, 72).rectTransform.Anchor(new Vector2(0, 0.5f), new Vector2(0, 0.5f), Vector2.zero, new Vector2(72, 72));
            UIKit.Label(head, page.title, 40, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold).rectTransform.Stretch(92, 0, 0, 0);
            Paragraph(page.summary, 28, Palette.Hull);

            if (!string.IsNullOrEmpty(page.building) && Enum.TryParse(page.building, out BuildingType type))
                foreach (var fact in HelpText.Facts(BalanceLoader.Load().Def(type), true)) Row(fact.StartsWith("Cost") ? "supplies" : fact.StartsWith("Size") ? "build" : "power", fact);
            foreach (var row in page.rows) Row(row.icon, row.text);
            if (!string.IsNullOrEmpty(page.best)) Row("check", "Best spot: " + page.best);
            if (replay && !string.IsNullOrEmpty(page.lesson))
            {
                string lesson = page.lesson;
                var again = UIKit.Button(content, "Replay this lesson", () => { Close(); Flow.ReplayLesson(lesson); }, Palette.Azure, Palette.Hull, 26);
                again.gameObject.AddComponent<LayoutElement>().preferredHeight = 64;
            }
            ((RectTransform)content).anchoredPosition = Vector2.zero;
        }

        static void Paragraph(string text, int size, Color color)
        {
            if (string.IsNullOrEmpty(text)) return;
            UIKit.Label(content, Keywords.Mark(text), size, color, TextAnchor.UpperLeft);
        }

        static void Row(string icon, string text)
        {
            var row = UIKit.Rect(content, "Row");
            var layout = row.gameObject.AddComponent<HorizontalLayoutGroup>();
            layout.spacing = 18;
            layout.childControlWidth = layout.childControlHeight = true;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = false;
            var image = UIKit.Icon(row, icon, 44);
            var size = image.gameObject.AddComponent<LayoutElement>();
            size.preferredWidth = size.preferredHeight = 44;
            size.flexibleWidth = 0;
            var label = UIKit.Label(row, Keywords.Mark(text), 26, Palette.Ice, TextAnchor.UpperLeft);
            label.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        }

        static RectTransform Scroll(RectTransform parent, Vector2 topLeft, Vector2 size)
        {
            var view = UIKit.Panel(parent, Palette.Alpha(Palette.Space, 0.6f), "Scroll");
            view.rectTransform.TopLeft(topLeft.x, topLeft.y, size.x, size.y);
            view.gameObject.AddComponent<RectMask2D>();
            var scroll = view.gameObject.AddComponent<ScrollRect>();
            scroll.horizontal = false;
            scroll.movementType = ScrollRect.MovementType.Clamped;
            scroll.scrollSensitivity = 40;
            var inner = UIKit.Rect(view.transform, "Content");
            inner.anchorMin = new Vector2(0, 1);
            inner.anchorMax = new Vector2(1, 1);
            inner.pivot = new Vector2(0.5f, 1);
            inner.sizeDelta = Vector2.zero;
            var column = inner.gameObject.AddComponent<VerticalLayoutGroup>();
            column.padding = new RectOffset(20, 20, 16, 24);
            column.spacing = 14;
            column.childControlWidth = column.childControlHeight = true;
            column.childForceExpandWidth = true;
            column.childForceExpandHeight = false;
            inner.gameObject.AddComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            scroll.viewport = view.rectTransform;
            scroll.content = inner;
            return inner;
        }
    }

    // A bouncing yellow arrow with a short label, pointing at the next thing a new player should press.
    public sealed class CoachMark : MonoBehaviour
    {
        RectTransform root, arrow, label;
        Text text;
        Vector2 point;

        public static CoachMark Create(RectTransform root)
        {
            var holder = UIKit.Rect(root, "Coach");
            holder.Stretch();
            var coach = holder.gameObject.AddComponent<CoachMark>();
            coach.root = root;
            var arrow = UIKit.Rect(holder, "Arrow").gameObject.AddComponent<Image>();
            arrow.sprite = UIKit.ArrowSprite;
            arrow.color = Palette.Amber;
            arrow.raycastTarget = false;
            coach.arrow = arrow.rectTransform;
            coach.arrow.anchorMin = coach.arrow.anchorMax = new Vector2(0.5f, 0.5f);
            coach.arrow.pivot = new Vector2(0.5f, 0);
            coach.arrow.sizeDelta = new Vector2(48, 48);
            var outline = arrow.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(2, -2);
            var box = UIKit.Panel(holder, Palette.Amber, "Label");
            box.raycastTarget = false;
            coach.label = box.rectTransform;
            coach.label.anchorMin = coach.label.anchorMax = new Vector2(0.5f, 0.5f);
            coach.label.pivot = new Vector2(0.5f, 0);
            var fit = box.gameObject.AddComponent<HorizontalLayoutGroup>();
            fit.padding = new RectOffset(18, 18, 8, 8);
            fit.childControlWidth = fit.childControlHeight = true;
            box.gameObject.AddComponent<ContentSizeFitter>().horizontalFit = ContentSizeFitter.FitMode.PreferredSize;
            box.gameObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            coach.text = UIKit.Label(box.transform, "", 26, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            coach.text.horizontalOverflow = HorizontalWrapMode.Overflow;
            holder.gameObject.SetActive(false);
            return coach;
        }

        public bool Showing => gameObject.activeSelf;
        public string Message => text.text;

        // Points at the top centre of a UI element.
        public void Point(RectTransform target, string message)
        {
            var corners = new Vector3[4];
            target.GetWorldCorners(corners);
            var top = (corners[1] + corners[2]) / 2;
            point = (Vector2)root.InverseTransformPoint(top) + new Vector2(0, 6);
            text.text = Keywords.Mark(message, true);
            gameObject.SetActive(true);
            transform.SetAsLastSibling();
        }

        public void Hide() => gameObject.SetActive(false);

        void Update()
        {
            float bob = UIFx.Calm ? 0 : Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 12;
            arrow.anchoredPosition = point + new Vector2(0, bob);
            label.anchoredPosition = point + new Vector2(0, bob + 50);
        }
    }
}
