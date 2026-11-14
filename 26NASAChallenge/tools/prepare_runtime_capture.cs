using UnityEngine;
using Mapmakers.Game;
using Mapmakers.Sim;
public static class PrepareRuntimeCapture
{
    public static string Main()
    {
        var game=Object.FindAnyObjectByType<MissionController>();
        if(game==null)throw new System.InvalidOperationException("Open Main in Play Mode first.");
        game.AutoTick=false;game.ResetMission();
        new ShedLoadsBot().Act(game.Session.Simulator,game.Session.State);
        for(int i=0;i<1000&&game.Session.State.stormPhase==StormPhase.None;i++)game.Advance(.25);
        game.ChooseBuilding(null);game.SetSpeed(0);
        return "Prepared existing bot base at storm warning, paused for a reproducible screenshot.";
    }
}
