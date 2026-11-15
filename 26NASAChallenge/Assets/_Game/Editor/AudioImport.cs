using UnityEditor;
using UnityEngine;

namespace Mapmakers.EditorTools
{
    // Music: compressed, loaded on first play so the web build starts quickly. Effects: short mono clips, ready at once.
    public sealed class AudioImport : AssetPostprocessor
    {
        const string Root = "Assets/_Game/Audio/Resources/Audio/";

        void OnPreprocessAudio()
        {
            string path = assetPath.Replace('\\', '/');
            if (!path.StartsWith(Root)) return;
            var importer = (AudioImporter)assetImporter;
            var settings = importer.defaultSampleSettings;
            bool music = path.StartsWith(Root + "Music/");
            importer.forceToMono = !music;
            importer.loadInBackground = music;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality = music ? 0.4f : 0.7f;
            settings.loadType = music ? AudioClipLoadType.CompressedInMemory : AudioClipLoadType.DecompressOnLoad;
            settings.preloadAudioData = !music;
            importer.defaultSampleSettings = settings;
        }
    }
}
