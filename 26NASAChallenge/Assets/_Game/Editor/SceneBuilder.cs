using System.Collections.Generic;
using Mapmakers.Game;
using Mapmakers.UI;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Mapmakers.EditorTools
{
    // Creates the five game scenes and the build order: Boot, Menu, Cutscene, Game, Results.
    // Each scene holds a camera and one screen component that builds its UI in code.
    public static class SceneBuilder
    {
        const string Folder = "Assets/_Game/Scenes/";

        [MenuItem("Mapmakers/Build Scenes")]
        public static void BuildAll()
        {
            var scenes = new List<EditorBuildSettingsScene>
            {
                Make("Boot", typeof(CampaignBoot)),
                Make(Flow.MenuScene, typeof(MenuScreen)),
                Make(Flow.CutsceneScene, typeof(CutsceneScreen)),
                Make(Flow.GameScene, typeof(GameScreen)),
                Make(Flow.ResultsScene, typeof(ResultsScreen))
            };
            EditorBuildSettings.scenes = scenes.ToArray();
            EditorSceneManager.OpenScene(Folder + "Boot.unity");
            Debug.Log("Mapmakers: built " + scenes.Count + " scenes and set the build order.");
        }

        static EditorBuildSettingsScene Make(string name, System.Type screen)
        {
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var camera = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener)).GetComponent<Camera>();
            camera.tag = "MainCamera";
            camera.orthographic = true;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.063f, 0.094f, 0.176f);
            camera.transform.position = new Vector3(0, 0, -10);
            new GameObject(name + " Screen", screen);
            string path = Folder + name + ".unity";
            EditorSceneManager.SaveScene(scene, path);
            return new EditorBuildSettingsScene(path, true);
        }
    }

    // Story pictures: sprites, no mipmaps, crunch-compressed so the web build stays small.
    public sealed class CutsceneImport : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if (!assetPath.Replace('\\', '/').Contains("/Art/Resources/Cutscenes/")) return;
            var importer = (TextureImporter)assetImporter;
            importer.textureType = TextureImporterType.Sprite;
            importer.spriteImportMode = SpriteImportMode.Single;
            importer.mipmapEnabled = false;
            importer.filterMode = FilterMode.Bilinear;
            importer.maxTextureSize = 2048;
            importer.textureCompression = TextureImporterCompression.Compressed;
            importer.crunchedCompression = true;
            importer.compressionQuality = 60;
        }
    }
}
