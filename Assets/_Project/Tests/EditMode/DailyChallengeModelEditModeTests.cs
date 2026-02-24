using AscendantContinuum.Systems;
using NUnit.Framework;

namespace AscendantContinuum.Tests.EditMode
{
    public class DailyChallengeModelEditModeTests
    {
        [Test]
        public void GetProgress_ReturnsZero_WhenTargetIsZero()
        {
            var challenge = new DailyChallenge
            {
                targetValue = 0,
                currentProgress = 5
            };

            Assert.AreEqual(0f, challenge.GetProgress());
        }

        [Test]
        public void GetProgress_ClampsToOne_WhenCurrentProgressExceedsTarget()
        {
            var challenge = new DailyChallenge
            {
                targetValue = 10,
                currentProgress = 25
            };

            Assert.AreEqual(1f, challenge.GetProgress());
        }
    }
}
