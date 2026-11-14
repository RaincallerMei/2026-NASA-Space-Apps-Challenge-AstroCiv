using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Mapmakers.Game;
using Mapmakers.UI;

public static class CreateCampaign
{
    public static string Main()
    {
        var previous=SceneManager.GetActiveScene();
        foreach(var name in new[]{"Boot","Prologue"})
        {
            string path="Assets/_Game/Scenes/"+name+".unity";
            if(System.IO.File.Exists(path))continue;
            var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
            SceneManager.SetActiveScene(scene);
            if(name=="Boot")new GameObject("Campaign Boot").AddComponent<CampaignBoot>();
            else
            {
                var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.transform.position=new Vector3(0,0,-10);camera.GetComponent<Camera>().orthographic=true;
                var root=new GameObject("Tutorial");root.AddComponent<MissionController>().StartWithTutorial=true;root.AddComponent<MissionView>();
            }
            EditorSceneManager.SaveScene(scene,path);EditorSceneManager.CloseScene(scene,true);
        }
        if(previous.IsValid())SceneManager.SetActiveScene(previous);
        string[] paths={"Assets/_Game/Scenes/Boot.unity","Assets/_Game/Scenes/Prologue.unity","Assets/_Game/Scenes/Main.unity"};
        EditorBuildSettings.scenes=paths.Select(p=>new EditorBuildSettingsScene(p,true)).Concat(EditorBuildSettings.scenes.Where(s=>!paths.Contains(s.path))).ToArray();
        AssetDatabase.SaveAssets();
        return "Boot -> Prologue -> Main registered; existing scenes preserved.";
    }
}
