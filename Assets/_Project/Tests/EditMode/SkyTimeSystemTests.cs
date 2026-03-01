using NUnit.Framework;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Tests.EditMode
{
    /// <summary>
    /// Tests for <see cref="SkyTimeSystem"/> pure-static helper methods
    /// that don't require a Unity scene.
    /// </summary>
    public class SkyTimeSystemTests
    {
        // ── GetTimePeriod ─────────────────────────────────────────────────────

        [Test]
        [TestCase(0,  SkyTimeSystem.TimePeriod.Night)]
        [TestCase(3,  SkyTimeSystem.TimePeriod.Night)]
        [TestCase(5,  SkyTimeSystem.TimePeriod.Dawn)]
        [TestCase(7,  SkyTimeSystem.TimePeriod.Dawn)]
        [TestCase(8,  SkyTimeSystem.TimePeriod.Morning)]
        [TestCase(11, SkyTimeSystem.TimePeriod.Morning)]
        [TestCase(12, SkyTimeSystem.TimePeriod.Afternoon)]
        [TestCase(17, SkyTimeSystem.TimePeriod.Afternoon)]
        [TestCase(18, SkyTimeSystem.TimePeriod.Dusk)]
        [TestCase(20, SkyTimeSystem.TimePeriod.Dusk)]
        [TestCase(21, SkyTimeSystem.TimePeriod.Night)]
        [TestCase(23, SkyTimeSystem.TimePeriod.Night)]
        public void GetTimePeriod_CorrectPeriodForHour(int hour, SkyTimeSystem.TimePeriod expected)
        {
            Assert.That(SkyTimeSystem.GetTimePeriod(hour), Is.EqualTo(expected));
        }

        // ── GetSeason (northern) ──────────────────────────────────────────────

        [Test]
        [TestCase(1,   SkyTimeSystem.SeasonType.Winter)]  // Jan
        [TestCase(45,  SkyTimeSystem.SeasonType.Winter)]  // mid-Feb
        [TestCase(80,  SkyTimeSystem.SeasonType.Spring)]  // late Mar
        [TestCase(150, SkyTimeSystem.SeasonType.Spring)]  // late May
        [TestCase(172, SkyTimeSystem.SeasonType.Summer)]  // mid-Jun
        [TestCase(213, SkyTimeSystem.SeasonType.Summer)]  // Aug
        [TestCase(266, SkyTimeSystem.SeasonType.Autumn)]  // late Sep
        [TestCase(330, SkyTimeSystem.SeasonType.Autumn)]  // late Nov
        [TestCase(355, SkyTimeSystem.SeasonType.Winter)]  // Dec
        public void GetSeason_NorthernHemisphere_CorrectSeason(int dayOfYear, SkyTimeSystem.SeasonType expected)
        {
            Assert.That(SkyTimeSystem.GetSeason(dayOfYear, true), Is.EqualTo(expected));
        }

        // ── Hemisphere flip ───────────────────────────────────────────────────

        [Test]
        public void GetSeason_SouthernHemisphere_OppositeSummer()
        {
            // Day 200 (mid-summer NH) should be winter SH
            var nh = SkyTimeSystem.GetSeason(200, true);
            var sh = SkyTimeSystem.GetSeason(200, false);
            Assert.That(nh, Is.EqualTo(SkyTimeSystem.SeasonType.Summer));
            Assert.That(sh, Is.EqualTo(SkyTimeSystem.SeasonType.Winter));
        }

        [Test]
        public void GetSeason_SouthernHemisphere_OppositeWinter()
        {
            // Day 15 (deep winter NH) should be summer SH
            var nh = SkyTimeSystem.GetSeason(15, true);
            var sh = SkyTimeSystem.GetSeason(15, false);
            Assert.That(nh, Is.EqualTo(SkyTimeSystem.SeasonType.Winter));
            Assert.That(sh, Is.EqualTo(SkyTimeSystem.SeasonType.Summer));
        }

        // ── Edge cases ────────────────────────────────────────────────────────

        [Test]
        public void GetTimePeriod_Boundary_Hour5_IsDawn()
        {
            Assert.That(SkyTimeSystem.GetTimePeriod(5), Is.EqualTo(SkyTimeSystem.TimePeriod.Dawn));
        }

        [Test]
        public void GetTimePeriod_Boundary_Hour21_IsNight()
        {
            Assert.That(SkyTimeSystem.GetTimePeriod(21), Is.EqualTo(SkyTimeSystem.TimePeriod.Night));
        }
    }
}
