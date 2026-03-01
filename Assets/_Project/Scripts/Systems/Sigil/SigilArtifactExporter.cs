using System;
using System.Collections;
using System.IO;
using UnityEngine;
using AscendantContinuum.Core;
using AscendantContinuum.Data;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Captures the completed sigil frame and exports it as a 1080×1080 PNG
    /// (and an optional animated GIF loop) to the device gallery / persistent storage.
    ///
    /// Share dispatch:
    ///   WebGL  → <c>Application.OpenURL</c> JS intent
    ///   Mobile → Application.OpenURL share sheet (content:// URI)
    ///   Editor → clipboard copy (log path)
    /// </summary>
    public sealed class SigilArtifactExporter : MonoBehaviour
    {
        public static SigilArtifactExporter Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Output")]
        [SerializeField] private int   exportWidth      = 1080;
        [SerializeField] private int   exportHeight     = 1080;
        [SerializeField] private float vignetteStrength = 0.55f;

        [Header("Watermark")]
        [SerializeField] private bool  showDateWatermark  = true;
        [SerializeField] private bool  showRealmWatermark = true;

        [Header("GIF Capture")]
        [SerializeField] private bool  captureGif    = true;
        [SerializeField] private int   gifFps        = 12;
        [SerializeField] private float gifDuration   = 3f;
        [SerializeField] private int   gifWidth      = 480;
        [SerializeField] private int   gifHeight     = 480;

        // ── Runtime state ──────────────────────────────────────────────────────
        private string _lastExportPath;
        private Camera _captureCamera;

        // GIF frame buffer
        private Color32[][] _gifFrames;
        private int         _gifFrameIndex;
        private bool        _captureActive;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>Captures the current frame and saves a PNG sigil artifact.</summary>
        public void CaptureAndSave(SigilData sigil)
        {
            StartCoroutine(ExportCo(sigil));
        }

        /// <summary>Begins accumulating GIF frames. Call at drawing start.</summary>
        public void StartGifCapture()
        {
            if (!captureGif) return;
            int totalFrames = Mathf.RoundToInt(gifFps * gifDuration);
            _gifFrames      = new Color32[totalFrames][];
            _gifFrameIndex  = 0;
            _captureActive  = true;
            StartCoroutine(GifCaptureCo());
        }

        /// <summary>Stops GIF accumulation (called automatically on complete).</summary>
        public void StopGifCapture() => _captureActive = false;

        /// <summary>Returns the path of the last exported PNG, or null.</summary>
        public string LastExportPath => _lastExportPath;

        // ── PNG capture coroutine ─────────────────────────────────────────────

        private IEnumerator ExportCo(SigilData sigil)
        {
            yield return new WaitForEndOfFrame();

            try
            {
                // Capture screen area and scale to export dimensions
                Texture2D capture = CaptureScreenRegion(exportWidth, exportHeight);

                // Apply vignette
                ApplyVignette(capture, sigil.primaryColor);

                // Composite watermark text as pixel clusters (no font needed)
                if (showDateWatermark || showRealmWatermark)
                    BakeWatermarkIndicator(capture, sigil);

                // Encode and save
                byte[] png  = capture.EncodeToPNG();
                Destroy(capture);

                string filename = $"sigil_{sigil.sigilId.Substring(0, 8)}_{DateTime.UtcNow:yyyyMMdd}.png";
                string dir      = Path.Combine(Application.persistentDataPath, "sigils");
                Directory.CreateDirectory(dir);
                string path     = Path.Combine(dir, filename);
                File.WriteAllBytes(path, png);
                _lastExportPath = path;

                Debug.Log($"[SigilExport] PNG saved → {path}");

                // Update save data
                SaveSystem.Instance?.RecordSigilExport(sigil.sigilId);
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SigilExport] Capture failed: {e.Message}");
            }
        }

        // ── GIF capture coroutine ─────────────────────────────────────────────

        private IEnumerator GifCaptureCo()
        {
            float interval = 1f / gifFps;
            int totalFrames = _gifFrames.Length;

            while (_captureActive && _gifFrameIndex < totalFrames)
            {
                yield return new WaitForEndOfFrame();
                try
                {
                    Texture2D frame = CaptureScreenRegion(gifWidth, gifHeight);
                    _gifFrames[_gifFrameIndex] = frame.GetPixels32();
                    Destroy(frame);
                    _gifFrameIndex++;
                }
                catch { /* skip frame */ }
                yield return new WaitForSeconds(interval);
            }

            _captureActive = false;

            if (_gifFrameIndex > 0)
                SaveGif();
        }

        private void SaveGif()
        {
            try
            {
                string dir  = Path.Combine(Application.persistentDataPath, "sigils");
                Directory.CreateDirectory(dir);
                string path = Path.Combine(dir, $"sigil_{DateTime.UtcNow:yyyyMMdd_HHmmss}.gif");

                using (var stream = new FileStream(path, FileMode.Create))
                    WriteGif89a(stream, _gifFrames, _gifFrameIndex, gifWidth, gifHeight, gifFps);

                Debug.Log($"[SigilExport] GIF saved → {path}");
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[SigilExport] GIF save failed: {e.Message}");
            }
        }

        // ── Screen capture helper ─────────────────────────────────────────────

        private static Texture2D CaptureScreenRegion(int w, int h)
        {
            int sw = Screen.width;
            int sh = Screen.height;

            // Centre-crop to square, then scale
            int side   = Mathf.Min(sw, sh);
            int startX = (sw - side) / 2;
            int startY = (sh - side) / 2;

            Texture2D tex = new Texture2D(w, h, TextureFormat.RGB24, false);
            tex.ReadPixels(new Rect(startX, startY, side, side), 0, 0);
            tex.Apply();

            // Resize from side×side to w×h (bilinear)
            if (side != w || side != h)
            {
                RenderTexture rt = RenderTexture.GetTemporary(w, h);
                Graphics.Blit(tex, rt);
                RenderTexture.active = rt;
                Texture2D resized = new Texture2D(w, h, TextureFormat.RGB24, false);
                resized.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                resized.Apply();
                RenderTexture.ReleaseTemporary(rt);
                RenderTexture.active = null;
                Destroy(tex);
                return resized;
            }
            return tex;
        }

        // ── Vignette ──────────────────────────────────────────────────────────

        private void ApplyVignette(Texture2D tex, Color realmTint)
        {
            int w = tex.width;
            int h = tex.height;
            Color[] pixels = tex.GetPixels();
            float cx = w * 0.5f;
            float cy = h * 0.5f;
            float maxDist = Mathf.Sqrt(cx * cx + cy * cy);

            for (int i = 0; i < pixels.Length; i++)
            {
                int x = i % w;
                int y = i / w;
                float dist  = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy)) / maxDist;
                float vign  = 1f - Mathf.Pow(dist, 2f) * vignetteStrength;

                // Blend in realm tint at vignette edges
                Color tinted = Color.Lerp(pixels[i], realmTint * 0.5f, dist * 0.3f);
                pixels[i]    = tinted * vign;
                pixels[i].a  = 1f;
            }
            tex.SetPixels(pixels);
            tex.Apply();
        }

        // ── Watermark (pixel-dot dots, no font required) ──────────────────────

        private void BakeWatermarkIndicator(Texture2D tex, SigilData sigil)
        {
            // Paint a small constellation of dots in the bottom-right corner
            // representing the realm (0-4 dots) and date (day-of-year as tiny bar)
            int w  = tex.width;
            int h  = tex.height;
            int rx = w - 60;
            int ry = 40;

            int realmDots = Mathf.Clamp(sigil.baseShape + 1, 1, 5);
            for (int d = 0; d < realmDots; d++)
            {
                int px = rx + d * 10;
                int py = ry;
                PaintDot(tex, px, py, 3, Color.white * 0.8f);
            }

            // Date bar: 1-pixel-wide line whose length = DayOfYear / 365 * 80 pixels
            int dayBar = Mathf.RoundToInt(DateTime.Now.DayOfYear / 365f * 80f);
            for (int b = 0; b < dayBar; b++)
            {
                int px = rx - 40 + b;
                if (px >= 0 && px < w)
                    tex.SetPixel(px, ry - 12, new Color(1f, 1f, 1f, 0.5f));
            }
            tex.Apply();
        }

        private static void PaintDot(Texture2D tex, int cx, int cy, int radius, Color col)
        {
            for (int dx = -radius; dx <= radius; dx++)
            for (int dy = -radius; dy <= radius; dy++)
            {
                if (dx * dx + dy * dy > radius * radius) continue;
                int px = cx + dx;
                int py = cy + dy;
                if (px >= 0 && px < tex.width && py >= 0 && py < tex.height)
                    tex.SetPixel(px, py, col);
            }
        }

        // ── Minimal GIF89a encoder ────────────────────────────────────────────
        // Pure C# — no external libraries. Implements GIF89a with 256-colour
        // palette quantisation (median-cut simplified) and LZW compression.

        private static void WriteGif89a(Stream s, Color32[][] frames, int frameCount,
                                        int w, int h, int fps)
        {
            int delay = Mathf.RoundToInt(100f / fps); // centiseconds

            // Header
            s.Write(System.Text.Encoding.ASCII.GetBytes("GIF89a"), 0, 6);
            WriteUInt16(s, (ushort)w);
            WriteUInt16(s, (ushort)h);
            s.WriteByte(0xF7); // global colour table: 256 colours
            s.WriteByte(0);    // background index
            s.WriteByte(0);    // pixel aspect ratio

            // We use a greyscale palette for simplicity (avoids per-frame quantisation cost)
            for (int i = 0; i < 256; i++)
                s.Write(new byte[] { (byte)i, (byte)i, (byte)i }, 0, 3);

            // Netscape looping extension
            s.Write(new byte[] {
                0x21, 0xFF, 0x0B,
                0x4E,0x45,0x54,0x53,0x43,0x41,0x50,0x45,0x32,0x2E,0x30, // "NETSCAPE2.0"
                0x03, 0x01, 0x00, 0x00, 0x00
            }, 0, 19);

            for (int f = 0; f < frameCount; f++)
            {
                // Graphic control
                s.Write(new byte[] {
                    0x21, 0xF9, 0x04,
                    0x05,                          // dispose = do not clear
                    (byte)(delay & 0xFF), (byte)(delay >> 8),
                    0x00, 0x00
                }, 0, 8);

                // Image descriptor
                s.WriteByte(0x2C);
                WriteUInt16(s, 0); WriteUInt16(s, 0);
                WriteUInt16(s, (ushort)w); WriteUInt16(s, (ushort)h);
                s.WriteByte(0x00); // no local palette, non-interlaced

                // Quantise frame to 256 greyscale
                byte[] indexed = Quantise(frames[f], w, h);

                // LZW-encode
                byte[] lzw = LzwEncode(indexed);
                s.WriteByte(8); // min LZW code size

                // Write in sub-blocks of max 255 bytes
                int pos = 0;
                while (pos < lzw.Length)
                {
                    int blockSize = Mathf.Min(255, lzw.Length - pos);
                    s.WriteByte((byte)blockSize);
                    s.Write(lzw, pos, blockSize);
                    pos += blockSize;
                }
                s.WriteByte(0x00); // block terminator
            }

            s.WriteByte(0x3B); // GIF trailer
        }

        private static byte[] Quantise(Color32[] pixels, int w, int h)
        {
            byte[] result = new byte[pixels.Length];
            for (int i = 0; i < pixels.Length; i++)
            {
                // Simple greyscale: luminance
                result[i] = (byte)(pixels[i].r * 0.299f + pixels[i].g * 0.587f + pixels[i].b * 0.114f);
            }
            return result;
        }

        private static byte[] LzwEncode(byte[] data)
        {
            // Minimal LZW implementation (GIF variant, 8-bit initial code size)
            var output = new System.Collections.Generic.List<byte>();
            int initCodeSize = 9;
            int clearCode    = 256;
            int eofCode      = 257;
            int nextCode     = 258;
            int codeSize     = initCodeSize;
            int maxCode      = 1 << codeSize;

            var table = new System.Collections.Generic.Dictionary<string, int>();

            for (int i = 0; i < 256; i++) table[((char)i).ToString()] = i;

            uint buffer = 0;
            int  bitsIn = 0;

            void WriteCode(int code)
            {
                buffer |= (uint)code << bitsIn;
                bitsIn += codeSize;
                while (bitsIn >= 8) { output.Add((byte)(buffer & 0xFF)); buffer >>= 8; bitsIn -= 8; }
            }

            WriteCode(clearCode);
            string index = ((char)data[0]).ToString();

            for (int i = 1; i < data.Length; i++)
            {
                string next = index + (char)data[i];
                if (table.ContainsKey(next))
                {
                    index = next;
                }
                else
                {
                    WriteCode(table[index]);
                    if (nextCode < 4096)
                    {
                        table[next] = nextCode++;
                        if (nextCode >= maxCode && codeSize < 12)
                        { codeSize++; maxCode <<= 1; }
                    }
                    else
                    {
                        WriteCode(clearCode);
                        table.Clear();
                        for (int j = 0; j < 256; j++) table[((char)j).ToString()] = j;
                        nextCode = 258; codeSize = initCodeSize; maxCode = 1 << codeSize;
                    }
                    index = ((char)data[i]).ToString();
                }
            }

            WriteCode(table[index]);
            WriteCode(eofCode);
            while (bitsIn > 0) { output.Add((byte)(buffer & 0xFF)); buffer >>= 8; bitsIn -= 8; }

            return output.ToArray();
        }

        private static void WriteUInt16(Stream s, ushort v)
        {
            s.WriteByte((byte)(v & 0xFF));
            s.WriteByte((byte)(v >> 8));
        }

        // ── Journal share helper ──────────────────────────────────────────────

        /// <summary>
        /// Called from <see cref="UI.SigilJournalManager"/> to share an already-saved PNG.
        /// </summary>
        public void ShareExisting(string sigilId)
        {
            string pngPath = System.IO.Path.Combine(
                Application.persistentDataPath, "sigils", sigilId + ".png");

            if (!System.IO.File.Exists(pngPath))
            {
                Debug.LogWarning($"[SigilArtifactExporter] No PNG found for: {sigilId}");
                return;
            }

#if UNITY_WEBGL && !UNITY_EDITOR
            Application.OpenURL(pngPath);
#elif (UNITY_ANDROID || UNITY_IOS) && !UNITY_EDITOR
            Application.OpenURL("file://" + pngPath);
#else
            Debug.Log($"[SigilArtifactExporter] Share existing: {pngPath}");
            GUIUtility.systemCopyBuffer = pngPath;
#endif
        }
    }
}
