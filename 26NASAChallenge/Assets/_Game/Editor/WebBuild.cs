using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Mapmakers.EditorTools
{
    public static class WebBuild
    {
        const string OutputFolder = "../docs";

        [MenuItem("Mapmakers/Build WebGL (docs/)")]
        public static void Build()
        {
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Disabled;
            PlayerSettings.WebGL.decompressionFallback = false;
            PlayerSettings.WebGL.nameFilesAsHashes = true;

            var options = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray(),
                locationPathName = OutputFolder,
                target = BuildTarget.WebGL,
                options = BuildOptions.None,
            };

            var summary = BuildPipeline.BuildPlayer(options).summary;
            if (summary.result == BuildResult.Succeeded)
                Debug.Log($"WebGL build finished: {OutputFolder}/ ({summary.totalSize / 1048576f:F1} MB)");
            else
                Debug.LogError($"WebGL build {summary.result} with {summary.totalErrors} error(s)");
        }
    }
}
