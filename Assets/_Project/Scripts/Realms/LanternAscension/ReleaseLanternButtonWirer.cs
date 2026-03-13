using UnityEngine;
using UnityEngine.UI;

namespace AscendantContinuum.Realms.LanternAscension
{
    /// <summary>
    /// Wires the "Release Lantern" HUD button to LanternRitual.BeginLanternCreation().
    /// Added automatically by RealmPrefabBuilder.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class ReleaseLanternButtonWirer : MonoBehaviour
    {
        [SerializeField] internal LanternRitual ritual;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => ritual?.BeginLanternCreation());
        }
    }
}
