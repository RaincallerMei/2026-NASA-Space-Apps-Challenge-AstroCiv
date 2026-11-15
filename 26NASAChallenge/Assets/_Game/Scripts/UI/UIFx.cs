using System;
using Mapmakers.Game;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Small, gentle motion for children: button bounce, placement ripples, pop-in, shake.
    // All motion uses unscaled UI time (it keeps working while the mission is paused)
    // and is reduced to a soft fade when "Calm motion" is on.
    public static class UIFx
    {
        public static bool Calm => Progress.ReducedMotion;

        static Sprite ring;

        // A 16 x 16 hollow pixel square, two pixels thick.
        public static Sprite RingSprite
        {
            get
            {
                if (ring) return ring;
                const int size = 16;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int edge = Mathf.Min(Mathf.Min(x, y), Mathf.Min(size - 1 - x, size - 1 - y));
                        tex.SetPixel(x, y, edge < 2 ? Color.white : new Color(1, 1, 1, 0));
                    }
                tex.Apply();
                ring = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(3, 3, 3, 3));
                return ring;
            }
        }

        // White pixel rings that grow out of a footprint and fade (bottom-left anchored map coordinates).
        public static void Ripple(RectTransform parent, Vector2 position, Vector2 size)
        {
            int rings = Calm ? 1 : 3;
            for (int i = 0; i < rings; i++)
            {
                var image = UIKit.Rect(parent, "Ripple").gameObject.AddComponent<Image>();
                image.sprite = RingSprite;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 0.5f;
                image.raycastTarget = false;
                var rt = image.rectTransform;
                rt.anchorMin = rt.anchorMax = Vector2.zero;
                rt.pivot = new Vector2(0.5f, 0.5f);
                rt.anchoredPosition = position + size / 2;
                rt.sizeDelta = size;
                var fx = image.gameObject.AddComponent<RippleFx>();
                fx.delay = i * 0.12f;
                fx.grow = Calm ? 0 : 36 + i * 14;
                fx.start = size;
            }
        }

        public static void Pop(Transform target)
        {
            if (Calm || target == null) return;
            target.gameObject.AddComponent<PopFx>();
        }

        public static void Shake(Transform target)
        {
            if (Calm || target == null || target.GetComponent<ShakeFx>() != null) return;
            target.gameObject.AddComponent<ShakeFx>();
        }

        public static float EaseOutBack(float t)
        {
            const float c = 1.70158f;
            float u = t - 1;
            return 1 + (c + 1) * u * u * u + c * u * u;
        }
    }

    public sealed class RippleFx : MonoBehaviour
    {
        public float delay, grow;
        public Vector2 start;
        const float Life = 0.55f;
        float t;
        Image image;
        RectTransform rt;

        void Awake()
        {
            image = GetComponent<Image>();
            rt = (RectTransform)transform;
            image.color = new Color(1, 1, 1, 0);
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            float k = (t - delay) / Life;
            if (k < 0) return;
            if (k >= 1) { Destroy(gameObject); return; }
            rt.sizeDelta = start + Vector2.one * grow * k;
            // Step the size to whole pixels of the ring sprite for a pixel-art feel.
            rt.sizeDelta = new Vector2(Mathf.Round(rt.sizeDelta.x / 4) * 4, Mathf.Round(rt.sizeDelta.y / 4) * 4);
            image.color = new Color(1, 1, 1, 0.9f * (1 - k));
        }
    }

    public sealed class PopFx : MonoBehaviour
    {
        float t;
        void Update()
        {
            t += Time.unscaledDeltaTime / 0.3f;
            transform.localScale = Vector3.one * Mathf.LerpUnclamped(0.6f, 1, UIFx.EaseOutBack(Mathf.Clamp01(t)));
            if (t < 1) return;
            transform.localScale = Vector3.one;
            Destroy(this);
        }
    }

    public sealed class ShakeFx : MonoBehaviour
    {
        float t;
        Vector2 origin;
        RectTransform rt;

        void Awake()
        {
            rt = (RectTransform)transform;
            origin = rt.anchoredPosition;
        }

        void Update()
        {
            t += Time.unscaledDeltaTime;
            if (t >= 0.25f)
            {
                rt.anchoredPosition = origin;
                Destroy(this);
                return;
            }
            rt.anchoredPosition = origin + new Vector2(Mathf.Sin(t * 60) * 6 * (1 - t / 0.25f), 0);
        }
    }

    // Buttons grow a little on hover and squash on press. Added to every UIKit button.
    public sealed class ButtonJuice : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
    {
        Selectable selectable;
        float target = 1, current = 1;

        void Awake() => selectable = GetComponent<Selectable>();

        bool Live => selectable == null || selectable.interactable;

        public void OnPointerEnter(PointerEventData e) { if (Live) target = 1.05f; }
        public void OnPointerExit(PointerEventData e) => target = 1;
        public void OnPointerDown(PointerEventData e) { if (Live) target = 0.93f; }
        public void OnPointerUp(PointerEventData e) => target = e.hovered.Contains(gameObject) && Live ? 1.05f : 1;

        void OnDisable()
        {
            target = current = 1;
            transform.localScale = Vector3.one;
        }

        void Update()
        {
            float goal = UIFx.Calm ? 1 : target;
            if (Math.Abs(current - goal) < 0.001f) return;
            current = Mathf.MoveTowards(current, goal, Time.unscaledDeltaTime * 1.2f);
            transform.localScale = Vector3.one * current;
        }
    }

    // Slow, soft blink (well under three flashes per second) used for emergency lights and highlights.
    public sealed class SoftPulse : MonoBehaviour
    {
        public float speed = 1.2f, low = 0.35f, high = 1f;
        Graphic graphic;
        Color baseColor;

        void Awake()
        {
            graphic = GetComponent<Graphic>();
            baseColor = graphic.color;
        }

        void Update()
        {
            float a = UIFx.Calm ? high : Mathf.Lerp(low, high, 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * speed * Mathf.PI));
            graphic.color = new Color(baseColor.r, baseColor.g, baseColor.b, baseColor.a * a);
        }
    }

    // Circles a top-bar resource that is running low: a pulsing ring around it and a bouncing arrow with a short
    // label under it, so the player notices it while looking at the map. Pulses at most 1.5 times a second.
    public sealed class LowMark : MonoBehaviour
    {
        RectTransform target, self, ring, arrow, label;
        Image ringImage, arrowImage, labelImage;
        Text text;
        bool danger;

        public int Severity { get; private set; }
        public string Message => text.text;

        public static LowMark Create(RectTransform layer, RectTransform target)
        {
            var self = UIKit.Rect(layer, "Low " + target.name);
            self.anchorMin = self.anchorMax = self.pivot = new Vector2(0.5f, 0.5f);
            var mark = self.gameObject.AddComponent<LowMark>();
            mark.target = target;
            mark.self = self;

            mark.ringImage = Part(self, "Ring", UIFx.RingSprite);
            mark.ringImage.type = Image.Type.Sliced;
            mark.ringImage.pixelsPerUnitMultiplier = 0.25f;
            mark.ring = mark.ringImage.rectTransform;

            mark.arrowImage = Part(self, "Arrow", UIKit.ArrowSprite);
            mark.arrow = mark.arrowImage.rectTransform;
            mark.arrow.pivot = new Vector2(0.5f, 0);
            mark.arrow.sizeDelta = new Vector2(40, 40);
            mark.arrow.localEulerAngles = new Vector3(0, 0, 180);
            var outline = mark.arrowImage.gameObject.AddComponent<Outline>();
            outline.effectColor = Palette.Ink;
            outline.effectDistance = new Vector2(2, -2);

            mark.labelImage = UIKit.Panel(self, Palette.Amber, "Label");
            mark.labelImage.raycastTarget = false;
            mark.label = mark.labelImage.rectTransform;
            mark.label.anchorMin = mark.label.anchorMax = new Vector2(0.5f, 0.5f);
            mark.label.pivot = new Vector2(0.5f, 1);
            mark.label.sizeDelta = new Vector2(170, 36);
            mark.text = UIKit.Label(mark.label, "", 22, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            mark.text.rectTransform.Stretch();
            mark.text.raycastTarget = false;
            self.gameObject.SetActive(false);
            return mark;
        }

        static Image Part(RectTransform parent, string name, Sprite sprite)
        {
            var image = UIKit.Rect(parent, name).gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0.5f);
            return image;
        }

        // 0 hides the mark, 1 is a yellow warning, 2 is red danger.
        public void Set(int severity, string message)
        {
            bool appeared = severity > 0 && Severity == 0;
            Severity = severity;
            gameObject.SetActive(severity > 0);
            if (severity == 0) return;
            danger = severity > 1;
            var color = danger ? Palette.Danger : Palette.Amber;
            ringImage.color = arrowImage.color = labelImage.color = color;
            text.color = danger ? Palette.Hull : Palette.Ink;
            text.text = message;
            if (appeared) UIFx.Pop(label);
        }

        void LateUpdate()
        {
            var bounds = target.rect;
            self.position = target.TransformPoint(bounds.center);
            self.sizeDelta = bounds.size;
            float wave = UIFx.Calm ? 1 : 0.5f + 0.5f * Mathf.Sin(Time.unscaledTime * Mathf.PI * 2 * (danger ? 1.5f : 1f));
            ring.anchoredPosition = Vector2.zero;
            ring.sizeDelta = bounds.size + Vector2.one * (12 + 12 * wave);
            ringImage.color = Palette.Alpha(ringImage.color, 0.55f + 0.45f * wave);
            float bob = UIFx.Calm ? 0 : Mathf.Abs(Mathf.Sin(Time.unscaledTime * 4)) * 10;
            float bottom = -bounds.height / 2 - 14;
            arrow.anchoredPosition = new Vector2(0, bottom - bob);
            label.anchoredPosition = new Vector2(0, bottom - 44 - bob);
        }
    }

    // Gently pulses a layer (placement suggestions) so it reads as a hint, not a wall.
    public sealed class GlowPulse : MonoBehaviour
    {
        CanvasGroup group;
        void Awake() => group = GetComponent<CanvasGroup>();
        void Update()
        {
            if (group == null) return;
            group.alpha = UIFx.Calm ? 0.85f : 0.55f + 0.45f * Mathf.PingPong(Time.unscaledTime * 1.2f, 1);
        }
    }
}
