using NUnit.Framework;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="SerendipityTierCalculator.CalculateTier"/>.
    /// Pure static \u2014 no MonoBehaviour or scene required.
    /// </summary>
    public class SerendipityManagerTests
    {
        // Default probability config matching SerendipityManager inspector defaults
        private const float L = 0.01f;   // legendary
        private const float E = 0.04f;   // epic
        private const float R = 0.10f;   // rare
        private const float U = 0.25f;   // uncommon

        // ── Boundary tier tests ────────────────────────────────────────────

        [Test]
        public void CalculateTier_RollZero_ReturnsLegendary()
        {
            var tier = SerendipityTierCalculator.CalculateTier(0f, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Legendary));
        }

        [Test]
        public void CalculateTier_RollJustBelowLegendaryThreshold_ReturnsLegendary()
        {
            var tier = SerendipityTierCalculator.CalculateTier(L - 0.001f, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Legendary));
        }

        [Test]
        public void CalculateTier_RollAtLegendaryThreshold_ReturnsEpic()
        {
            var tier = SerendipityTierCalculator.CalculateTier(L, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Epic));
        }

        [Test]
        public void CalculateTier_RollInEpicRange_ReturnsEpic()
        {
            float midEpic = L + E * 0.5f;
            var tier = SerendipityTierCalculator.CalculateTier(midEpic, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Epic));
        }

        [Test]
        public void CalculateTier_RollInRareRange_ReturnsRare()
        {
            float midRare = L + E + R * 0.5f;
            var tier = SerendipityTierCalculator.CalculateTier(midRare, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Rare));
        }

        [Test]
        public void CalculateTier_RollInUncommonRange_ReturnsUncommon()
        {
            float midUncommon = L + E + R + U * 0.5f;
            var tier = SerendipityTierCalculator.CalculateTier(midUncommon, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Uncommon));
        }

        [Test]
        public void CalculateTier_RollOne_ReturnsCommon()
        {
            var tier = SerendipityTierCalculator.CalculateTier(1f, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Common));
        }

        [Test]
        public void CalculateTier_SumOfAllThresholds_ReturnsCommon()
        {
            // At exactly L+E+R+U the roll is into the common zone
            float commonStart = L + E + R + U;
            var tier = SerendipityTierCalculator.CalculateTier(commonStart, L, E, R, U);
            Assert.That(tier, Is.EqualTo(SerendipityTier.Common));
        }
    }
}
