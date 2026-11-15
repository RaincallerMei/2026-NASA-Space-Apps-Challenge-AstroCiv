// Put this file in Assets/AstroCiv/Editor before copying the PNG folders.
// Art-only import helper. No runtime dependencies or gameplay changes.
using UnityEngine;
using UnityEditor;
public sealed class AstroCivTextureImporter : AssetPostprocessor
{
    void OnPreprocessTexture()
    {
        if (!assetPath.StartsWith("Assets/AstroCiv/") || !assetPath.EndsWith(".png")) return;
        var t = (TextureImporter)assetImporter;
        t.filterMode = FilterMode.Point;
        t.textureCompression = TextureImporterCompression.Uncompressed;
        t.mipmapEnabled = false;
        t.alphaIsTransparency = true;
        t.npotScale = TextureImporterNPOTScale.None;
        t.maxTextureSize = 2048;
        bool repeating = assetPath.Contains("ground_repeat");
        bool sheet = assetPath.Contains("/Sheets/") || assetPath.Contains("/Font/");
        t.wrapMode = repeating ? TextureWrapMode.Repeat : TextureWrapMode.Clamp;
        t.textureType = repeating || sheet ? TextureImporterType.Default : TextureImporterType.Sprite;
        if (repeating || sheet) return;
        t.spriteImportMode = SpriteImportMode.Single;
        t.spritePixelsPerUnit = 64;
        t.GetSourceTextureWidthAndHeight(out int w, out int h);
        var settings = new TextureImporterSettings();
        t.ReadTextureSettings(settings);
        settings.spriteAlignment = (int)SpriteAlignment.Custom;
        settings.spriteMeshType = SpriteMeshType.FullRect;
        settings.spritePivot = new Vector2(.5f, 8f / h);
        if (assetPath.Contains("/Corridors/")) settings.spritePivot = new Vector2(.5f, 24f / h);
        if (assetPath.Contains("/Characters/Frames/")) settings.spritePivot = new Vector2(.5f, 4f / h);
        if (assetPath.Contains("/UI/") || assetPath.EndsWith("portrait.png") || assetPath.Contains("ground_diamond")) settings.spritePivot = new Vector2(.5f, .5f);
        if (assetPath.Contains("/UI/Panels/")) settings.spriteBorder = new Vector4(8, 8, 8, 8);
        t.SetTextureSettings(settings);
    }
}
