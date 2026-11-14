using System.Text;
using Mapmakers.Data;
using Mapmakers.Game;
using Mapmakers.Sim;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.UI;

namespace Mapmakers.UI
{
    // Functional greybox only. Opus can replace this view without changing simulation commands.
    public sealed class MissionView : MonoBehaviour
    {
        MissionController game;
        RectTransform root;
        Font font;
        Text hud, notices, info, banner, debrief;
        Button toggle, recall;
        Image[,] tiles;
        Text[,] labels;
        int hoverX = -1, hoverY = -1, revision = -1;
        long step = -1;
        readonly Color ground = new Color(.32f,.34f,.38f), blocked = new Color(.12f,.13f,.16f);
        public Button[,] TileButtons { get; private set; }
        public Button RecallButton => recall;
        public Button RetryButton { get; private set; }

        void Start()
        {
            game = GetComponent<MissionController>();
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            var canvasObject = new GameObject("Mission Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvasObject.transform.SetParent(transform, false);
            root = canvasObject.GetComponent<RectTransform>();
            canvasObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scale = canvasObject.GetComponent<CanvasScaler>(); scale.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize; scale.referenceResolution = new Vector2(1920,1080);
            if (FindAnyObjectByType<EventSystem>() == null)
            {
                var events = new GameObject("Mission Input", typeof(EventSystem), typeof(InputSystemUIInputModule));
                events.transform.SetParent(transform, false);
                events.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
            }
            hud = Label("Resources", 30, 12, 1850, 105, 25);
            banner = Label("Mission events", 30, 120, 1850, 44, 22);
            int w = game.Session.Simulator.terrain.width, h = game.Session.Simulator.terrain.height;
            tiles = new Image[w,h]; labels = new Text[w,h]; TileButtons = new Button[w,h];
            for (int y=0;y<h;y++) for (int x=0;x<w;x++)
            {
                int tx=x,ty=y;
                var button = MakeButton("Tile " + x + "," + y, 30+x*50, 180+(h-1-y)*50, 48,48, () => game.ClickTile(tx,ty));
                tiles[x,y]=button.GetComponent<Image>(); labels[x,y]=button.GetComponentInChildren<Text>(); TileButtons[x,y]=button;
                labels[x,y].fontSize=13;
                var hover=button.gameObject.AddComponent<TileHover>(); hover.View=this; hover.X=x; hover.Y=y;
            }
            int row=0;
            foreach(BuildingType type in System.Enum.GetValues(typeof(BuildingType)))
            {
                var choice=type; var def=game.Session.Simulator.balance.Def(type);
                MakeButton(def.displayName+" ("+def.cost+")",1070,180+row++*49,390,44,()=>game.ChooseBuilding(choice));
            }
            MakeButton("Inspect / cancel placement",1070,530,390,44,()=>game.ChooseBuilding(null));
            Label("Locked: Lab, Rover Garage\nLater missions",1070,580,390,55,21);
            info=Label("Building details",1480,180,410,250,22);
            toggle=MakeButton("Switch selected system",1480,440,410,48,()=>game.Toggle(game.SelectedId));
            recall=MakeButton("Call Yuki inside",1480,500,410,48,()=>game.Recall());
            notices=Label("Warnings",1070,650,820,260,21);
            int i=0; foreach(int speed in new[]{0,1,2,4}) { int value=speed; MakeButton(speed==0?"Pause":speed+"x",30+i++*130,950,120,48,()=>game.SetSpeed(value)); }
            RetryButton=MakeButton("Reset / Retry",560,950,220,48,()=>game.ResetMission());
            Label("Choose a building to place. Inspect to switch optional systems.\nTiles: # blocked, . sunlit. Building labels include on/off status.",30,1008,1860,62,20);
            debrief=Label("Debrief",1070,925,820,90,22);
            Refresh();
        }
        void Update() { if(game!=null && (step!=game.Session.State.step || revision!=game.Revision)) Refresh(); }
        public void Hover(int x,int y) { hoverX=x;hoverY=y;Refresh(); }
        public void Leave() { hoverX=-1;hoverY=-1;Refresh(); }
        void Refresh()
        {
            if(tiles==null)return;
            var session=game.Session;var s=session.State;var sim=session.Simulator;var r=s.report;
            step=s.step;revision=game.Revision;
            hud.text="Power "+N(r.generationKW)+" / "+N(r.demandKW)+" kW    Battery "+N(s.batteryKWh)+" / "+N(sim.BatteryCapacity(s))+" kWh ("+N(r.batteryFlowKW)+" kW)\n"
                +"Air "+N(s.airHours)+" h ("+N(r.airPerHour)+"/h)    Water "+N(s.waterL)+" L ("+N(r.waterPerHour)+"/h)    Food "+N(s.meals)+" ("+N(r.foodPerHour)+"/h)\n"
                +"Safety "+N(s.safety)+"/100 ("+N(r.safetyPerHour)+"/h)    Supplies "+s.supplies+"    "+SimText.Clock(s.timeHours,sim.balance.startClockHour)+"   Speed "+session.Speed+"x";
            banner.text=s.stage+" | "+s.stormPhase+" | "+(s.events.Count>0?s.events[s.events.Count-1].text:game.Message);
            for(int y=0;y<sim.terrain.height;y++)for(int x=0;x<sim.terrain.width;x++)
            {
                var b=Placement.BuildingAt(sim.balance,s,x,y);
                tiles[x,y].color=b!=null?(b.powered?new Color(.25f,.5f,.65f):new Color(.5f,.35f,.2f)):(sim.terrain.IsBlocked(x,y)?blocked:ground* (float)(.7+sim.terrain.Sun(x,y)*.3));
                labels[x,y].text=b==null?(sim.terrain.IsBlocked(x,y)?"#":"."):(b.x==x&&b.y==y?Short(b.type)+"\n"+(b.type==BuildingType.Battery?"SAVE":b.enabled?(b.powered?"ON":"WAIT"):"OFF"):"+");
            }
            string preview="";
            if(game.BuildChoice.HasValue&&hoverX>=0)
            {
                var d=sim.balance.Def(game.BuildChoice.Value);var check=sim.CanPlace(s,game.BuildChoice.Value,hoverX,hoverY);
                preview=check.Ok?"Valid placement": "X: "+check.message;
                for(int dy=0;dy<d.height;dy++)for(int dx=0;dx<d.width;dx++) if(sim.terrain.InBounds(hoverX+dx,hoverY+dy))
                { tiles[hoverX+dx,hoverY+dy].color=check.Ok?new Color(.2f,.65f,.75f):new Color(.85f,.4f,.1f); labels[hoverX+dx,hoverY+dy].text=check.Ok?"+":"X"; }
            }
            var selected=s.Find(game.SelectedId);toggle.interactable=selected!=null&&sim.balance.Def(selected.type).optional&&!session.Finished;
            if(selected==null)info.text="Select Inspect, then click a building.";
            else { var d=sim.balance.Def(selected.type);info.text=d.displayName+"\n"+d.blurb+"\nMakes "+N(d.generationKW)+" kW\nUses "+N(d.demandKW)+" kW\n"+(selected.type==BuildingType.Battery?"Storage: shared battery reserve":selected.enabled?(selected.powered?"Powered":"Waiting for power"):"Switched off"); }
            recall.interactable=!session.Finished&&sim.CanRecall(s,out _);
            var warnings=new StringBuilder(game.Message+"\n"+preview+"\n");foreach(var warning in r.warnings)warnings.AppendLine(warning.text);
            if(s.stormPhase!=StormPhase.None) { var data=FixtureLoader.Donki();warnings.AppendLine("Real data: DONKI "+data.flare.classType+"\n"+data.flare.beginTime+" | Crew: "+s.geologist); }
            notices.text=warnings.ToString();
            if(session.Finished) { var result=game.Result;var story=StoryData.Load();debrief.text=story.Debrief(result.kind==OutcomeKind.Win?"title_win":result.kind==OutcomeKind.CloseCall?"title_close":"title_fail")+"  Stars: "+result.stars+"\n"+(s.failed?story.Debrief("fail_"+s.failReason):story.Debrief("sci_warning"))+"\nReset / Retry to start again."; }
            else debrief.text="Game values are fictional balance.\nNASA/CSA references; no endorsement.";
        }
        static string N(double value)=>SimText.Num(value);
        static string Short(BuildingType t) { switch(t){case BuildingType.Habitat:return "HAB";case BuildingType.SolarArray:return "SUN";case BuildingType.Battery:return "BAT";case BuildingType.LifeSupport:return "AIR";case BuildingType.Communications:return "RAD";case BuildingType.Greenhouse:return "FOOD";default:return "SAFE";} }
        RectTransform Rect(string name,float x,float y,float w,float h)
        {var go=new GameObject(name,typeof(RectTransform));var rt=go.GetComponent<RectTransform>();rt.SetParent(root,false);rt.anchorMin=rt.anchorMax=rt.pivot=new Vector2(0,1);rt.anchoredPosition=new Vector2(x,-y);rt.sizeDelta=new Vector2(w,h);return rt;}
        Text Label(string name,float x,float y,float w,float h,int size)
        {var t=Rect(name,x,y,w,h).gameObject.AddComponent<Text>();t.font=font;t.fontSize=size;t.color=Color.white;t.text=name;t.raycastTarget=false;return t;}
        Button MakeButton(string title,float x,float y,float w,float h,UnityEngine.Events.UnityAction action)
        {var rt=Rect(title,x,y,w,h);var image=rt.gameObject.AddComponent<Image>();image.color=new Color(.22f,.26f,.32f);var b=rt.gameObject.AddComponent<Button>();b.targetGraphic=image;b.onClick.AddListener(action);var text=Label(title,x+3,y+3,w-6,h-6,22);text.transform.SetParent(rt,true);text.rectTransform.anchorMin=text.rectTransform.anchorMax=new Vector2(0,1);text.rectTransform.anchoredPosition=new Vector2(3,-3);text.alignment=TextAnchor.MiddleCenter;return b;}
    }
    public sealed class TileHover:MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {public MissionView View;public int X,Y;public void OnPointerEnter(PointerEventData e)=>View.Hover(X,Y);public void OnPointerExit(PointerEventData e)=>View.Leave();}
}
