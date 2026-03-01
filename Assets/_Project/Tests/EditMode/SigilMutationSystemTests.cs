using NUnit.Framework;
using UnityEngine;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="SigilMutationSystem.CalculateMutationStage"/> logic.
    /// Uses the public static helper exposed for testability.
    /// </summary>
    public class SigilMutationSystemTests
    {
        // ── Stage thresholds ─────────────────────────────────────────────────
        // Stage 0: < 7 days
        // Stage 1: >= 7 days
        // Stage 2: >= 30 days
        // Stage 3: celestial event only (tested separately)

        [Test]
        [TestCase(0,  0)]
        [TestCase(6,  0)]
        [TestCase(7,  1)]
        [TestCase(29, 1)]
        [TestCase(30, 2)]
        [TestCase(99, 2)]
        [TestCase(365, 2)]
        public void CalculateMutationStage_TimeBased_CorrectStage(int daysOld, int expectedStage)
        {
            int stage = SigilMutationSystem.CalculateMutationStage(daysOld, false);
            Assert.That(stage, Is.EqualTo(expectedStage));
        }

        [Test]
        public void CalculateMutationStage_CelestialEvent_ReturnsStage3()
        {
            // Even 0-day sigil should reach stage 3 during celestial event
            int stage = SigilMutationSystem.CalculateMutationStage(0, true);
            Assert.That(stage, Is.EqualTo(3));
        }

        [Test]
        public void CalculateMutationStage_OldSigilDuringCelestial_ReturnsStage3()
        {
            int stage = SigilMutationSystem.CalculateMutationStage(100, true);
            Assert.That(stage, Is.EqualTo(3));
        }

        // ── SigilMutationProperties ───────────────────────────────────────────

        [Test]
        public void MutationProperties_Stage0_NoPulseNoShimmer()
        {
            var props = SigilMutationSystem.BuildProperties(0);
            Assert.That(props.PulseSpeed,    Is.EqualTo(0f));
            Assert.That(props.ShimmerActive, Is.False);
        }

        [Test]
        public void MutationProperties_Stage1_HasPulse()
        {
            var props = SigilMutationSystem.BuildProperties(1);
            Assert.That(props.PulseSpeed, Is.GreaterThan(0f));
        }

        [Test]
        public void MutationProperties_Stage2_HasColorBlend()
        {
            var props = SigilMutationSystem.BuildProperties(2);
            Assert.That(props.SecondaryColorBlend, Is.GreaterThan(0f));
        }

        [Test]
        public void MutationProperties_Stage3_ShimmerActive()
        {
            var props = SigilMutationSystem.BuildProperties(3);
            Assert.That(props.ShimmerActive, Is.True);
        }
    }
}
