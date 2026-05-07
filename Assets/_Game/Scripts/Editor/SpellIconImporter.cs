#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

/// <summary>
/// Post-processeur d'import Unity.
/// Détecte automatiquement les PNG placés dans Assets/Resources/SpellIcons/
/// et les configure comme Sprite (2D and UI), lecture activée, mipmaps désactivés.
/// S'exécute à chaque import de fichier (y compris les ajouts en masse).
/// </summary>
public class SpellIconImporter : AssetPostprocessor
{
    const string TargetFolder = "Assets/Resources/SpellIcons";

    void OnPreprocessTexture()
    {
        if (!assetPath.Replace('\\', '/').StartsWith(TargetFolder, System.StringComparison.OrdinalIgnoreCase))
            return;

        var importer = assetImporter as TextureImporter;
        if (importer == null) return;

        importer.textureType        = TextureImporterType.Sprite;
        importer.spriteImportMode   = SpriteImportMode.Single;
        importer.spritePivot        = new Vector2(0.5f, 0.5f);
        importer.spritePixelsPerUnit = 100f;
        importer.mipmapEnabled      = false;
        importer.isReadable         = false;
        importer.filterMode         = FilterMode.Point;   // pixel art net
        importer.textureCompression = TextureImporterCompression.Uncompressed;
        importer.maxTextureSize     = 128;

        var settings = importer.GetDefaultPlatformTextureSettings();
        settings.format = TextureImporterFormat.RGBA32;
        importer.SetPlatformTextureSettings(settings);
    }
}
#endif
