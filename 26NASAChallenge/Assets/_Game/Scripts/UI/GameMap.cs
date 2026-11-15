using System.Collections.Generic;
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
            frame.rectTransform.TopLeft((1920 - w) / 2 - 8, top - 8, w + 16, h + 16);

            var background = UIKit.Rect(root, "Map").gameObject.AddComponent<Image>();
            background.color = run.IsDrawing ? Palette.Hex("0B1A35") : Palette.Hex("2A3346");
            map = background.rectTransform;
            map.TopLeft((1920 - w) / 2, top, w, h);
            map.pivot = new Vector2(0, 0);
            map.anchoredPosition = new Vector2((1920 - w) / 2, -(top + h));
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

            var source = UIKit.Label(root, MapSource(), 18, Palette.Muted, TextAnchor.LowerRight);
            source.rectTransform.Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, BottomBar + 10), new Vector2(Side, 70));
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
            RefreshGhost();
        }

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
            ghost.gameObject.SetActive(true);
            ghost.color = Palette.Alpha(check.Ok ? Palette.Cyan : Palette.Danger, 0.55f);
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
            SetStatus(parts.Count == 0 ? "Good spot." : string.Join("   ", parts));
        }

        public void Hover(int x, int y)
        {
            if (x == hoverX && y == hoverY) return;
            hoverX = x;
            hoverY = y;
            if (run.BuildChoice.HasValue) RefreshGhost();
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

        // ---------- selection panel ----------

        Image selection;
        Image selectionIcon;
        Text selectionName, selectionInfo, selectionState;
        Button toggleButton, fixButton, removeButton;

        void BuildSelectionPanel()
        {
            selection = UIKit.Panel(root, Palette.Alpha(Palette.Panel, 0.97f), "Selection");
            selection.rectTransform.TopLeft(20, 1080 - BottomBar - 20 - 380, Side, 380);
            selectionIcon = UIKit.Icon(selection.transform, "Habitat", 56);
            selectionIcon.TopLeftOf(20, 20);
            selectionName = UIKit.Label(selection.transform, "", 30, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            selectionName.rectTransform.TopLeft(90, 20, Side - 150, 56);
            var close = UIKit.Button(selection.transform, "X", () => run.Deselect(), Palette.PanelLight, Palette.Hull, 22);
            close.GetComponent<RectTransform>().TopLeft(Side - 56, 16, 40, 40);
            selectionInfo = UIKit.Label(selection.transform, "", 21, Palette.Ice, TextAnchor.UpperLeft);
            selectionInfo.rectTransform.TopLeft(20, 86, Side - 40, 170);
            selectionState = UIKit.Label(selection.transform, "", 24, Palette.Mint, TextAnchor.MiddleLeft, FontStyle.Bold);
            selectionState.rectTransform.TopLeft(20, 258, Side - 40, 34);

            toggleButton = UIKit.Button(selection.transform, "Turn off", () => run.Toggle(run.SelectedId), Palette.Azure, Palette.Hull, 22);
            toggleButton.GetComponent<RectTransform>().TopLeft(20, 306, 180, 54);
            fixButton = UIKit.Button(selection.transform, "Fix", () => run.Repair(run.SelectedId), Palette.Amber, Palette.Ink, 22);
            fixButton.GetComponent<RectTransform>().TopLeft(20, 306, 180, 54);
            removeButton = UIKit.Button(selection.transform, "Remove", () => run.Remove(run.SelectedId), Palette.PanelLight, Palette.Ice, 22);
            removeButton.GetComponent<RectTransform>().TopLeft(Side - 160, 306, 140, 54);
        }

        void RefreshSelection(StepReport power)
        {
            var b = run.Selected;
            selection.gameObject.SetActive(b != null);
            if (crewList != null) crewList.gameObject.SetActive(b == null);
            if (b == null) return;
            var def = run.sim.balance.Def(b.type);
            var s = run.State;
            selectionIcon.sprite = Icons.Get(b.type.ToString());
            selectionName.text = def.displayName;

            var info = def.blurb;
            if (def.generationKW > 0) info += "\nMakes " + Num(run.sim.SolarOutput(s, b)) + " kW here.";
            if (def.demandKW > 0) info += "\nUses " + Num(run.sim.DemandOf(s, b)) + " kW.";
            if (def.batteryKWh > 0) info += "\nStores " + Num(def.batteryKWh) + " kWh.";
            if (b.type == BuildingType.RoverGarage)
            {
                double soilTrip = run.sim.TripHours(b, false), waterTrip = run.sim.TripHours(b, true);
                info += "\nTrip: soil " + Trip(soilTrip) + ", " + run.Planet.waterSource + " " + Trip(waterTrip) + ".";
            }
            if (run.sim.layoutRules)
                foreach (var c in Layout.Checks(run.sim.balance, run.sim.terrain, s, b))
                    info += "\n" + (c.met ? "+ " : "- ") + c.Text;
            selectionInfo.text = info;

            string state;
            Color color;
            if (b.broken) { state = "Broken"; color = Palette.Danger; }
            else if (!b.enabled) { state = "Off"; color = Palette.LunarGray; }
            else if (def.demandKW > 0 && !b.powered && s.step > 0) { state = "No power"; color = Palette.Amber; }
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

        static string Trip(double hours) => double.IsInfinity(hours) ? "too far" : SimText.Num(hours) + " h";

        // ---------- bottom bar: build cards and time controls ----------

        sealed class BuildCard { public BuildingType? type; public Button button; public Text name, cost, count; public Image icon; public string lockedWhy; }
        readonly List<BuildCard> cards = new List<BuildCard>();
        Button playButton, finishButton;
        Image playIcon;
        readonly List<(Button button, int speed)> speedButtons = new List<(Button, int)>();
        Text countdownText;

        void BuildBottomBar()
        {
            var bar = UIKit.Panel(root, Palette.Ink, "Bottom Bar", false);
            bar.rectTransform.anchorMin = new Vector2(0, 0);
            bar.rectTransform.anchorMax = new Vector2(1, 0);
            bar.rectTransform.pivot = new Vector2(0.5f, 0);
            bar.rectTransform.anchoredPosition = Vector2.zero;
            bar.rectTransform.sizeDelta = new Vector2(0, BottomBar);

            statusText = UIKit.Label(root, "", 22, Palette.Ice, TextAnchor.MiddleCenter);
            statusText.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, BottomBar + 6), new Vector2(1060, 34));
            statusText.rectTransform.pivot = new Vector2(0.5f, 0);

            int i = 0;
            foreach (var type in run.buildable) AddCard(bar.transform, i++, type, null);
            AddCard(bar.transform, i++, null, "Lab");
            if (!run.buildable.Contains(BuildingType.RoverGarage)) AddCard(bar.transform, i, null, "Rover Garage");

            if (run.IsDrawing)
            {
                finishButton = UIKit.Button(bar.transform, "Finish", PressFinish, Palette.Azure, Palette.Hull, 34);
                finishButton.GetComponent<RectTransform>().Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(330, 96));
                return;
            }
            BuildTimeControls(bar.transform);
        }

        void BuildTimeControls(Transform bar)
        {
            countdownText = UIKit.Label(bar, "", 20, Palette.Ice, TextAnchor.MiddleCenter, FontStyle.Bold);
            countdownText.rectTransform.Anchor(new Vector2(1, 1), new Vector2(1, 1), new Vector2(-20, -6), new Vector2(330, 30));

            playButton = UIKit.Button(bar, "", TogglePause, Palette.Azure, Palette.Hull, 22);
            playButton.GetComponent<RectTransform>().Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-266, 12), new Vector2(84, 72));
            playIcon = UIKit.Icon(playButton.transform, "pause", 44);
            playIcon.rectTransform.anchorMin = playIcon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            playIcon.rectTransform.anchoredPosition = Vector2.zero;

            for (int k = 0; k < MissionClock.Speeds.Length; k++)
            {
                int speed = MissionClock.Speeds[k];
                var b = UIKit.Button(bar, speed + "x", () => SetSpeed(speed), Palette.PanelLight, Palette.Hull, 26);
                b.GetComponent<RectTransform>().Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20 - (2 - k) * 80, 12), new Vector2(72, 72));
                speedButtons.Add((b, speed));
            }
        }

        public void TogglePause()
        {
            if (run.IsDrawing || run.Ended || ModalOpen) return;
            if (run.Phase == RunPhase.Briefing) { run.StartMission(); RefreshTime(); return; }
            run.clock.TogglePause();
            RefreshTime();
        }

        public void SetSpeed(int speed)
        {
            if (run.Ended) return;
            run.clock.SetSpeed(speed);
            if (run.clock.UserPaused) run.clock.SetPaused(false);
            RefreshTime();
        }

        // Countdown follows simulation time, so it stops while paused and speeds up at 2x and 4x.
        void RefreshTime()
        {
            if (run.IsDrawing || countdownText == null) return;
            bool paused = run.clock.Paused;
            playIcon.sprite = Icons.Get(paused ? "play" : "pause");
            playButton.image.color = paused ? Palette.Amber : Palette.Azure;
            foreach (var (button, speed) in speedButtons)
                button.image.color = speed == run.clock.Speed && !paused ? Palette.Azure : Palette.PanelLight;
            string left = SimText.Duration(run.TimeLeft, run.Planet.dayWord, run.Planet.dayHours);
            countdownText.text = run.Ended ? "Inspection" : "Evaluation in " + left + (paused ? "  ·  Paused" : "");
            countdownText.color = paused ? Palette.Amber : Palette.Ice;
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
            button.GetComponent<RectTransform>().TopLeft(16 + index * 142, 12, 134, 100);
            card.button = button;
            card.icon = UIKit.Icon(button.transform, type.HasValue ? type.Value.ToString() : "lock", 40);
            card.icon.TopLeftOf(10, 10);
            string name = type.HasValue ? run.sim.balance.Def(type.Value).displayName : lockedName;
            card.name = UIKit.Label(button.transform, name, 17, Palette.Hull, TextAnchor.UpperLeft, FontStyle.Bold);
            card.name.rectTransform.TopLeft(8, 56, 120, 36);
            card.name.resizeTextForBestFit = true;
            card.name.resizeTextMinSize = 11;
            card.name.resizeTextMaxSize = 17;
            card.name.horizontalOverflow = HorizontalWrapMode.Wrap;
            card.cost = UIKit.Label(button.transform, type.HasValue ? CostText(type.Value) : "Later", 17, Palette.Amber, TextAnchor.UpperRight, FontStyle.Bold);
            card.cost.rectTransform.TopLeft(52, 10, 76, 24);
            card.count = UIKit.Label(button.transform, "", 16, Palette.Muted, TextAnchor.UpperRight);
            card.count.rectTransform.TopLeft(52, 32, 76, 22);
            if (!type.HasValue) card.lockedWhy = lockedName + " is for a later mission. Power and people first.";
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
            SetStatus("Click the map to place the " + run.sim.balance.Def(type).displayName + ". Right-click to cancel.");
        }

        void RefreshBottomBar()
        {
            foreach (var card in cards)
            {
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
            if (Tile(e, out int x, out int y)) screen.Hover(x, y);
            else screen.Hover(-1, -1);
        }

        public void OnPointerExit(PointerEventData e) => screen.Hover(-1, -1);
    }
}
