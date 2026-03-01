using NUnit.Framework;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="ContinuumFieldManager"/> offline energy calculation.
    /// Uses the public static helpers exposed for testability.
    /// </summary>
    public class ContinuumFieldManagerTests
    {
        // ── DeterministicPseudoCollective ─────────────────────────────────────

        [Test]
        public void PseudoCollective_SameInputs_ProduceSameOutput()
        {
            int a = ContinuumFieldManager.DeterministicPseudoCollective(100, 2025);
            int b = ContinuumFieldManager.DeterministicPseudoCollective(100, 2025);
            Assert.That(a, Is.EqualTo(b));
        }

        [Test]
        public void PseudoCollective_DifferentDays_DifferentOutputs()
        {
            int a = ContinuumFieldManager.DeterministicPseudoCollective(1,   2025);
            int b = ContinuumFieldManager.DeterministicPseudoCollective(180, 2025);
            Assert.That(a, Is.Not.EqualTo(b));
        }

        [Test]
        public void PseudoCollective_OutputIsInExpectedRange()
        {
            for (int day = 1; day <= 365; day++)
            {
                int val = ContinuumFieldManager.DeterministicPseudoCollective(day, 2025);
                Assert.That(val, Is.InRange(ContinuumFieldManager.PSEUDO_MIN,
                                            ContinuumFieldManager.PSEUDO_MAX),
                    $"Day {day} out of bounds: {val}");
            }
        }

        // ── ComputeCollectiveEnergy ────────────────────────────────────────────

        [Test]
        public void CollectiveEnergy_ZeroLocal_StillPositiveDueToPseudo()
        {
            float energy = ContinuumFieldManager.ComputeCollectiveEnergy(0, 100, 2025);
            Assert.That(energy, Is.GreaterThan(0f));
        }

        [Test]
        public void CollectiveEnergy_AlwaysInUnitRange()
        {
            for (int localCount = 0; localCount <= 50; localCount++)
            {
                float energy = ContinuumFieldManager.ComputeCollectiveEnergy(localCount, 180, 2025);
                Assert.That(energy, Is.InRange(0f, 1f),
                    $"Energy out of [0,1] for localCount={localCount}: {energy}");
            }
        }

        [Test]
        public void CollectiveEnergy_MoreLocalSigils_HigherEnergy()
        {
            float low  = ContinuumFieldManager.ComputeCollectiveEnergy(0,  100, 2025);
            float high = ContinuumFieldManager.ComputeCollectiveEnergy(50, 100, 2025);
            Assert.That(high, Is.GreaterThan(low));
        }

        // ── TimeZoneActivityPulse ─────────────────────────────────────────────

        [Test]
        public void TimeZoneActivityPulse_IsInUnitRange()
        {
            for (int hour = 0; hour < 24; hour++)
            {
                float pulse = ContinuumFieldManager.GetTimeZoneActivityPulse(hour);
                Assert.That(pulse, Is.InRange(0f, 1f),
                    $"Hour {hour}: pulse {pulse} out of range");
            }
        }

        [Test]
        public void TimeZoneActivityPulse_PeaksAround8PM()
        {
            // Hour 20 (8pm) should be near peak
            float peakHour = ContinuumFieldManager.GetTimeZoneActivityPulse(20);
            float lowHour  = ContinuumFieldManager.GetTimeZoneActivityPulse(4);
            Assert.That(peakHour, Is.GreaterThan(lowHour));
        }
    }
}
