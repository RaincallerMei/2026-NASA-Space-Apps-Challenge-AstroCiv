using UnityEditor;
using UnityEngine;

namespace Mapmakers.EditorTools
{
    // AstroCiv pack settings: point filter, no mipmaps, no compression, centred sprites. Ground textures tile.
    public sealed class PixelArtImporter : AssetPostprocessor
    {
        const string Root = "Assets/_Game/Art/Resources/AstroCiv/";

        void OnPreprocessTexture()
        {
            if (!assetPath.StartsWith(Root) || !assetPath.EndsWith(".png")) return;
            var t = (TextureImporter)assetImporter;
            bool repeating = assetPath.Contains("ground_repeat");
            t.filterMode = FilterMode.Point;
            t.textureCompression = TextureImporterCompression.Uncompressed;
            t.mipmapEnabled = false;
            t.alphaIsTransparency = true;
            t.npotScale = TextureImporterNPOTScale.None;
            t.wrapMode = repeating ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
            t.textureType = repeating ? TextureImporterType.Default : TextureImporterType.Sprite;
            if (repeating) return;
            t.spriteImportMode = SpriteImportMode.Single;
            t.spritePixelsPerUnit = 64;
            var settings = new TextureImporterSettings();
            t.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteAlignment = (int)SpriteAlignment.Center;
            t.SetTextureSettings(settings);
        }
    }
}
