using UnityEngine;
using Mapmakers.Game;
using Mapmakers.Sim;
using Mapmakers.UI;
public static class PrepareTutorialCapture
{
    public static string Main()
    {
        var game=Object.FindAnyObjectByType<MissionController>();
        game.AutoTick=false;game.ResetMission();
        for(int i=0;i<6;i++)game.ContinueStory();
        if(!game.Place(BuildingType.Habitat,0,0).Ok)throw new System.Exception("Tutorial setup failed.");
        Object.FindAnyObjectByType<MissionView>().Hover(4,5);
        return "Tutorial: Habitat drawn; player is learning Solar Array placement.";
    }
}
