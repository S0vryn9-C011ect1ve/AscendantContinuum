using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Pure math tests for <see cref="SigilStrokeAnalyzer"/>.
    /// No scene, no MonoBehaviour — fully deterministic.
    /// </summary>
    public class SigilStrokeAnalyzerTests
    {
        // ── Helpers ──────────────────────────────────────────────────────────

        private static List<Vector2> Line(Vector2 from, Vector2 to, int pts = 10)
        {
            var stroke = new List<Vector2>(pts);
            for (int i = 0; i < pts; i++)
                stroke.Add(Vector2.Lerp(from, to, i / (float)(pts - 1)));
            return stroke;
        }

        private static List<Vector2> Circle(Vector2 centre, float r, int pts = 40)
        {
            var stroke = new List<Vector2>(pts);
            for (int i = 0; i < pts; i++)
            {
                float a = i / (float)(pts - 1) * Mathf.PI * 2f;
                stroke.Add(centre + new Vector2(Mathf.Cos(a), Mathf.Sin(a)) * r);
            }
            return stroke;
        }

        // ── Arc length ────────────────────────────────────────────────────────

        [Test]
        public void ArcLength_StraightLine_EqualsDistanceBetweenEndpoints()
        {
            var strokes = new List<List<Vector2>> { Line(Vector2.zero, new Vector2(3f, 4f)) };
            var result  = SigilStrokeAnalyzer.Analyze(strokes, 1f);
            Assert.That(result.totalArcLength, Is.EqualTo(5f).Within(0.05f));
        }

        [Test]
        public void ArcLength_SinglePoint_IsZero()
        {
            var stroke  = new List<Vector2> { Vector2.zero };
            var strokes = new List<List<Vector2>> { stroke };
            var result  = SigilStrokeAnalyzer.Analyze(strokes, 0.1f);
            Assert.That(result.totalArcLength, Is.EqualTo(0f).Within(0.001f));
        }

        // ── Closed loop ───────────────────────────────────────────────────────

        [Test]
        public void ClosedLoop_PerfectCircle_DetectedAtLeastOnce()
        {
            var strokes = new List<List<Vector2>> { Circle(Vector2.zero, 1f) };
            var result  = SigilStrokeAnalyzer.Analyze(strokes, 2f);
            Assert.That(result.closedLoopCount, Is.GreaterThanOrEqualTo(1));
        }

        [Test]
        public void ClosedLoop_StraightLine_NotDetected()
        {
            var strokes = new List<List<Vector2>> { Line(Vector2.zero, new Vector2(5f, 0f)) };
            var result  = SigilStrokeAnalyzer.Analyze(strokes, 1f);
            Assert.That(result.closedLoopCount, Is.EqualTo(0));
        }

        // ── Velocity ──────────────────────────────────────────────────────────

        [Test]
        public void PeakVelocity_IsGreaterThanOrEqualToAverage()
        {
            var strokes = new List<List<Vector2>> { Line(Vector2.zero, new Vector2(5f, 5f)) };
            var result  = SigilStrokeAnalyzer.Analyze(strokes, 1f);
            Assert.That(result.peakVelocity, Is.GreaterThanOrEqualTo(result.averageVelocity));
        }

        [Test]
        public void AverageVelocity_PositiveForNonTrivialStroke()
        {
            var strokes = new List<List<Vector2>> { Line(Vector2.zero, new Vector2(10f, 0f)) };
            var result  = SigilStrokeAnalyzer.Analyze(strokes, 1f);
            Assert.That(result.averageVelocity, Is.GreaterThan(0f));
        }

        // ── Complexity ────────────────────────────────────────────────────────

        [Test]
        public void Complexity_IsInUnitRange()
        {
            var strokes = new List<List<Vector2>>
            {
                Circle(Vector2.zero, 1f),
                Circle(new Vector2(0.5f, 0f), 0.8f),
                Line(new Vector2(-1f, -1f), new Vector2(1f, 1f))
            };
            var result = SigilStrokeAnalyzer.Analyze(strokes, 3f);
            Assert.That(result.Complexity, Is.InRange(0f, 1f));
        }

        [Test]
        public void Complexity_MultipleCircles_HigherThanSingleLine()
        {
            var lineStrokes = new List<List<Vector2>> { Line(Vector2.zero, new Vector2(5f, 0f)) };
            var richStrokes = new List<List<Vector2>>
            {
                Circle(Vector2.zero, 1f),
                Circle(new Vector2(1f, 0f), 1f),
                Line(Vector2.zero, new Vector2(-2f, -2f))
            };

            var lineResult = SigilStrokeAnalyzer.Analyze(lineStrokes, 1f);
            var richResult = SigilStrokeAnalyzer.Analyze(richStrokes, 3f);

            Assert.That(richResult.Complexity, Is.GreaterThan(lineResult.Complexity));
        }

        // ── Stroke count ──────────────────────────────────────────────────────

        [Test]
        public void StrokeCount_MatchesInput()
        {
            var strokes = new List<List<Vector2>>
            {
                Line(Vector2.zero, Vector2.one),
                Line(Vector2.right, Vector2.up),
                Circle(Vector2.zero, 1f)
            };
            var result = SigilStrokeAnalyzer.Analyze(strokes, 2f);
            Assert.That(result.strokeCount, Is.EqualTo(3));
        }

        // ── Dominant direction ────────────────────────────────────────────────

        [Test]
        public void DominantDirection_StraightRightLine_IsZeroRadians()
        {
            var strokes = new List<List<Vector2>> { Line(Vector2.zero, new Vector2(10f, 0f)) };
            var result  = SigilStrokeAnalyzer.Analyze(strokes, 1f);
            // Right direction = 0 radians, allow small tolerance
            float diff  = Mathf.Abs(Mathf.DeltaAngle(result.dominantDirection, 0f));
            Assert.That(diff, Is.LessThan(15f));
        }

        // ── Null / edge cases ─────────────────────────────────────────────────

        [Test]
        public void EmptyStrokeList_ReturnsZeroComplexity()
        {
            var result = SigilStrokeAnalyzer.Analyze(new List<List<Vector2>>(), 0f);
            Assert.That(result.Complexity, Is.EqualTo(0f).Within(0.001f));
        }

        [Test]
        public void NullStrokeList_ReturnsDefaultResult()
        {
            Assert.DoesNotThrow(() => SigilStrokeAnalyzer.Analyze(null, 0f));
        }
    }
}
