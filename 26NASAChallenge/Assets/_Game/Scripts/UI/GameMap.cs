using System.Collections.Generic;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    public sealed partial class GameScreen
    {
        public const int Tile = 50;

        RectTransform map, buildingLayer, fxLayer;
        Image[,] tiles;
        Image[,] zoneShade;
        Image[,] suggestShade;
        RectTransform suggestLayer;
        readonly HashSet<(int x, int y)> suggested = new HashSet<(int, int)>();
        string suggestKey = "";
        int hoverBuilding;
        Image ghost, ghostArt;
        Image rover, walker;
        Vector2 walkerPos;
        double walkerHour;
        int hoverX = -1, hoverY = -1;
        readonly Dictionary<int, BuildingView> views = new Dictionary<int, BuildingView>();
        double lastShadow = -1;
        bool zoneShown;

        sealed class BuildingView
        {
            public Image body;
            public Image art;
            public Image badge;
            public Text badgeText;
            public Outline outline;
        }

        public static readonly Dictionary<BuildingType, string> ShortNames = new Dictionary<BuildingType, string>
        {
            { BuildingType.Habitat, "Habitat" }, { BuildingType.SolarArray, "Solar" }, { BuildingType.Battery, "" },
            { BuildingType.LifeSupport, "Air" }, { BuildingType.Communications, "" }, { BuildingType.Greenhouse, "Greenhouse" },
            { BuildingType.Shelter, "Shelter" }, { BuildingType.RoverGarage, "Rover" }
        };

        public static Color BuildingColor(BuildingType type)
        {
            switch (type)
            {
                case BuildingType.Habitat: return Palette.Hull;
                case BuildingType.SolarArray: return Palette.Hex("2E5FA8");
                case BuildingType.Battery: return Palette.Cyan;
                case BuildingType.LifeSupport: return Palette.Mint;
                case BuildingType.Communications: return Palette.Peach;
                case BuildingType.Greenhouse: return Palette.Hex("6CC283");
                case BuildingType.RoverGarage: return Palette.Amber;
                default: return Palette.Hex("C9A876");
            }
        }

        static Vector2 TilePos(int x, int y) => new Vector2(x * Tile, y * Tile);

        void BuildMap()
        {
            var terrain = run.sim.terrain;
            float w = terrain.width * Tile, h = terrain.height * Tile;
            float top = TopBar + (1080 - TopBar - BottomBar - h) / 2;
            var frame = UIKit.Panel(root, Palette.Ink, "Map Frame");
            frame.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(0.5f, 1), new Vector2(0, -(top - 8)), new Vector2(w + 16, h + 16));

            var background = UIKit.Rect(root, "Map").gameObject.AddComponent<Image>();
            background.color = run.IsDrawing ? Palette.Hex("0B1A35") : Palette.Hex("2A3346");
            map = background.rectTransform;
            map.anchorMin = map.anchorMax = new Vector2(0.5f, 1);
            map.pivot = new Vector2(0, 0);
            map.sizeDelta = new Vector2(w, h);
            map.anchoredPosition = new Vector2(-w / 2, -(top + h));
            var input = background.gameObject.AddComponent<MapInput>();
            input.screen = this;

            tiles = new Image[terrain.width, terrain.height];
            zoneShade = new Image[terrain.width, terrain.height];
            for (int y = 0; y < terrain.height; y++)
                for (int x = 0; x < terrain.width; x++)
                    tiles[x, y] = MapImage(map, "Tile", TilePos(x, y) + Vector2.one, new Vector2(Tile - 2, Tile - 2));
            PaintTerrain();
            if (!run.IsDrawing) AddGround();
            for (int y = 0; y < terrain.height; y++)
                for (int x = 0; x < terrain.width; x++)
                    AddFeature(x, y);
            for (int y = 0; y < terrain.height; y++)
                for (int x = 0; x < terrain.width; x++)
                {
                    zoneShade[x, y] = MapImage(map, "Zone", TilePos(x, y), new Vector2(Tile, Tile));
                    zoneShade[x, y].color = Palette.Alpha(Palette.Space, 0.55f);
                    zoneShade[x, y].enabled = false;
                }
            suggestLayer = UIKit.Rect(map, "Suggestions");
            suggestLayer.Stretch();
            suggestLayer.gameObject.AddComponent<CanvasGroup>().blocksRaycasts = false;
            suggestLayer.gameObject.AddComponent<GlowPulse>();
            suggestShade = new Image[terrain.width, terrain.height];
            for (int y = 0; y < terrain.height; y++)
                for (int x = 0; x < terrain.width; x++)
                {
                    suggestShade[x, y] = MapImage(suggestLayer, "Suggest", TilePos(x, y) + Vector2.one * 3, new Vector2(Tile - 6, Tile - 6));
                    suggestShade[x, y].sprite = UIKit.PanelSprite;
                    suggestShade[x, y].type = Image.Type.Sliced;
                    suggestShade[x, y].color = Palette.Alpha(Palette.Mint, 0.5f);
                    suggestShade[x, y].enabled = false;
                }

            buildingLayer = UIKit.Rect(map, "Buildings");
            buildingLayer.Stretch();
            rover = MapImage(map, "Rover", Vector2.zero, new Vector2(40, 30));
            rover.sprite = Icons.Get("rover");
            rover.preserveAspect = true;
            rover.enabled = false;
            ghost = MapImage(map, "Ghost", Vector2.zero, Vector2.one);
            ghost.sprite = UIKit.PanelSprite;
            ghost.type = Image.Type.Sliced;
            ghostArt = BottomImage(ghost.transform, null, Vector2.zero);
            ghostArt.color = new Color(1, 1, 1, 0.7f);
            ghost.gameObject.SetActive(false);
            walker = MapImage(map, "Geologist", Vector2.zero, new Vector2(40, 60));
            walker.enabled = false;
            fxLayer = UIKit.Rect(map, "Effects");
            fxLayer.Stretch();

            var badge = UIKit.Panel(root, Palette.Alpha(Palette.Ink, 0.88f), "Map Data");
            badge.rectTransform.Anchor(new Vector2(0.5f, 1), new Vector2(1, 0), new Vector2(w / 2 - 6, -(top + h - 6)), new Vector2(260, 38));
            UIKit.Label(badge.transform, run.mode == GameMode.Free ? "Made-up map  (i)" : "Real NASA map data  (i)", 20, Palette.Cyan, TextAnchor.MiddleCenter, FontStyle.Bold)
                .rectTransform.Stretch();
            Tip.Add(badge, () => new TipContent("About this map", "info", MapSource()));
        }

        static Image MapImage(RectTransform parent, string name, Vector2 pos, Vector2 size)
        {
            var image = UIKit.Rect(parent, name).gameObject.AddComponent<Image>();
            image.raycastTarget = false;
            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            rt.anchoredPosition = pos;
            rt.sizeDelta = size;
            return image;
        }

        // A light regolith texture over the data colours, and craters or rocks on tiles where nothing can be built.
        void AddGround()
        {
            var terrain = run.sim.terrain;
            var texture = PixelArt.Ground(run.Planet.id);
            if (texture != null)
            {
                var ground = UIKit.Rect(map, "Ground").gameObject.AddComponent<RawImage>();
                ground.texture = texture;
                ground.raycastTarget = false;
                ground.color = new Color(1, 1, 1, 0.28f);
                ground.rectTransform.Stretch();
                ground.uvRect = new Rect(0, 0, terrain.width * Tile / 128f, terrain.height * Tile / 128f);
            }
            for (int y = 0; y < terrain.height; y++)
                for (int x = 0; x < terrain.width; x++)
                {
                    int hash = (x * 73856093) ^ (y * 19349663);
                    if (!terrain.IsBlocked(x, y) || (hash & 3) == 0) continue;
                    string kind = terrain.IsWater(x, y) ? "crater" : "rocks";
                    var sprite = PixelArt.Prop(run.Planet.id, kind + ((hash & 4) == 0 ? "_medium" : "_small"));
                    if (sprite == null) continue;
                    var prop = MapImage(map, "Prop", TilePos(x, y) + new Vector2(4, 8), new Vector2(Tile - 8, Tile - 16));
                    prop.sprite = sprite;
                    prop.preserveAspect = true;
                }
        }

        static Image BottomImage(Transform parent, Sprite sprite, Vector2 size)
        {
            var image = UIKit.Rect(parent, "Art").gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.raycastTarget = false;
            var rt = image.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 2);
            rt.sizeDelta = size;
            return image;
        }

        static Vector2 ArtSize(Sprite sprite, BuildingDef def)
        {
            float w = def.width * Tile, h = def.height * Tile;
            return sprite.rect.size * Mathf.Min((w - 4) / sprite.rect.width, (h + Tile * 0.5f) / sprite.rect.height);
        }

        // Small pixel marks so soil, ice and clay are easy to spot.
        void AddFeature(int x, int y)
        {
            var t = run.sim.terrain;
            if (run.IsDrawing) return;
            if (t.IsSoil(x, y))
                foreach (var p in new[] { new Vector2(10, 12), new Vector2(28, 30), new Vector2(34, 10) })
                    MapImage(map, "Soil", TilePos(x, y) + p, new Vector2(8, 6)).color = Palette.Hex("8A5A3C");
            if (t.IsWater(x, y))
            {
                var color = run.Planet.id == "Mars" ? Palette.Hex("5FA8A0") : Palette.Hex("9FE3F0");
                MapImage(map, "Water", TilePos(x, y) + new Vector2(14, 20), new Vector2(8, 8)).color = color;
                MapImage(map, "Water", TilePos(x, y) + new Vector2(30, 32), new Vector2(6, 6)).color = color;
            }
        }

        string MapSource()
        {
            if (run.mode == GameMode.Free) return "Made-up map. Brighter ground gets more sun.\nBrown dots: soft soil. Blue dots: water.";
            if (run.Planet.id == "Mars") return "Map: real heights of Jezero crater\n(NASA MGS MOLA, simplified). Brown: lake mud. Teal: wet clay.";
            return "Map: real sunlight at the Moon's south pole\n(NASA LRO/LOLA, simplified). Brown: soft soil. Blue: ice.";
        }

        Color TerrainColor(int x, int y, double shade)
        {
            var terrain = run.sim.terrain;
            float sun = Mathf.Clamp01((float)((terrain.Sun(x, y) - 0.5) / 0.5));
            Color low, high, rock;
            if (run.IsDrawing) { low = Palette.Hex("1A3560"); high = Palette.Hex("86B6EE"); rock = Palette.Hex("0D1C38"); }
            else if (run.Planet.id == "Mars") { low = Palette.Hex("7E4038"); high = Palette.Hex("E3A57B"); rock = Palette.Hex("4A2622"); }
            else { low = Palette.Hex("5E6B85"); high = Palette.Hex("D5DDE6"); rock = Palette.Hex("323B50"); }
            var c = terrain.IsBlocked(x, y) ? rock : Color.Lerp(low, high, sun);
            return Color.Lerp(Palette.Space, c, (float)shade);
        }

        void PaintTerrain()
        {
            var terrain = run.sim.terrain;
            double shade = run.State.shadowFactor < 1 ? 0.7 : 1;
            lastShadow = run.State.shadowFactor;
            for (int y = 0; y < terrain.height; y++)
                for (int x = 0; x < terrain.width; x++)
                    tiles[x, y].color = TerrainColor(x, y, shade);
        }

        void RefreshMap()
        {
            if (run.State.shadowFactor != lastShadow) PaintTerrain();
            var s = run.State;
            var alive = new HashSet<int>();
            foreach (var b in s.buildings)
            {
                alive.Add(b.id);
                if (!views.TryGetValue(b.id, out var view)) views[b.id] = view = CreateView(b);
                UpdateView(view, b);
            }
            var gone = new List<int>();
            foreach (var pair in views)
                if (!alive.Contains(pair.Key)) gone.Add(pair.Key);
            foreach (int id in gone)
            {
                Destroy(views[id].body.gameObject);
                views.Remove(id);
            }
            var order = new List<PlacedBuilding>(s.buildings);
            order.Sort((a, b) => Top(b).CompareTo(Top(a)));
            for (int i = 0; i < order.Count; i++) views[order[i].id].body.transform.SetSiblingIndex(i);
            RefreshZone();
            RefreshSuggestions();
            RefreshGhost();
        }

        // Optional green glow on good spots for the chosen building (Layout.Suggest). Never a placement rule.
        void RefreshSuggestions()
        {
            var type = run.BuildChoice;
            bool show = type.HasValue && Progress.SuggestionsOn && !run.Ended;
            string key = show ? type.Value + ":" + run.State.buildings.Count + ":" + run.State.nextId : "";
            if (key == suggestKey) return;
            suggestKey = key;
            suggested.Clear();
            var t = run.sim.terrain;
            var cover = new bool[t.width, t.height];
            if (show)
            {
                var def = run.sim.balance.Def(type.Value);
                foreach (var spot in Layout.Suggest(run.sim, run.State, type.Value))
                {
                    suggested.Add(spot);
                    for (int dy = 0; dy < def.height; dy++)
                        for (int dx = 0; dx < def.width; dx++)
                            if (t.InBounds(spot.x + dx, spot.y + dy)) cover[spot.x + dx, spot.y + dy] = true;
                }
            }
            for (int y = 0; y < t.height; y++)
                for (int x = 0; x < t.width; x++)
                    suggestShade[x, y].enabled = cover[x, y];
        }

        public int SuggestedCount => suggested.Count;

        int Top(PlacedBuilding b) => b.y + run.sim.balance.Def(b.type).height;

        // While placing, the area outside the base is shaded.
        void RefreshZone()
        {
            bool show = run.BuildChoice.HasValue && run.sim.layoutRules && run.State.Count(BuildingType.Habitat) > 0;
            if (!show && !zoneShown) return;
            zoneShown = show;
            var t = run.sim.terrain;
            for (int y = 0; y < t.height; y++)
                for (int x = 0; x < t.width; x++)
                    zoneShade[x, y].enabled = show && !Placement.InsideBaseTile(run.sim, run.State, x, y);
        }

        BuildingView CreateView(PlacedBuilding b)
        {
            var def = run.sim.balance.Def(b.type);
            var body = UIKit.Panel(buildingLayer, Palette.Alpha(BuildingColor(b.type), 0.3f), "Building " + b.type);
            body.raycastTarget = false;
            var rt = body.rectTransform;
            rt.anchorMin = rt.anchorMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);
            rt.sizeDelta = new Vector2(def.width * Tile - 6, def.height * Tile - 6);
            rt.anchoredPosition = TilePos(b.x, b.y) + new Vector2(def.width * Tile, def.height * Tile) / 2;

            var view = new BuildingView { body = body };
            var sprite = PixelArt.Building(b.type);
            if (sprite != null)
            {
                var size = ArtSize(sprite, def);
                var shadow = PixelArt.Shadow(b.type);
                if (shadow != null) BottomImage(body.transform, shadow, size);
                view.art = BottomImage(body.transform, sprite, size);
                if (run.Planet.id == "Mars")
                    foreach (var overlay in new[] { PixelArt.MarsSurface(b.type), PixelArt.Dust(b.type) })
                        if (overlay != null) BottomImage(body.transform, overlay, size);
            }
            else
            {
                var icon = UIKit.Icon(body.transform, b.type.ToString(), Mathf.Min(def.width, def.height) * Tile * 0.62f);
                icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
                string name = ShortNames[b.type];
                if (def.width >= 2 && def.height >= 2 && name.Length > 0)
                {
                    icon.rectTransform.anchoredPosition = new Vector2(0, 8);
                    var label = UIKit.Label(body.transform, name, 16, Palette.Ink, TextAnchor.LowerCenter, FontStyle.Bold);
                    label.rectTransform.Stretch(2, 0, 2, 4);
                }
            }

            view.badge = UIKit.Rect(body.transform, "Badge").gameObject.AddComponent<Image>();
            view.badge.sprite = UIKit.CircleSprite;
            view.badge.raycastTarget = false;
            view.badge.rectTransform.Anchor(new Vector2(1, 1), new Vector2(0.5f, 0.5f), new Vector2(-4, -4), new Vector2(26, 26));
            view.badgeText = UIKit.Label(view.badge.transform, "", 14, Palette.Ink, TextAnchor.MiddleCenter, FontStyle.Bold);
            view.badgeText.rectTransform.Stretch();
            view.outline = body.gameObject.AddComponent<Outline>();
            view.outline.effectColor = Palette.Cyan;
            view.outline.effectDistance = new Vector2(3, -3);
            return view;
        }

        void UpdateView(BuildingView view, PlacedBuilding b)
        {
            var def = run.sim.balance.Def(b.type);
            string badge = "";
            Color badgeColor = Palette.Amber;
            bool dim = false;
            if (b.broken) { badge = "X"; badgeColor = Palette.Danger; dim = true; }
            else if (!b.enabled) { badge = "off"; badgeColor = Palette.LunarGray; dim = true; }
            else if (!run.IsDrawing && def.demandKW > 0 && !b.powered && run.State.step > 0) { badge = "!"; dim = true; }
            else if (!run.IsDrawing && b.type == BuildingType.SolarArray && !run.sim.HasCable(run.State, b)) { badge = "~"; }
            var plate = dim ? Color.Lerp(BuildingColor(b.type), Palette.Space, 0.45f) : BuildingColor(b.type);
            view.body.color = view.art != null ? Palette.Alpha(plate, 0.3f) : plate;
            if (view.art != null) view.art.color = dim ? new Color(0.5f, 0.5f, 0.58f) : Color.white;
            view.badge.gameObject.SetActive(badge.Length > 0);
            view.badge.color = badgeColor;
            view.badgeText.text = badge;
            view.outline.enabled = b.id == run.SelectedId;
        }

        // The geologist walks outside the Habitat, and heads back to the door when called in.
        void UpdateWalker()
        {
            var s = run.State;
            var habitat = s.First(BuildingType.Habitat);
            bool outside = habitat != null && !run.IsDrawing && s.geologist != CrewLocation.Inside;
            walker.enabled = outside;
            if (!outside) { walkerHour = s.timeHours; return; }
            var def = run.sim.balance.Def(BuildingType.Habitat);
            Vector2 door = TilePos(habitat.x, habitat.y) + new Vector2(def.width * Tile / 2f, -Tile * 0.3f);
            Vector2 site = door + new Vector2(def.width * Tile * 0.5f + Tile * 2, -Tile * 1.5f);
            site.x = Mathf.Clamp(site.x, Tile * 2.5f, map.sizeDelta.x - 20);
            site.y = Mathf.Clamp(site.y, 0, map.sizeDelta.y - Tile * 2.5f);
            door.y = Mathf.Max(door.y, 0);
            Vector2 before = walkerPos;
            if (s.geologist == CrewLocation.Outside)
                walkerPos = Vector2.Lerp(site, site + new Vector2(-Tile * 2, Tile), Mathf.PingPong((float)s.timeHours * 0.4f, 1));
            else
                walkerPos = Vector2.MoveTowards(walkerPos, door, (float)(s.timeHours - walkerHour) * Tile * 4);
            walkerHour = s.timeHours;
            Vector2 move = walkerPos - before;
            string dir = (move.y > 0 ? "N" : "S") + (move.x < 0 ? "W" : "E");
            walker.sprite = PixelArt.Frame("walk_" + dir + "_" + (int)(s.timeHours * 12) % 4) ?? Icons.Get("crew");
            walker.rectTransform.anchoredPosition = walkerPos - new Vector2(20, 0);
            walker.transform.SetAsLastSibling();
        }

        // The rover drives out to soil or water and back, following the simulation's trip progress.
        void UpdateRover()
        {
            UpdateWalker();
            if (rover == null) return;
            var s = run.State;
            var garage = s.First(BuildingType.RoverGarage);
            bool driving = garage != null && garage.powered && garage.enabled && s.roverCargo != "" && !run.IsDrawing;
            rover.enabled = driving;
            if (!driving) return;
            var t = run.sim.terrain;
            var def = run.sim.balance.Def(garage.type);
            Vector2 home = TilePos(garage.x, garage.y) + new Vector2(def.width * Tile, def.height * Tile) / 2;
            Vector2 target = home;
            float best = float.MaxValue;
            bool water = s.roverCargo == "water";
            for (int y = 0; y < t.height; y++)
                for (int x = 0; x < t.width; x++)
                {
                    if (water ? !t.IsWater(x, y) : !t.IsSoil(x, y)) continue;
                    Vector2 p = TilePos(x, y) + Vector2.one * Tile / 2;
                    float d = (p - home).sqrMagnitude;
                    if (d < best) { best = d; target = p; }
                }
            float k = (float)s.roverTrip;
            float leg = k < 0.5f ? k * 2 : 2 - k * 2;
            rover.rectTransform.anchoredPosition = Vector2.Lerp(home, target, Mathf.SmoothStep(0, 1, leg)) - new Vector2(20, 15);
            rover.rectTransform.localScale = new Vector3(target.x < home.x == k < 0.5f ? -1 : 1, 1, 1);
        }

        void RefreshGhost()
        {
            if (!run.BuildChoice.HasValue || hoverX < 0)
            {
                ghost.gameObject.SetActive(false);
                if (run.BuildChoice.HasValue)
                    SetStatus("Click the map to place the " + run.sim.balance.Def(run.BuildChoice.Value).displayName + ". Right-click to cancel.");
                return;
            }
            var type = run.BuildChoice.Value;
            var def = run.sim.balance.Def(type);
            var check = run.sim.CanPlace(run.State, type, hoverX, hoverY);
            bool good = check.Ok && suggested.Contains((hoverX, hoverY));
            ghost.gameObject.SetActive(true);
            ghost.color = Palette.Alpha(!check.Ok ? Palette.Danger : good ? Palette.Mint : Palette.Cyan, 0.6f);
            ghost.rectTransform.anchoredPosition = TilePos(hoverX, hoverY) + Vector2.one * 2;
            ghost.rectTransform.sizeDelta = new Vector2(def.width * Tile - 4, def.height * Tile - 4);
            ghostArt.sprite = PixelArt.Building(type);
            ghostArt.enabled = ghostArt.sprite != null;
            if (ghostArt.enabled) ghostArt.rectTransform.sizeDelta = ArtSize(ghostArt.sprite, def);
            if (!check.Ok) { SetStatus(check.message, true); return; }
            var parts = new List<string>();
            if (def.generationKW > 0)
                parts.Add("Sun here: " + Mathf.RoundToInt((float)run.sim.terrain.AverageSun(hoverX, hoverY, def.width, def.height) * 100) + "%");
            if (run.sim.layoutRules)
                foreach (var c in Layout.Checks(run.sim.balance, run.sim.terrain, run.State, type, hoverX, hoverY))
                    parts.Add((c.met ? "+ " : "- ") + c.Text);
            if (good) parts.Insert(0, "Suggested spot!");
            SetStatus(parts.Count == 0 ? "You can build here." : string.Join("   ", parts));
        }

        public void Hover(int x, int y) => Hover(x, y, Vector2.zero);

        // While placing: move the ghost. Otherwise: explain the building under the pointer.
        public void Hover(int x, int y, Vector2 screen)
        {
            if (x == hoverX && y == hoverY) return;
            hoverX = x;
            hoverY = y;
            if (run.BuildChoice.HasValue) { RefreshGhost(); return; }
            var b = x < 0 ? null : Placement.BuildingAt(run.sim.balance, run.State, x, y);
            int id = b == null ? 0 : b.id;
            if (id == hoverBuilding) return;
            hoverBuilding = id;
            if (b == null) { TooltipView.Hide(map); return; }
            var tip = HelpText.Building(run.sim.balance, b.type, run.sim.layoutRules);
            tip.body = StateText(b) + "\n" + tip.body.Split('\n')[0] + "\nClick it to see more.";
            TooltipView.Show(map, tip, screen);
        }

        string StateText(PlacedBuilding b)
        {
            var def = run.sim.balance.Def(b.type);
            if (b.broken) return "Broken.";
            if (!b.enabled) return "Switched off.";
            if (def.demandKW > 0 && !b.powered && run.State.step > 0 && !run.IsDrawing) return "No power!";
            return "Working.";
        }

        public void ClickTile(int x, int y)
        {
            if (ModalOpen || run.Ended) return;
            if (run.BuildChoice.HasValue)
            {
                var type = run.BuildChoice.Value;
                var result = run.Place(x, y);
                if (result.Ok)
                {
                    SetStatus(run.sim.balance.Def(type).displayName + " built.");
                    Refresh();
                    var def = run.sim.balance.Def(type);
                    UIFx.Ripple(fxLayer, TilePos(x, y), new Vector2(def.width * Tile, def.height * Tile));
                    if (views.TryGetValue(result.building.id, out var view)) UIFx.Pop(view.body.transform);
                }
                else
                {
                    SetStatus(result.message, true);
                    UIFx.Shake(ghost.transform);
                }
                return;
            }
            run.ClickTile(x, y);
            SetStatus("");
        }

        public void CancelBuild()
        {
            run.Deselect();
            SetStatus("");
        }

        // ---------- selection panel (left): the selected building, or the building being placed ----------

        Image selection;
        Image selectionIcon;
        Text selectionName, selectionInfo, selectionState;
        Button toggleButton, fixButton, removeButton, cancelButton, suggestButton;
        float SelectionHeight => Mathf.Min(run.IsDrawing ? 560 : 1080, 1080 - BottomBar - Gap - 64 - Gap - LeftTop - Gap);

        void BuildSelectionPanel()
        {
            float h = SelectionHeight;
            selection = UIKit.Panel(root, Palette.Alpha(Palette.Panel, 0.97f), "Selection");
            selection.rectTransform.TopLeft(20, LeftTop, Side, h);
            selectionIcon = UIKit.Icon(selection.transform, "Habitat", 56);
            selectionIcon.TopLeftOf(18, 16);
            selectionName = UIKit.Label(selection.transform, "", 28, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            selectionName.rectTransform.TopLeft(86, 16, Side - 146, 56);
            var close = UIKit.Button(selection.transform, "X", () => run.Deselect(), Palette.PanelLight, Palette.Hull, 22);
            close.GetComponent<RectTransform>().TopLeft(Side - 56, 16, 40, 40);
            selectionState = UIKit.Label(selection.transform, "", 24, Palette.Mint, TextAnchor.MiddleLeft, FontStyle.Bold);
            selectionState.rectTransform.TopLeft(20, 78, Side - 40, 30);
            selectionInfo = UIKit.Label(selection.transform, "", 21, Palette.Ice, TextAnchor.UpperLeft);
            selectionInfo.rectTransform.TopLeft(20, 112, Side - 40, h - 112 - 76);

            float buttons = h - 70;
            toggleButton = UIKit.Button(selection.transform, "Turn off", () => run.Toggle(run.SelectedId), Palette.Azure, Palette.Hull, 22);
            toggleButton.GetComponent<RectTransform>().TopLeft(20, buttons, 180, 54);
            fixButton = UIKit.Button(selection.transform, "Fix", () => run.Repair(run.SelectedId), Palette.Amber, Palette.Ink, 22);
            fixButton.GetComponent<RectTransform>().TopLeft(20, buttons, 180, 54);
            removeButton = UIKit.Button(selection.transform, "Remove", () => run.Remove(run.SelectedId), Palette.PanelLight, Palette.Ice, 22);
            removeButton.GetComponent<RectTransform>().TopLeft(Side - 160, buttons, 140, 54);
            cancelButton = UIKit.Button(selection.transform, "Cancel", CancelBuild, Palette.PanelLight, Palette.Hull, 22);
            cancelButton.GetComponent<RectTransform>().TopLeft(20, buttons, 150, 54);
            suggestButton = UIKit.Button(selection.transform, "", ToggleSuggestions, Palette.PanelLight, Palette.Hull, 20);
            suggestButton.GetComponent<RectTransform>().TopLeft(Side - 230, buttons, 210, 54);
        }

        void ToggleSuggestions()
        {
            Progress.SuggestionsOn = !Progress.SuggestionsOn;
            suggestKey = "?";
            Refresh();
        }

        void RefreshSelection(StepReport power)
        {
            if (run.BuildChoice.HasValue) { ShowPlacing(run.BuildChoice.Value); return; }
            var b = run.Selected;
            selection.gameObject.SetActive(b != null);
            cancelButton.gameObject.SetActive(false);
            suggestButton.gameObject.SetActive(false);
            if (b == null) return;
            var def = run.sim.balance.Def(b.type);
            var s = run.State;
            selectionIcon.sprite = Icons.Get(b.type.ToString());
            selectionName.text = def.displayName;

            var page = HelpData.Load().BuildingPage(b.type.ToString());
            var info = page != null ? page.summary : def.blurb;
            if (def.generationKW > 0) info += "\nMakes " + Num(run.sim.SolarOutput(s, b)) + " kW here.";
            if (def.demandKW > 0) info += "\nUses " + Num(run.sim.DemandOf(s, b)) + " kW." + (def.optional ? "" : " Always on.");
            if (def.batteryKWh > 0) info += "\nStores " + Num(def.batteryKWh) + " kWh.";
            if (b.type == BuildingType.RoverGarage)
            {
                double soilTrip = run.sim.TripHours(b, false), waterTrip = run.sim.TripHours(b, true);
                info += "\nTrip: soil " + Trip(soilTrip) + ", " + run.Planet.waterSource + " " + Trip(waterTrip) + ".";
            }
            if (run.sim.layoutRules)
                foreach (var c in Layout.Checks(run.sim.balance, run.sim.terrain, s, b))
                    info += "\n" + (c.met ? "Good: " : "Tip: ") + c.Text;
            if (page != null && page.rows.Length > 1) info += "\nWhy it matters: " + page.rows[page.rows.Length - 1].text;
            selectionInfo.text = Keywords.Mark(info);

            string state;
            Color color;
            if (b.broken) { state = "Broken"; color = Palette.Danger; }
            else if (!b.enabled) { state = "Switched off"; color = Palette.LunarGray; }
            else if (def.demandKW > 0 && !b.powered && s.step > 0) { state = "No power!"; color = Palette.Amber; }
            else { state = "Working"; color = Palette.Mint; }
            selectionState.text = state;
            selectionState.color = color;

            bool canToggle = !run.IsDrawing && def.optional && !b.broken && !run.Ended;
            toggleButton.gameObject.SetActive(canToggle);
            UIKit.LabelOf(toggleButton).text = b.enabled ? "Turn off" : "Turn on";
            fixButton.gameObject.SetActive(b.broken && !run.Ended);
            UIKit.LabelOf(fixButton).text = "Fix (" + run.sim.balance.repairCost + ")";
            fixButton.interactable = s.supplies >= run.sim.balance.repairCost;
            removeButton.gameObject.SetActive(!run.Ended && !(b.type == BuildingType.Habitat && s.buildings.Count > 1));
        }

        bool CanSuggest(BuildingType type) =>
            type == BuildingType.SolarArray || (run.sim.layoutRules && Layout.Checks(run.sim.balance, run.sim.terrain, run.State, type, 0, 0).Count > 0);

        // What the chosen building does, what it costs, where it works best, and how to read the map colours.
        void ShowPlacing(BuildingType type)
        {
            selection.gameObject.SetActive(true);
            toggleButton.gameObject.SetActive(false);
            fixButton.gameObject.SetActive(false);
            removeButton.gameObject.SetActive(false);
            cancelButton.gameObject.SetActive(true);
            bool canSuggest = CanSuggest(type);
            suggestButton.gameObject.SetActive(canSuggest);
            UIKit.LabelOf(suggestButton).text = "Suggestions: " + (Progress.SuggestionsOn ? "On" : "Off");

            var def = run.sim.balance.Def(type);
            selectionIcon.sprite = Icons.Get(type.ToString());
            selectionName.text = "Placing: " + def.displayName;
            selectionState.text = "Click the map to build";
            selectionState.color = Palette.Cyan;
            var tip = HelpText.Building(run.sim.balance, type, run.sim.layoutRules);
            var lines = new List<string> { tip.body, "" };
            if (canSuggest && Progress.SuggestionsOn)
                lines.Add(SuggestedCount > 0 ? "Green glow: suggested spots. Only a tip: you may build on any allowed tile." : "No perfect spot right now. Any allowed spot still works.");
            if (run.sim.layoutRules && run.State.Count(BuildingType.Habitat) > 0) lines.Add("Dark area: outside your base. You cannot build there.");
            lines.Add("Red outline: that spot is blocked.");
            selectionInfo.text = Keywords.Mark(string.Join("\n", lines));
        }

        static string Trip(double hours) => double.IsInfinity(hours) ? "too far" : SimText.Num(hours) + " h";

        // ---------- bottom bar: Buildings ----------

        sealed class BuildCard { public BuildingType? type; public Button button; public Text name, cost, count; public Image icon; public Outline glow; public string lockedWhy; }
        readonly List<BuildCard> cards = new List<BuildCard>();
        Button finishButton;
        Text buildingsTitle;

        static readonly Dictionary<BuildingType, string> Outputs = new Dictionary<BuildingType, string>
        {
            { BuildingType.SolarArray, "power" }, { BuildingType.Battery, "battery" }, { BuildingType.LifeSupport, "air" },
            { BuildingType.Greenhouse, "food" }, { BuildingType.RoverGarage, "soil" }, { BuildingType.Shelter, "crew" }
        };

        void BuildBottomBar()
        {
            var bar = UIKit.Panel(root, Palette.Ink, "Bottom Bar", false);
            bar.rectTransform.anchorMin = new Vector2(0, 0);
            bar.rectTransform.anchorMax = new Vector2(1, 0);
            bar.rectTransform.pivot = new Vector2(0.5f, 0);
            bar.rectTransform.anchoredPosition = Vector2.zero;
            bar.rectTransform.sizeDelta = new Vector2(0, BottomBar);

            buildingsTitle = UIKit.Label(bar.transform, "BUILDINGS  <color=#B9C3CF>· pick one, then click the map</color>", 24, Palette.Cyan, TextAnchor.MiddleLeft, FontStyle.Bold);
            buildingsTitle.rectTransform.TopLeft(18, 4, 900, 32);

            statusText = UIKit.Label(root, "", 23, Palette.Ice, TextAnchor.MiddleCenter);
            statusText.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, BottomBar + 6), new Vector2(1000, 36));
            statusText.rectTransform.pivot = new Vector2(0.5f, 0);

            int i = 0;
            foreach (var type in run.buildable) AddCard(bar.transform, i++, type, null);
            AddCard(bar.transform, i++, null, "Lab");
            if (!run.buildable.Contains(BuildingType.RoverGarage)) AddCard(bar.transform, i, null, "Rover Garage");

            if (run.IsDrawing)
            {
                finishButton = UIKit.Button(bar.transform, "Finish", PressFinish, Palette.Azure, Palette.Hull, 34);
                finishButton.GetComponent<RectTransform>().Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, -10), new Vector2(330, 96));
            }
        }

        // During the first lesson the Buildings label glows so a new player knows where to look.
        void HighlightBuildings(bool on)
        {
            if (buildingsTitle == null) return;
            buildingsTitle.color = on && Mathf.PingPong(Time.unscaledTime * 2, 1) > 0.5f ? Palette.Amber : Palette.Cyan;
        }

        public void PressFinish()
        {
            if (ModalOpen || run.Ended) return;
            if (!run.DrawingReady) { SetStatus("First: " + run.CurrentGoal, true); return; }
            run.FinishDrawing();
        }

        void AddCard(Transform bar, int index, BuildingType? type, string lockedName)
        {
            var card = new BuildCard { type = type };
            var button = UIKit.Button(bar, "", () => ClickCard(card), Palette.PanelLight);
            button.GetComponent<RectTransform>().TopLeft(16 + index * 142, 38, 134, 92);
            card.button = button;
            card.glow = button.gameObject.AddComponent<Outline>();
            card.glow.effectColor = Palette.Amber;
            card.glow.effectDistance = new Vector2(4, -4);
            card.glow.enabled = false;
            card.icon = UIKit.Icon(button.transform, type.HasValue ? type.Value.ToString() : "lock", 40);
            card.icon.TopLeftOf(10, 8);
            string name = type.HasValue ? run.sim.balance.Def(type.Value).displayName : lockedName;
            card.name = UIKit.Label(button.transform, name, 18, Palette.Hull, TextAnchor.UpperLeft, FontStyle.Bold);
            card.name.rectTransform.TopLeft(8, 52, 122, 38);
            card.name.resizeTextForBestFit = true;
            card.name.resizeTextMinSize = 13;
            card.name.resizeTextMaxSize = 18;
            card.name.horizontalOverflow = HorizontalWrapMode.Wrap;
            card.cost = UIKit.Label(button.transform, type.HasValue ? CostText(type.Value) : "Later", 18, Palette.Amber, TextAnchor.UpperRight, FontStyle.Bold);
            card.cost.rectTransform.TopLeft(52, 6, 76, 24);
            card.count = UIKit.Label(button.transform, "", 17, Palette.Ice, TextAnchor.UpperRight);
            card.count.rectTransform.TopLeft(80, 28, 48, 22);
            if (type.HasValue && Outputs.TryGetValue(type.Value, out var output)) UIKit.Icon(button.transform, output, 22).TopLeftOf(54, 29);
            if (type.HasValue)
            {
                var t = type.Value;
                Tip.Add(button, () => HelpText.Building(run.sim.balance, t, run.sim.layoutRules), false);
            }
            else
            {
                card.lockedWhy = lockedName + " is for a later mission. Power and people first.";
                Tip.Add(button, () => new TipContent(lockedName, "lock", card.lockedWhy), false);
            }
            cards.Add(card);
        }

        string CostText(BuildingType type)
        {
            var def = run.sim.balance.Def(type);
            return def.soilCost > 0 && run.sim.layoutRules ? def.cost + "+" + def.soilCost + "s" : def.cost.ToString();
        }

        void ClickCard(BuildCard card)
        {
            if (ModalOpen || run.Ended) return;
            if (!card.type.HasValue) { SetStatus(card.lockedWhy, true); return; }
            var type = card.type.Value;
            if (run.BuildChoice == type) { CancelBuild(); return; }
            if (!run.CanBuild(type, out string why)) { SetStatus(run.sim.balance.Def(type).displayName + ": " + why, true); UIFx.Shake(card.button.transform); return; }
            run.Choose(type);
            SetStatus("Click the map to place the " + run.sim.balance.Def(type).displayName + ". Right-click or Cancel to stop.");
        }

        void RefreshBottomBar()
        {
            RectTransform coached = coach != null && coach.Showing ? CoachTarget(out _) : null;
            foreach (var card in cards)
            {
                card.glow.enabled = coached != null && coached == (RectTransform)card.button.transform;
                if (!card.type.HasValue)
                {
                    card.button.image.color = Palette.Alpha(Palette.Panel, 0.8f);
                    card.name.color = Palette.Muted;
                    continue;
                }
                var type = card.type.Value;
                bool can = run.CanBuild(type, out _);
                bool chosen = run.BuildChoice == type;
                card.button.image.color = chosen ? Palette.Azure : can ? Palette.PanelLight : Palette.Alpha(Palette.Panel, 0.8f);
                card.name.color = can || chosen ? Palette.Hull : Palette.Muted;
                card.icon.color = can || chosen ? Color.white : new Color(1, 1, 1, 0.4f);
                card.count.text = run.CountText(type);
                bool locked = !Placement.IsUnlocked(run.State, type);
                card.cost.text = locked ? "Locked" : CostText(type);
            }
            if (finishButton != null) finishButton.image.color = run.DrawingReady ? Palette.Azure : Palette.PanelLight;
            RefreshTime();
        }
    }

    // Turns pointer events on the map into tile coordinates.
    public sealed class MapInput : MonoBehaviour, IPointerClickHandler, IPointerMoveHandler, IPointerExitHandler
    {
        public GameScreen screen;

        bool Tile(PointerEventData e, out int x, out int y)
        {
            x = y = -1;
            var rt = (RectTransform)transform;
            if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, e.position, e.pressEventCamera, out var local)) return false;
            x = Mathf.FloorToInt(local.x / GameScreen.Tile);
            y = Mathf.FloorToInt(local.y / GameScreen.Tile);
            return x >= 0 && y >= 0 && local.x < rt.rect.width && local.y < rt.rect.height;
        }

        public void OnPointerClick(PointerEventData e)
        {
            if (e.button == PointerEventData.InputButton.Right) { screen.CancelBuild(); return; }
            if (Tile(e, out int x, out int y)) screen.ClickTile(x, y);
        }

        public void OnPointerMove(PointerEventData e)
        {
            if (Tile(e, out int x, out int y)) screen.Hover(x, y, e.position);
            else screen.Hover(-1, -1, e.position);
        }

        public void OnPointerExit(PointerEventData e) => screen.Hover(-1, -1, e.position);
    }
}
