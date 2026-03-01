using System;
using UnityEngine;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// The analytical result of a completed gesture drawing session.
    /// Produced by <see cref="SigilStrokeAnalyzer"/> and consumed by
    /// <see cref="SigilGenerator"/>, <see cref="SigilCompletionHandler"/>,
    /// and the audio / VFX pipelines.
    /// </summary>
    [Serializable]
    public struct SigilAnalysisResult
    {
        // ── Structural ────────────────────────────────────────────────────────
        /// <summary>Number of closed loops detected in the stroke.</summary>
        public int   closedLoopCount;

        /// <summary>Number of self-intersections detected.</summary>
        public int   intersectionCount;

        /// <summary>
        /// Curvature entropy [0,1]: 0 = perfectly straight, 1 = maximally chaotic.
        /// </summary>
        public float curvatureEntropy;

        /// <summary>Total arc length of all strokes in world units.</summary>
        public float totalArcLength;

        // ── Temporal ──────────────────────────────────────────────────────────
        /// <summary>Average drawing velocity (world units / second).</summary>
        public float averageVelocity;

        /// <summary>Peak velocity reached at any point in the gesture.</summary>
        public float peakVelocity;

        /// <summary>Total elapsed drawing time in seconds.</summary>
        public float drawingDuration;

        // ── Directional ───────────────────────────────────────────────────────
        /// <summary>
        /// Normalised dominant direction of the gesture.
        /// (0,1) = upward; (1,0) = rightward; etc.
        /// </summary>
        public Vector2 dominantDirection;

        /// <summary>Number of individual strokes (finger lifts).</summary>
        public int     strokeCount;

        // ── Computed helpers ──────────────────────────────────────────────────

        /// <summary>Complexity score [0,1] combining loops, intersections, and entropy.</summary>
        public float Complexity =>
            Mathf.Clamp01((closedLoopCount * 0.3f + intersectionCount * 0.15f + curvatureEntropy * 0.55f));

        /// <summary>True when the sigil has at least one closed loop and a meaningful arc length.</summary>
        public bool IsRich => closedLoopCount >= 1 && totalArcLength > 1.5f;

        /// <summary>Maps dominant direction to a cardinal label for audio/VFX routing.</summary>
        public string DominantDirectionLabel
        {
            get
            {
                float angle = Mathf.Atan2(dominantDirection.y, dominantDirection.x) * Mathf.Rad2Deg;
                if (angle > 45f && angle < 135f)  return "ascending";
                if (angle < -45f && angle > -135f) return "descending";
                if (Mathf.Abs(angle) < 45f)        return "expansive";
                return "contracting";
            }
        }
    }
}
