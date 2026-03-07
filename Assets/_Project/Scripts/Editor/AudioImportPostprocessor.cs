#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Automatically applies correct import settings to any audio file dropped
    /// into the Assets/_Project/Audio/ folders.
    ///
    /// Music  → Streaming, Vorbis, 192 kbps
    /// SFX    → Decompress On Load, Vorbis, 128 kbps
    /// Ambient→ Streaming, Vorbis, 128 kbps
    ///
    /// Rules match the technical spec in docs/AUDIO_SPECIFICATION.md.
    /// </summary>
    public class AudioImportPostprocessor : AssetPostprocessor
    {
        private const string AUDIO_ROOT    = "Assets/_Project/Audio/";
        private const string MUSIC_PATH    = "Assets/_Project/Audio/Music/";
        private const string SFX_PATH      = "Assets/_Project/Audio/SFX/";
        private const string AMBIENT_PATH  = "Assets/_Project/Audio/Ambient/";

        void OnPreprocessAudio()
        {
            string path = assetPath;

            // Only process audio inside our Audio folder
            if (!path.StartsWith(AUDIO_ROOT, StringComparison.OrdinalIgnoreCase))
                return;

            var importer = (AudioImporter)assetImporter;

            // Shared base settings
            importer.forceToMono    = path.StartsWith(SFX_PATH, StringComparison.OrdinalIgnoreCase);
            importer.loadInBackground = true;
            importer.ambisonic      = false;

            AudioImporterSampleSettings settings = default;
            settings.compressionFormat = AudioCompressionFormat.Vorbis;
            settings.quality           = 1f; // 0-1 maps to kbps slider; 1 = ~192 kbps for stereo

            if (path.StartsWith(MUSIC_PATH, StringComparison.OrdinalIgnoreCase))
            {
                // ── Music: Streaming stereo, highest quality ──────────────
                settings.loadType = AudioClipLoadType.Streaming;
                settings.quality  = 1f;
                importer.forceToMono = false;
                Debug.Log($"[AudioImport] Music settings applied: {Path.GetFileName(path)}");
            }
            else if (path.StartsWith(SFX_PATH, StringComparison.OrdinalIgnoreCase))
            {
                // ── SFX: Decompress on load, mono, 128 kbps ───────────────
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.quality  = 0.7f; // ~128 kbps
                importer.forceToMono = true;
                Debug.Log($"[AudioImport] SFX settings applied: {Path.GetFileName(path)}");
            }
            else if (path.StartsWith(AMBIENT_PATH, StringComparison.OrdinalIgnoreCase))
            {
                // ── Ambient: Streaming stereo, 128 kbps ───────────────────
                settings.loadType = AudioClipLoadType.Streaming;
                settings.quality  = 0.7f;
                importer.forceToMono = false;
                Debug.Log($"[AudioImport] Ambient settings applied: {Path.GetFileName(path)}");
            }
            else
            {
                // Fallback for any other audio in the Audio/ root
                settings.loadType = AudioClipLoadType.DecompressOnLoad;
                settings.quality  = 0.7f;
            }

            importer.defaultSampleSettings = settings;

            // Apply the same settings to all platform overrides we care about
            foreach (string platform in new[] { "Android", "iOS", "WebGL" })
            {
                importer.SetOverrideSampleSettings(platform, settings);
            }
        }
    }
}
#endif
