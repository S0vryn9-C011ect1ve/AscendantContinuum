using System.Collections.Generic;
using UnityEngine;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Analyses a completed set of stroke points to produce a <see cref="SigilAnalysisResult"/>.
    ///
    /// All maths are pure (no MonoBehaviour dependency), so this class is directly
    /// unit-testable without a Unity runtime.
    /// </summary>
    public static class SigilStrokeAnalyzer
    {
        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Analyse a list of strokes and return a <see cref="SigilAnalysisResult"/>.
        /// Each stroke is a <c>List&lt;Vector2&gt;</c> of screen / world positions.
        /// </summary>
        public static SigilAnalysisResult Analyze(
            List<List<Vector2>> strokes,
            float totalDrawingDurationSeconds)
        {
            if (strokes == null || strokes.Count == 0)
                return default;

            // Flatten for global metrics
            var allPoints = new List<Vector2>();
            foreach (var stroke in strokes)
                allPoints.AddRange(stroke);

            float arcLength        = ComputeTotalArcLength(allPoints);
            float avgVel           = arcLength > 0f ? arcLength / Mathf.Max(0.01f, totalDrawingDurationSeconds) : 0f;
            float peakVel          = ComputePeakVelocity(allPoints, totalDrawingDurationSeconds);
            Vector2 domDir         = ComputeDominantDirection(allPoints);
            float entropy          = ComputeCurvatureEntropy(allPoints);
            int loops              = CountClosedLoops(strokes);
            int intersections      = CountIntersections(strokes);

            return new SigilAnalysisResult
            {
                closedLoopCount      = loops,
                intersectionCount    = intersections,
                curvatureEntropy     = entropy,
                totalArcLength       = arcLength,
                averageVelocity      = avgVel,
                peakVelocity         = peakVel,
                drawingDuration      = totalDrawingDurationSeconds,
                dominantDirection    = domDir,
                strokeCount          = strokes.Count
            };
        }

        // ── Private: geometry helpers ─────────────────────────────────────────

        private static float ComputeTotalArcLength(List<Vector2> points)
        {
            float len = 0f;
            for (int i = 1; i < points.Count; i++)
                len += Vector2.Distance(points[i - 1], points[i]);
            return len;
        }

        private static float ComputePeakVelocity(List<Vector2> points, float duration)
        {
            if (points.Count < 2 || duration <= 0f) return 0f;

            // Approximate: assume uniform time distribution across all points
            float dt = duration / Mathf.Max(1, points.Count - 1);
            float peak = 0f;
            for (int i = 1; i < points.Count; i++)
            {
                float v = Vector2.Distance(points[i - 1], points[i]) / dt;
                if (v > peak) peak = v;
            }
            return peak;
        }

        private static Vector2 ComputeDominantDirection(List<Vector2> points)
        {
            if (points.Count < 2) return Vector2.up;

            Vector2 sum = Vector2.zero;
            for (int i = 1; i < points.Count; i++)
                sum += (points[i] - points[i - 1]);

            return sum.magnitude > 0.001f ? sum.normalized : Vector2.up;
        }

        /// <summary>
        /// Curvature entropy: measure direction-change variance across the stroke.
        /// Returns [0,1] — 0 = straight line, 1 = maximum curvature chaos.
        /// </summary>
        private static float ComputeCurvatureEntropy(List<Vector2> points)
        {
            if (points.Count < 3) return 0f;

            const int bins = 8;
            int[] histogram = new int[bins];

            for (int i = 1; i < points.Count - 1; i++)
            {
                Vector2 a = (points[i]     - points[i - 1]).normalized;
                Vector2 b = (points[i + 1] - points[i]    ).normalized;
                float angle = Vector2.SignedAngle(a, b); // -180 to 180
                int bin = Mathf.Clamp(Mathf.FloorToInt((angle + 180f) / 360f * bins), 0, bins - 1);
                histogram[bin]++;
            }

            // Normalise to probabilities and compute Shannon entropy / max-entropy
            float total = points.Count - 2f;
            float entropy = 0f;
            float maxEntropy = Mathf.Log(bins);
            foreach (int h in histogram)
            {
                if (h <= 0) continue;
                float p = h / total;
                entropy -= p * Mathf.Log(p);
            }
            return maxEntropy > 0f ? Mathf.Clamp01(entropy / maxEntropy) : 0f;
        }

        /// <summary>
        /// Counts closed loops: a stroke is considered closed when its endpoints
        /// are within 15% of its bounding-box diagonal.
        /// </summary>
        private static int CountClosedLoops(List<List<Vector2>> strokes)
        {
            int count = 0;
            foreach (var stroke in strokes)
            {
                if (stroke.Count < 8) continue;

                Vector2 start = stroke[0];
                Vector2 end   = stroke[stroke.Count - 1];

                // Bounding-box diagonal
                float minX = float.MaxValue, maxX = float.MinValue;
                float minY = float.MaxValue, maxY = float.MinValue;
                foreach (var p in stroke)
                {
                    if (p.x < minX) minX = p.x;
                    if (p.x > maxX) maxX = p.x;
                    if (p.y < minY) minY = p.y;
                    if (p.y > maxY) maxY = p.y;
                }
                float diag = Vector2.Distance(new Vector2(minX, minY), new Vector2(maxX, maxY));
                float closure = Vector2.Distance(start, end);
                if (diag > 0.01f && closure / diag < 0.15f) count++;
            }
            return count;
        }

        /// <summary>
        /// Counts approximate line-segment intersections across all stroke pairs.
        /// Uses the classic CCW / cross-product intersection test.
        /// Capped at 20 to prevent O(n²) blow-up on very long strokes.
        /// </summary>
        private static int CountIntersections(List<List<Vector2>> strokes)
        {
            int count = 0;
            const int sampleStep = 4; // sample every 4th segment
            const int cap = 20;

            for (int si = 0; si < strokes.Count && count < cap; si++)
            {
                var s1 = strokes[si];
                for (int sj = 0; sj < strokes.Count && count < cap; sj++)
                {
                    var s2 = strokes[sj];
                    bool samestroke = si == sj;

                    for (int i = 0; i < s1.Count - 1 && count < cap; i += sampleStep)
                    for (int j = 0; j < s2.Count - 1 && count < cap; j += sampleStep)
                    {
                        if (samestroke && Mathf.Abs(i - j) < 4) continue; // skip adjacent
                        if (SegmentsIntersect(s1[i], s1[i + 1], s2[j], s2[j + 1]))
                            count++;
                    }
                }
            }
            return count / 2; // each intersection counted twice
        }

        private static bool SegmentsIntersect(Vector2 p1, Vector2 p2, Vector2 p3, Vector2 p4)
        {
            float d1 = Cross(p3, p4, p1);
            float d2 = Cross(p3, p4, p2);
            float d3 = Cross(p1, p2, p3);
            float d4 = Cross(p1, p2, p4);

            if (((d1 > 0 && d2 < 0) || (d1 < 0 && d2 > 0)) &&
                ((d3 > 0 && d4 < 0) || (d3 < 0 && d4 > 0)))
                return true;

            return false;
        }

        private static float Cross(Vector2 a, Vector2 b, Vector2 c)
        {
            return (b.x - a.x) * (c.y - a.y) - (b.y - a.y) * (c.x - a.x);
        }
    }
}
