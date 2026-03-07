#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Automatically applies correct import settings to any image dropped into
    /// the Assets/_Project/Art/ folder hierarchy.
    ///
    /// Backgrounds → Sprite (2D and UI), 1024 max, no mip maps, Bilinear
    /// Gameplay     → Sprite (2D and UI), 512 max, no mip maps, Point (pixel-perfect)
    /// UI           → Sprite (2D and UI), 512 max, no mip maps, Bilinear
    /// Particles    → Default (Texture), 64 max, no mip maps, Bilinear, Alpha Is Transparency
    ///
    /// Settings follow the recommendations in docs/AUDIO_SPECIFICATION.md and
    /// the mobile performance target (60 fps, URP, IL2CPP).
    /// </summary>
    public class SpriteImportPostprocessor : AssetPostprocessor
    {
        private const string ART_ROOT        = "Assets/_Project/Art/";
        private const string BG_PATH         = "Assets/_Project/Art/Backgrounds/";
        private const string PARTICLES_PATH  = "Assets/_Project/Art/Particles/";
        private const string UI_PATH         = "Assets/_Project/Art/Sprites/UI/";

        void OnPreprocessTexture()
        {
            string path = assetPath;
            if (!path.StartsWith(ART_ROOT, StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (TextureImporter)assetImporter;
            importer.mipmapEnabled = false;
            importer.wrapMode = TextureWrapMode.Clamp;
            importer.alphaIsTransparency = true;

            if (path.StartsWith(BG_PATH, StringComparison.OrdinalIgnoreCase))
            {
                // Full-screen realm backgrounds — high res sprite
                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Single;
                importer.filterMode          = FilterMode.Bilinear;
                importer.maxTextureSize      = 2048;
                importer.textureCompression  = TextureImporterCompression.CompressedLQ;
                ApplyPlatformOverride(importer, "Android",  2048, TextureImporterFormat.ETC2_RGBA8);
                ApplyPlatformOverride(importer, "iOS",      2048, TextureImporterFormat.ASTC_6x6);
                ApplyPlatformOverride(importer, "WebGL",    2048, TextureImporterFormat.DXT5);
                Debug.Log($"[SpriteImport] Background settings: {Path.GetFileName(path)}");
            }
            else if (path.StartsWith(PARTICLES_PATH, StringComparison.OrdinalIgnoreCase))
            {
                // Particle textures — small, clamped, no sprite mode
                importer.textureType        = TextureImporterType.Default;
                importer.filterMode         = FilterMode.Bilinear;
                importer.maxTextureSize     = 128;
                importer.textureCompression = TextureImporterCompression.CompressedLQ;
                ApplyPlatformOverride(importer, "Android",  128, TextureImporterFormat.ETC2_RGBA8);
                ApplyPlatformOverride(importer, "iOS",      128, TextureImporterFormat.ASTC_8x8);
                ApplyPlatformOverride(importer, "WebGL",    128, TextureImporterFormat.DXT5);
                Debug.Log($"[SpriteImport] Particle texture settings: {Path.GetFileName(path)}");
            }
            else if (path.StartsWith(UI_PATH, StringComparison.OrdinalIgnoreCase))
            {
                // UI icons / HUD elements
                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Single;
                importer.filterMode          = FilterMode.Bilinear;
                importer.maxTextureSize      = 512;
                importer.textureCompression  = TextureImporterCompression.CompressedLQ;
                ApplyPlatformOverride(importer, "Android",  512, TextureImporterFormat.ETC2_RGBA8);
                ApplyPlatformOverride(importer, "iOS",      512, TextureImporterFormat.ASTC_6x6);
                ApplyPlatformOverride(importer, "WebGL",    512, TextureImporterFormat.DXT5);
                Debug.Log($"[SpriteImport] UI sprite settings: {Path.GetFileName(path)}");
            }
            else
            {
                // Gameplay sprites (realm characters, sparks, plants, etc.)
                importer.textureType         = TextureImporterType.Sprite;
                importer.spriteImportMode    = SpriteImportMode.Single;
                importer.filterMode          = FilterMode.Bilinear;
                importer.maxTextureSize      = 512;
                importer.textureCompression  = TextureImporterCompression.CompressedLQ;
                ApplyPlatformOverride(importer, "Android",  512, TextureImporterFormat.ETC2_RGBA8);
                ApplyPlatformOverride(importer, "iOS",      512, TextureImporterFormat.ASTC_6x6);
                ApplyPlatformOverride(importer, "WebGL",    512, TextureImporterFormat.DXT5);
                Debug.Log($"[SpriteImport] Gameplay sprite settings: {Path.GetFileName(path)}");
            }
        }

        private static void ApplyPlatformOverride(TextureImporter importer,
            string platform, int maxSize, TextureImporterFormat format)
        {
            var settings = new TextureImporterPlatformSettings
            {
                name            = platform,
                overridden      = true,
                maxTextureSize  = maxSize,
                format          = format,
                compressionQuality = 50,
                allowsAlphaSplitting = false,
            };
            importer.SetPlatformTextureSettings(settings);
        }
    }
}
#endif
