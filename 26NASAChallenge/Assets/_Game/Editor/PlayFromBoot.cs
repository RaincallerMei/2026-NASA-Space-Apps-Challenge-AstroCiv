using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mapmakers.EditorTools
{
    // Pressing Play on an empty or unlisted scene starts the game from Boot instead of a blank screen,
    // and Play mode keeps running while the editor window is not focused.
    [InitializeOnLoad]
    static class PlayFromBoot
    {
        const string Boot = "Assets/_Game/Scenes/Boot.unity";

        static PlayFromBoot() => EditorApplication.playModeStateChanged += OnChange;

        static void OnChange(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.ExitingEditMode)
                EditorSceneManager.playModeStartScene = InGameScene() ? null : AssetDatabase.LoadAssetAtPath<SceneAsset>(Boot);
            else if (change == PlayModeStateChange.EnteredPlayMode)
                Application.runInBackground = true;
            else if (change == PlayModeStateChange.EnteredEditMode)
                EditorSceneManager.playModeStartScene = null;
        }

        static bool InGameScene()
        {
            var scene = EditorSceneManager.GetActiveScene();
            if (scene.GetRootGameObjects().Any(go => go.GetComponent("PlaymodeTestsController") != null)) return true;
            return EditorBuildSettings.scenes.Any(s => s.enabled && s.path == scene.path);
        }
    }
}
