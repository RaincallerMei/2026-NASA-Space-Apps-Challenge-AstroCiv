using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using Mapmakers.Game;
using Mapmakers.UI;
using System.Linq;
public static class CreateMain
{
    public static string Main()
    {
        const string path="Assets/_Game/Scenes/Main.unity";
        if(System.IO.File.Exists(path))return "Main already exists; preserved.";
        var previous=SceneManager.GetActiveScene();
        if(previous.IsValid() && string.IsNullOrEmpty(previous.path))
        {
            var snapshot=AssetDatabase.GenerateUniqueAssetPath("Assets/_Game/Scenes/WorkspaceSnapshot.unity");
            if(!EditorSceneManager.SaveScene(previous,snapshot))throw new System.InvalidOperationException("Could not preserve the untitled workspace scene.");
        }
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        SceneManager.SetActiveScene(scene);
        var camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener));camera.tag="MainCamera";camera.GetComponent<Camera>().orthographic=true;camera.transform.position=new Vector3(0,0,-10);
        var root=new GameObject("Mission");root.AddComponent<MissionController>();root.AddComponent<MissionView>();
        EditorSceneManager.SaveScene(scene,path);
        EditorSceneManager.CloseScene(scene,true);
        if(previous.IsValid())SceneManager.SetActiveScene(previous);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(path,true)}.Concat(EditorBuildSettings.scenes.Where(s=>s.path!=path)).ToArray();
        return "Created Main scene; existing scenes preserved.";
    }
}
