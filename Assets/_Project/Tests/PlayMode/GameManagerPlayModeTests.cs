using System.Collections;
using AscendantContinuum.Core;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace AscendantContinuum.Tests.PlayMode
{
    public class GameManagerPlayModeTests
    {
        [UnityTest]
        public IEnumerator Awake_TransitionsToMainMenuState()
        {
            var gameObject = new GameObject("GameManager_Test");
            var manager = gameObject.AddComponent<GameManager>();

            yield return null;

            Assert.AreEqual(GameState.MainMenu, manager.CurrentState);

            Object.Destroy(gameObject);
        }
    }
}
