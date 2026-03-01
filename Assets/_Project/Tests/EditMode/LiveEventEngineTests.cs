using System;
using NUnit.Framework;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="LiveEventEngine.GetPhase"/> phase logic.
    /// Uses the public static overload \u2014 no MonoBehaviour required.
    /// </summary>
    public class LiveEventEngineTests
    {
        private const int DEFAULT_DURATION = 3; // days

        // ── Phase detection ────────────────────────────────────────────────

        [Test]
        public void GetPhase_DeltaZero_ReturnsPeak()
        {
            var phase = LiveEventEngine.GetPhase(TimeSpan.Zero, DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(EventPhase.Peak));
        }

        [Test]
        public void GetPhase_HalfDayBeforePeak_ReturnsPeak()
        {
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromHours(-12), DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(EventPhase.Peak));
        }

        [Test]
        public void GetPhase_TwoDaysBefore_ReturnsApproaching()
        {
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromDays(2), DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(EventPhase.Approaching));
        }

        [Test]
        public void GetPhase_ExactlyThreeDaysBefore_ReturnsApproaching()
        {
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromDays(3), DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(EventPhase.Approaching));
        }

        [Test]
        public void GetPhase_FourDaysBefore_ReturnsNone()
        {
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromDays(4), DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(EventPhase.None));
        }

        [Test]
        public void GetPhase_TwoDaysAfterPeak_ReturnsEcho()
        {
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromDays(-2), DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(EventPhase.Echo));
        }

        [Test]
        public void GetPhase_BeyondDuration_ReturnsNone()
        {
            // DEFAULT_DURATION = 3, so -4 days is beyond
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromDays(-4), DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(EventPhase.None));
        }

        [Test]
        public void GetPhase_LongerDurationExtendEcho()
        {
            int longDuration = 7;
            // -5 days post-peak should be Echo for a 7-day event
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromDays(-5), longDuration);
            Assert.That(phase, Is.EqualTo(EventPhase.Echo));
        }

        // ── Spark multiplier (via LiveEvent data shape) ────────────────────

        [Test]
        [TestCase(3.5,  EventPhase.None)]
        [TestCase(2.0,  EventPhase.Approaching)]
        [TestCase(0.0,  EventPhase.Peak)]
        [TestCase(-1.5, EventPhase.Echo)]
        [TestCase(-3.5, EventPhase.None)]
        public void GetPhase_CorrectPhaseForDelta(double deltaDays, EventPhase expected)
        {
            var phase = LiveEventEngine.GetPhase(TimeSpan.FromDays(deltaDays), DEFAULT_DURATION);
            Assert.That(phase, Is.EqualTo(expected),
                $"Delta {deltaDays:+0.0;-0.0} days should return {expected}");
        }
    }
}
