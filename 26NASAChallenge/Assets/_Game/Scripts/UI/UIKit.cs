using System.Collections.Generic;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Palette from Agent Instructions/AstroCiv_Art_Creation_Guide.md.
    public static class Palette
    {
        public static readonly Color Ink = Hex("203653");
        public static readonly Color Space = Hex("10182D");
        public static readonly Color Hull = Hex("F4F8F5");
        public static readonly Color Ice = Hex("CDEAF4");
        public static readonly Color Azure = Hex("3889D7");
        public static readonly Color Cyan = Hex("60D5E5");
        public static readonly Color Mint = Hex("8DD6A1");
        public static readonly Color Peach = Hex("F3BD91");
        public static readonly Color Amber = Hex("F3B84D");
        public static readonly Color Danger = Hex("D95C62");
        public static readonly Color LunarGray = Hex("B9C3CF");
        public static readonly Color LunarShadow = Hex("707F9C");
        public static readonly Color MarsSand = Hex("D99A73");
        public static readonly Color MarsRust = Hex("AA6257");
        public static readonly Color Leaf = Hex("4FA35F");
        public static readonly Color Muted = Hex("8A9BB5");
        public static readonly Color Panel = Hex("1B2A47");
        public static readonly Color PanelLight = Hex("26395E");

        public static Color Hex(string hex)
        {
            ColorUtility.TryParseHtmlString("#" + hex, out var c);
            return c;
        }

        public static Color Alpha(Color c, float a) => new Color(c.r, c.g, c.b, a);
    }

    // Small pixel icons drawn from text, used until the real art arrives. 12 x 12, '.' is clear.
    public static class Icons
    {
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        static readonly Dictionary<char, Color> colors = new Dictionary<char, Color>
        {
            { 'k', Palette.Ink }, { 'w', Palette.Hull }, { 'c', Palette.Cyan }, { 'b', Palette.Azure },
            { 'y', Palette.Amber }, { 'g', Palette.Mint }, { 'G', Palette.Leaf }, { 'r', Palette.Danger },
            { 'p', Palette.Peach }, { 'd', Palette.Space }, { 's', Palette.LunarGray }, { 'm', Palette.Muted }
        };

        static readonly Dictionary<string, string[]> art = new Dictionary<string, string[]>
        {
            { "power", new[] { "......kk....", ".....kyk....", "....kyyk....", "...kyyk.....", "..kyyyyykk..", "..kkkyyyyk..", "....kyyyk...", "....kyyk....", "...kyyk.....", "...kyk......", "...kk......." } },
            { "battery", new[] { "....kkkk....", "...kkkkkk...", "...kwwwwk...", "...kwwwwk...", "...kcccck...", "...kcccck...", "...kcccck...", "...kcccck...", "...kcccck...", "...kcccck...", "...kkkkkk..." } },
            { "air", new[] { "............", "...kkk......", "..kcwck.....", "..kccck.....", "...kkk..kk..", ".......kcwk.", "..kk...kcck.", ".kcwk...kk..", ".kcck.......", "..kk...kkk..", "......kcwck.", ".......kkk.." } },
            { "water", new[] { ".....kk.....", ".....kk.....", "....kbbk....", "....kbbk....", "...kbbbbk...", "..kbbbbbbk..", "..kbwbbbbk..", "..kbwbbbbk..", "..kbbbbbbk..", "...kbbbbk...", "....kkkk...." } },
            { "food", new[] { "............", ".......kkk..", "......kgggk.", ".....kgggk..", "..kkk.kgk...", ".kgggk.k....", ".kgggkk.....", "..kkkk......", "....kk......", "..kkkkkk....", "..kppppk....", "...kkkk....." } },
            { "crew", new[] { "............", "..kk...kk...", ".krrk.krrk..", "krwrrkrrrrk.", "krrrrrrrrrk.", "krrrrrrrrrk.", ".krrrrrrrk..", "..krrrrrk...", "...krrrk....", "....krk.....", ".....k......" } },
            { "supplies", new[] { "............", ".kkkkkkkkkk.", ".kyyyyyyyyk.", ".kkkkkkkkkk.", ".kyykyykyyk.", ".kyykyykyyk.", ".kkkkkkkkkk.", ".kyykyykyyk.", ".kyykyykyyk.", ".kkkkkkkkkk." } },
            { "Habitat", new[] { "............", "....kkkk....", "..kkwwwwkk..", ".kwwwwwwwck.", ".kwwcwwcwck.", "kwwwwwwwwwck", "kcccccccccck", "kwwwwkkwwwwk", "kwwwwkdkwwwk", "kkkkkkkkkkkk" } },
            { "SolarArray", new[] { "kkkkkkkkkkkk", "kbcbbkbcbbbk", "kbbbbkbbbbbk", "kkkkkkkkkkkk", "kbcbbkbcbbbk", "kbbbbkbbbbbk", "kkkkkkkkkkkk", ".....kk.....", ".....kk.....", "....kkkk...." } },
            { "Battery", new[] { "....kkkk....", "...kkkkkk...", "...kwwwwk...", "...kcccck...", "...kcccck...", "...kcccck...", "...kcccck...", "...kcccck...", "...kkkkkk..." } },
            { "LifeSupport", new[] { ".kkk....kkk.", "kgggk..kgggk", "kgwgk..kgwgk", "kgggkkkkgggk", "kgggkcckgggk", "kgggkkkkgggk", "kgggk..kgggk", "kgggk..kgggk", ".kkk....kkk." } },
            { "Communications", new[] { ".........k..", "..kkkk..k...", ".kwwwwk.k...", "kwwwwwwkk...", "kwwwwwk.....", ".kwwwk......", "..kkk.......", "...kk.......", "..kkkk......", ".kkkkkk....." } },
            { "Greenhouse", new[] { "..kkkkkkkk..", ".kcccccccck.", "kcgcgcgcgcck", "kcgGgGgGgGck", "kkkkkkkkkkkk", "kGgGgGgGgGgk", "kkkkkkkkkkkk" } },
            { "Shelter", new[] { "............", "....kkkk....", "..kkppppkk..", ".kppppppppk.", "kppppkkppppk", "kppppkdkpppk", "kppppkdkpppk", "kkkkkkkkkkkk" } },
            { "soil", new[] { "............", "............", "....kkkk....", "..kkppppkk..", ".kpppkppppk.", "kpppppppkppk", "kppkpppppppk", "kpppppkppppk", "kkkkkkkkkkkk" } },
            { "RoverGarage", new[] { "............", "...kkkkkk...", "..kwwwwwwk..", ".kwwwwwwwwk.", "kkkkkkkkkkkk", "kwkyyyyyykwk", "kwkyyyyyykwk", "kwkyyyyyykwk", "kkkkkkkkkkkk" } },
            { "rover", new[] { "............", ".....k......", ".....kk.....", "..kkkkkkkk..", ".kwwwwwwwwk.", ".kwcwwwwcwk.", ".kkkkkkkkkk.", "kddkkddkkddk", "kddk.kdk.kdk", ".kk...k...k." } },
            { "pause", new[] { "............", "..kkk..kkk..", "..kwk..kwk..", "..kwk..kwk..", "..kwk..kwk..", "..kwk..kwk..", "..kwk..kwk..", "..kwk..kwk..", "..kkk..kkk.." } },
            { "play", new[] { "..kk........", "..kwk.......", "..kwwk......", "..kwwwk.....", "..kwwwwk....", "..kwwwk.....", "..kwwk......", "..kwk.......", "..kk........" } },
            { "clock", new[] { "...kkkkkk...", "..kwwwwwwk..", ".kwwwkwwwwk.", ".kwwwkwwwwk.", ".kwwwkkkwwk.", ".kwwwwwwwwk.", "..kwwwwwwk..", "...kkkkkk..." } },
            { "lock", new[] { "....kkkk....", "...k....k...", "...k....k...", "..kkkkkkkk..", "..kyyyyyyk..", "..kyyykyyk..", "..kyyykyyk..", "..kyyyyyyk..", "..kkkkkkkk.." } },
            { "star", new[] { ".....kk.....", ".....yk.....", "....kyyk....", "kkkkkyykkkkk", ".kyyyyyyyyk.", "..kyyyyyyk..", "...kyyyyk...", "..kyykkyyk..", "..kyk..kyk..", ".kk......kk." } },
            { "star_empty", new[] { ".....kk.....", ".....mk.....", "....kmmk....", "kkkkkmmkkkkk", ".kmmmmmmmmk.", "..kmmmmmmk..", "...kmmmmk...", "..kmmkkmmk..", "..kmk..kmk..", ".kk......kk." } },
            { "alert", new[] { ".....kk.....", "....kyyk....", "....kyyk....", "...kykkyk...", "...kykkyk...", "..kyykkyyk..", "..kyykkyyk..", ".kyyyyyyyyk.", ".kyyykkyyyk.", "kyyyyyyyyyyk", "kkkkkkkkkkkk" } }
        };

        public static Sprite Get(string id)
        {
            if (cache.TryGetValue(id, out var sprite) && sprite) return sprite;
            sprite = PixelArt.Icon(id);
            if (sprite != null) return cache[id] = sprite;
            if (!art.TryGetValue(id, out var rows)) rows = art["alert"];
            const int size = 12;
            var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
            var clear = new Color(0, 0, 0, 0);
            int top = (size - rows.Length) / 2;
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    int row = y - top;
                    char ch = row >= 0 && row < rows.Length && x < rows[row].Length ? rows[row][x] : '.';
                    tex.SetPixel(x, size - 1 - y, colors.TryGetValue(ch, out var c) ? c : clear);
                }
            }
            tex.Apply();
            sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
            cache[id] = sprite;
            return sprite;
        }
    }

    // Sprites from the AstroCiv pixel pack (Art/Resources/AstroCiv). Missing files return null.
    public static class PixelArt
    {
        const string Root = "AstroCiv/";
        static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

        static readonly Dictionary<BuildingType, string> files = new Dictionary<BuildingType, string>
        {
            { BuildingType.Habitat, "habitat" }, { BuildingType.SolarArray, "solar_array" }, { BuildingType.Battery, "battery_bank" },
            { BuildingType.LifeSupport, "life_support" }, { BuildingType.Communications, "communications_dish" },
            { BuildingType.Greenhouse, "greenhouse" }, { BuildingType.Shelter, "regolith_shelter" }, { BuildingType.RoverGarage, "rover_garage" }
        };

        static readonly Dictionary<string, string> icons = new Dictionary<string, string>
        {
            { "power", "Icons/power" }, { "water", "Icons/water" }, { "food", "Icons/food" }, { "air", "Icons/oxygen" },
            { "alert", "Icons/warning" }, { "info", "Icons/info" }, { "rover", "Vehicles/rover" }, { "battery", "Thumbnails/battery_bank" }
        };

        public static Sprite Load(string path)
        {
            if (cache.TryGetValue(path, out var sprite) && sprite) return sprite;
            return cache[path] = Resources.Load<Sprite>(Root + path);
        }

        public static Sprite Icon(string id)
        {
            if (icons.TryGetValue(id, out var path)) return Load(path);
            return System.Enum.TryParse(id, out BuildingType type) && files.ContainsKey(type) ? Load("Thumbnails/" + files[type]) : null;
        }

        public static Sprite Building(BuildingType type) => Load("Buildings/" + files[type]);
        public static Sprite Shadow(BuildingType type) => Load("Shadows/" + files[type]);
        public static Sprite Dust(BuildingType type) => Load("Dust/" + files[type]);
        public static Sprite MarsSurface(BuildingType type) => type == BuildingType.Shelter ? Load("Dust/regolith_shelter_surface") : null;
        public static Sprite Prop(string planet, string name) => Load("Environment/" + (planet == "Mars" ? "Mars/" : "Moon/") + name);
        public static Sprite Frame(string name) => Load("Characters/" + name);
        public static Texture2D Ground(string planet) => Resources.Load<Texture2D>(Root + "Environment/" + (planet == "Mars" ? "Mars" : "Moon") + "/ground_repeat");
    }

    // Builds uGUI in code with one consistent look: notched pixel panels, readable text, one font.
    public static class UIKit
    {
        static Font font;
        static Sprite panelSprite, circleSprite;

        public static Font Font => font ? font : font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

        public static Sprite PanelSprite
        {
            get
            {
                if (panelSprite) return panelSprite;
                const int size = 16;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Point, wrapMode = TextureWrapMode.Clamp };
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        int dx = Mathf.Min(x, size - 1 - x), dy = Mathf.Min(y, size - 1 - y);
                        tex.SetPixel(x, y, dx + dy < 2 ? new Color(1, 1, 1, 0) : Color.white);
                    }
                tex.Apply();
                panelSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100, 0, SpriteMeshType.FullRect, new Vector4(4, 4, 4, 4));
                return panelSprite;
            }
        }

        public static Sprite CircleSprite
        {
            get
            {
                if (circleSprite) return circleSprite;
                const int size = 64;
                var tex = new Texture2D(size, size, TextureFormat.RGBA32, false) { filterMode = FilterMode.Bilinear, wrapMode = TextureWrapMode.Clamp };
                float r = size / 2f;
                for (int y = 0; y < size; y++)
                    for (int x = 0; x < size; x++)
                    {
                        float d = Mathf.Sqrt((x + 0.5f - r) * (x + 0.5f - r) + (y + 0.5f - r) * (y + 0.5f - r));
                        tex.SetPixel(x, y, new Color(1, 1, 1, Mathf.Clamp01(r - d)));
                    }
                tex.Apply();
                circleSprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
                return circleSprite;
            }
        }

        public static RectTransform Canvas(string name, int order = 0)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = order;
            canvas.pixelPerfect = true;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
            EnsureEventSystem();
            return go.GetComponent<RectTransform>();
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindAnyObjectByType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
            go.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        }

        public static RectTransform Rect(Transform parent, string name)
        {
            var go = new GameObject(name, typeof(RectTransform));
            var rt = go.GetComponent<RectTransform>();
            rt.SetParent(parent, false);
            return rt;
        }

        // Positions a rect from the top-left corner of its parent, in reference pixels.
        public static RectTransform TopLeft(this RectTransform rt, float x, float y, float w, float h)
        {
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0, 1);
            rt.anchoredPosition = new Vector2(x, -y);
            rt.sizeDelta = new Vector2(w, h);
            return rt;
        }

        public static RectTransform Anchor(this RectTransform rt, Vector2 anchor, Vector2 pivot, Vector2 pos, Vector2 size)
        {
            rt.anchorMin = rt.anchorMax = anchor;
            rt.pivot = pivot;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return rt;
        }

        public static RectTransform Stretch(this RectTransform rt, float left = 0, float top = 0, float right = 0, float bottom = 0)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.offsetMin = new Vector2(left, bottom);
            rt.offsetMax = new Vector2(-right, -top);
            return rt;
        }

        public static Image Panel(Transform parent, Color color, string name = "Panel", bool notched = true)
        {
            var image = Rect(parent, name).gameObject.AddComponent<Image>();
            image.color = color;
            if (notched)
            {
                image.sprite = PanelSprite;
                image.type = Image.Type.Sliced;
                image.pixelsPerUnitMultiplier = 0.5f;
            }
            return image;
        }

        public static Image Icon(Transform parent, string id, float size)
        {
            var image = Rect(parent, "Icon " + id).gameObject.AddComponent<Image>();
            image.sprite = Icons.Get(id);
            image.preserveAspect = true;
            image.raycastTarget = false;
            image.rectTransform.sizeDelta = new Vector2(size, size);
            return image;
        }

        public static Text Label(Transform parent, string text, int size, Color color, TextAnchor align = TextAnchor.MiddleLeft, FontStyle style = FontStyle.Normal)
        {
            var label = Rect(parent, "Text").gameObject.AddComponent<Text>();
            label.font = Font;
            label.text = text;
            label.fontSize = size;
            label.color = color;
            label.alignment = align;
            label.fontStyle = style;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.lineSpacing = 1.05f;
            return label;
        }

        public static Button Button(Transform parent, string text, UnityAction onClick, Color? background = null, Color? foreground = null, int size = 26)
        {
            var image = Panel(parent, background ?? Palette.Azure, "Button " + text);
            var button = image.gameObject.AddComponent<Button>();
            button.targetGraphic = image;
            var colors = button.colors;
            colors.normalColor = Color.white;
            colors.highlightedColor = new Color(1.12f, 1.12f, 1.12f, 1);
            colors.selectedColor = Color.white;
            colors.pressedColor = new Color(0.8f, 0.8f, 0.8f, 1);
            colors.disabledColor = new Color(0.45f, 0.45f, 0.5f, 0.6f);
            colors.colorMultiplier = 1.2f;
            button.colors = colors;
            if (onClick != null) button.onClick.AddListener(onClick);
            image.gameObject.AddComponent<ButtonJuice>();
            var label = Label(image.transform, text, size, foreground ?? Palette.Hull, TextAnchor.MiddleCenter, FontStyle.Bold);
            label.rectTransform.Stretch(8, 4, 8, 4);
            return button;
        }

        public static Text LabelOf(Button button) => button.GetComponentInChildren<Text>();

        // Full-screen dim layer that blocks clicks to the map behind a window.
        public static Image Blocker(Transform parent, float alpha = 0.72f)
        {
            var image = Rect(parent, "Blocker").gameObject.AddComponent<Image>();
            image.color = Palette.Alpha(Palette.Space, alpha);
            image.rectTransform.Stretch();
            return image;
        }

        // Visible label required by the Space Apps guide for AI-generated (SI) images.
        public static void AiWatermark(Transform parent, Vector2 corner)
        {
            var tag = Panel(parent, Palette.Alpha(Palette.Space, 0.75f), "AI Watermark");
            tag.raycastTarget = false;
            var offset = new Vector2(corner.x < 0.5f ? 20 : -20, corner.y < 0.5f ? 20 : -20);
            tag.rectTransform.Anchor(corner, corner, offset, new Vector2(250, 36));
            Label(tag.transform, "AI-generated image", 20, Palette.Hull, TextAnchor.MiddleCenter).rectTransform.Stretch();
        }

        public static void Clear(Transform t)
        {
            for (int i = t.childCount - 1; i >= 0; i--) Object.Destroy(t.GetChild(i).gameObject);
        }
    }
}
