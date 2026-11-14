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

        RectTransform map, buildingLayer;
        Image[,] tiles;
        Image ghost;
        int hoverX = -1, hoverY = -1;
        readonly Dictionary<int, BuildingView> views = new Dictionary<int, BuildingView>();
        double lastShadow = -1;

        sealed class BuildingView
        {
            public Image body;
            public Image badge;
            public Text badgeText;
            public Outline outline;
        }

        public static readonly Dictionary<BuildingType, string> ShortNames = new Dictionary<BuildingType, string>
        {
            { BuildingType.Habitat, "Habitat" }, { BuildingType.SolarArray, "Solar" }, { BuildingType.Battery, "" },
            { BuildingType.LifeSupport, "Air" }, { BuildingType.Communications, "" }, { BuildingType.Greenhouse, "Greenhouse" },
            { BuildingType.Shelter, "Shelter" }
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
                default: return Palette.Hex("C9A876");
            }
        }

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
            for (int y = 0; y < terrain.height; y++)
            {
                for (int x = 0; x < terrain.width; x++)
                {
                    var tile = UIKit.Rect(map, "Tile").gameObject.AddComponent<Image>();
                    tile.raycastTarget = false;
                    var rt = tile.rectTransform;
                    rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
                    rt.anchoredPosition = new Vector2(x * Tile + 1, y * Tile + 1);
                    rt.sizeDelta = new Vector2(Tile - 2, Tile - 2);
                    tiles[x, y] = tile;
                }
            }
            PaintTerrain();

            buildingLayer = UIKit.Rect(map, "Buildings");
            buildingLayer.Stretch();
            ghost = UIKit.Rect(map, "Ghost").gameObject.AddComponent<Image>();
            ghost.sprite = UIKit.PanelSprite;
            ghost.type = Image.Type.Sliced;
            ghost.raycastTarget = false;
            var grt = ghost.rectTransform;
            grt.anchorMin = grt.anchorMax = grt.pivot = Vector2.zero;
            ghost.gameObject.SetActive(false);

            var source = UIKit.Label(root, MapSource(), 18, Palette.Muted, TextAnchor.LowerRight);
            source.rectTransform.Anchor(new Vector2(1, 0), new Vector2(1, 0), new Vector2(-20, BottomBar + 10), new Vector2(Side, 50));
        }

        string MapSource()
        {
            if (run.Planet.id == "Mars" || run.mode == GameMode.Free) return "Made-up map. Brighter ground gets more sun.";
            return "Map: real sunlight at the Moon's south pole\n(NASA LRO/LOLA, simplified)";
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
            RefreshGhost();
        }

        BuildingView CreateView(PlacedBuilding b)
        {
            var def = run.sim.balance.Def(b.type);
            var body = UIKit.Panel(buildingLayer, BuildingColor(b.type), "Building " + b.type);
            body.raycastTarget = false;
            var rt = body.rectTransform;
            rt.anchorMin = rt.anchorMax = rt.pivot = Vector2.zero;
            rt.anchoredPosition = new Vector2(b.x * Tile + 3, b.y * Tile + 3);
            rt.sizeDelta = new Vector2(def.width * Tile - 6, def.height * Tile - 6);

            float iconSize = Mathf.Min(def.width, def.height) * Tile * 0.62f;
            var icon = UIKit.Icon(body.transform, b.type.ToString(), iconSize);
            icon.rectTransform.anchorMin = icon.rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            icon.rectTransform.anchoredPosition = new Vector2(0, def.width >= 2 && def.height >= 2 ? 8 : 0);
            string name = ShortNames[b.type];
            if (def.width >= 2 && def.height >= 2 && name.Length > 0)
            {
                var label = UIKit.Label(body.transform, name, 16, Palette.Ink, TextAnchor.LowerCenter, FontStyle.Bold);
                label.rectTransform.Stretch(2, 0, 2, 4);
            }

            var view = new BuildingView { body = body };
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
            view.body.color = dim ? Color.Lerp(BuildingColor(b.type), Palette.Space, 0.45f) : BuildingColor(b.type);
            view.badge.gameObject.SetActive(badge.Length > 0);
            view.badge.color = badgeColor;
            view.badgeText.text = badge;
            view.outline.enabled = b.id == run.SelectedId;
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
            ghost.rectTransform.anchoredPosition = new Vector2(hoverX * Tile + 2, hoverY * Tile + 2);
            ghost.rectTransform.sizeDelta = new Vector2(def.width * Tile - 4, def.height * Tile - 4);
            if (!check.Ok) { SetStatus(ShortReason(check), true); return; }
            string sun = def.generationKW > 0
                ? "  Sun here: " + Mathf.RoundToInt((float)run.sim.terrain.AverageSun(hoverX, hoverY, def.width, def.height) * 100) + "%"
                : "";
            SetStatus("Good spot." + sun);
        }

        static string ShortReason(PlacementResult r)
        {
            switch (r.reason)
            {
                case PlacementReason.OutOfBounds: return "That is off the map.";
                case PlacementReason.BlockedTerrain: return "Rocks or a crater are in the way.";
                case PlacementReason.Overlap: return "Something is already there.";
                default: return r.message;
            }
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
                if (result.Ok) SetStatus(run.sim.balance.Def(type).displayName + " built.");
                else SetStatus(ShortReason(result), true);
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
            selection.rectTransform.TopLeft(20, 1080 - BottomBar - 20 - 330, Side, 330);
            selectionIcon = UIKit.Icon(selection.transform, "Habitat", 56);
            selectionIcon.TopLeftOf(20, 20);
            selectionName = UIKit.Label(selection.transform, "", 30, Palette.Hull, TextAnchor.MiddleLeft, FontStyle.Bold);
            selectionName.rectTransform.TopLeft(90, 20, Side - 150, 56);
            var close = UIKit.Button(selection.transform, "X", () => run.Deselect(), Palette.PanelLight, Palette.Hull, 22);
            close.GetComponent<RectTransform>().TopLeft(Side - 56, 16, 40, 40);
            selectionInfo = UIKit.Label(selection.transform, "", 23, Palette.Ice, TextAnchor.UpperLeft);
            selectionInfo.rectTransform.TopLeft(20, 90, Side - 40, 110);
            selectionState = UIKit.Label(selection.transform, "", 24, Palette.Mint, TextAnchor.MiddleLeft, FontStyle.Bold);
            selectionState.rectTransform.TopLeft(20, 204, Side - 40, 34);

            toggleButton = UIKit.Button(selection.transform, "Turn off", () => run.Toggle(run.SelectedId), Palette.Azure, Palette.Hull, 22);
            toggleButton.GetComponent<RectTransform>().TopLeft(20, 256, 180, 54);
            fixButton = UIKit.Button(selection.transform, "Fix", () => run.Repair(run.SelectedId), Palette.Amber, Palette.Ink, 22);
            fixButton.GetComponent<RectTransform>().TopLeft(20, 256, 180, 54);
            removeButton = UIKit.Button(selection.transform, "Remove", () => run.Remove(run.SelectedId), Palette.PanelLight, Palette.Ice, 22);
            removeButton.GetComponent<RectTransform>().TopLeft(Side - 160, 256, 140, 54);
        }

        void RefreshSelection(StepReport power)
        {
            var b = run.Selected;
            selection.gameObject.SetActive(b != null);
            if (crewList != null) crewList.gameObject.SetActive(b == null);
            if (b == null) return;
            var def = run.sim.balance.Def(b.type);
            selectionIcon.sprite = Icons.Get(b.type.ToString());
            selectionName.text = def.displayName;

            var info = def.blurb;
            if (def.generationKW > 0) info += "\nMakes " + Num(def.generationKW * run.sim.SunFor(run.State, b)) + " kW here.";
            if (def.demandKW > 0) info += "\nUses " + Num(def.demandKW) + " kW.";
            if (def.batteryKWh > 0) info += "\nStores " + Num(def.batteryKWh) + " kWh.";
            selectionInfo.text = info;

            string state;
            Color color;
            if (b.broken) { state = "Broken"; color = Palette.Danger; }
            else if (!b.enabled) { state = "Off"; color = Palette.LunarGray; }
            else if (def.demandKW > 0 && !b.powered && run.State.step > 0) { state = "No power"; color = Palette.Amber; }
            else { state = "Working"; color = Palette.Mint; }
            selectionState.text = state;
            selectionState.color = color;

            bool canToggle = !run.IsDrawing && def.optional && !b.broken;
            toggleButton.gameObject.SetActive(canToggle);
            UIKit.LabelOf(toggleButton).text = b.enabled ? "Turn off" : "Turn on";
            fixButton.gameObject.SetActive(b.broken);
            UIKit.LabelOf(fixButton).text = "Fix (" + run.sim.balance.repairCost + ")";
            fixButton.interactable = run.State.supplies >= run.sim.balance.repairCost;
            removeButton.gameObject.SetActive(!run.Ended);
        }

        // ---------- bottom bar ----------

        sealed class BuildCard { public BuildingType? type; public Button button; public Text name, cost, count; public Image icon; public string lockedWhy; }
        readonly List<BuildCard> cards = new List<BuildCard>();
        Button nextDay;
        Text nextDayLabel;

        void BuildBottomBar()
        {
            var bar = UIKit.Panel(root, Palette.Ink, "Bottom Bar", false);
            bar.rectTransform.anchorMin = new Vector2(0, 0);
            bar.rectTransform.anchorMax = new Vector2(1, 0);
            bar.rectTransform.pivot = new Vector2(0.5f, 0);
            bar.rectTransform.anchoredPosition = Vector2.zero;
            bar.rectTransform.sizeDelta = new Vector2(0, BottomBar);

            statusText = UIKit.Label(root, "", 24, Palette.Ice, TextAnchor.MiddleCenter);
            statusText.rectTransform.Anchor(new Vector2(0.5f, 0), new Vector2(0.5f, 0), new Vector2(0, BottomBar + 6), new Vector2(1000, 34));
            statusText.rectTransform.pivot = new Vector2(0.5f, 0);

            int i = 0;
            foreach (var type in run.buildable) AddCard(bar.transform, i++, type, null);
            AddCard(bar.transform, i++, null, "Lab");
            AddCard(bar.transform, i, null, "Rover Garage");

            nextDay = UIKit.Button(bar.transform, "Next Day", PressNextDay, Palette.Azure, Palette.Hull, 34);
            nextDay.GetComponent<RectTransform>().Anchor(new Vector2(1, 0.5f), new Vector2(1, 0.5f), new Vector2(-20, 0), new Vector2(330, 96));
            nextDayLabel = UIKit.LabelOf(nextDay);
        }

        void AddCard(Transform bar, int index, BuildingType? type, string lockedName)
        {
            var card = new BuildCard { type = type };
            var button = UIKit.Button(bar, "", () => ClickCard(card), Palette.PanelLight);
            button.GetComponent<RectTransform>().TopLeft(20 + index * 150, 12, 140, 100);
            card.button = button;
            card.icon = UIKit.Icon(button.transform, type.HasValue ? type.Value.ToString() : "lock", 40);
            card.icon.TopLeftOf(10, 10);
            string name = type.HasValue ? run.sim.balance.Def(type.Value).displayName : lockedName;
            card.name = UIKit.Label(button.transform, name, 17, Palette.Hull, TextAnchor.UpperLeft, FontStyle.Bold);
            card.name.rectTransform.TopLeft(8, 56, 126, 36);
            card.name.resizeTextForBestFit = true;
            card.name.resizeTextMinSize = 11;
            card.name.resizeTextMaxSize = 17;
            card.name.horizontalOverflow = HorizontalWrapMode.Wrap;
            card.cost = UIKit.Label(button.transform, type.HasValue ? run.sim.balance.Def(type.Value).cost.ToString() : "Later", 18, Palette.Amber, TextAnchor.UpperRight, FontStyle.Bold);
            card.cost.rectTransform.TopLeft(60, 10, 70, 24);
            card.count = UIKit.Label(button.transform, "", 16, Palette.Muted, TextAnchor.UpperRight);
            card.count.rectTransform.TopLeft(60, 32, 70, 22);
            if (!type.HasValue) card.lockedWhy = lockedName + " is for a later mission. Power and people first.";
            cards.Add(card);
        }

        void ClickCard(BuildCard card)
        {
            if (ModalOpen || run.Ended) return;
            if (!card.type.HasValue) { SetStatus(card.lockedWhy, true); return; }
            var type = card.type.Value;
            if (run.BuildChoice == type) { CancelBuild(); return; }
            if (!run.CanBuild(type, out string why)) { SetStatus(run.sim.balance.Def(type).displayName + ": " + why, true); return; }
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
                card.cost.text = locked ? "Locked" : run.sim.balance.Def(type).cost.ToString();
            }

            if (run.IsDrawing)
            {
                nextDayLabel.text = "Finish";
                nextDay.image.color = run.DrawingReady ? Palette.Azure : Palette.PanelLight;
                return;
            }
            nextDayLabel.text = "Next " + run.Planet.dayWord;
            var f = run.Forecast;
            bool risky = f != null && (f.failed || (f.lastDay != null && (f.lastDay.brownouts > 0 || f.lastDay.airRanOut)));
            nextDay.image.color = risky ? Palette.Amber : Palette.Azure;
            nextDayLabel.color = risky ? Palette.Ink : Palette.Hull;
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
