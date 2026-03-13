using UnityEngine;
using UnityEngine.UI;

namespace AscendantContinuum.Realms.LanternAscension
{
    /// <summary>
    /// Wires the "Confirm Wish" button to LanternRitual.ConfirmWish() at runtime.
    /// Added automatically by RealmPrefabBuilder.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class WishInputConfirmButton : MonoBehaviour
    {
        [SerializeField] internal LanternRitual ritual;

        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(() => ritual?.ConfirmWish());
        }
    }
}
