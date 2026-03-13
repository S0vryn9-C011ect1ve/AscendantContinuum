using UnityEngine;
using UnityEngine.UI;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Tiny runtime helper that connects the ⏸ HUD button to PauseMenuController.TogglePause().
    /// Added by the RealmPrefabBuilder — no manual setup required.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class PauseButtonWirer : MonoBehaviour
    {
        [SerializeField] private PauseMenuController controller;

        private void Start()
        {
            if (controller == null)
                controller = FindFirstObjectByType<PauseMenuController>();

            if (controller != null)
                GetComponent<Button>().onClick.AddListener(controller.TogglePause);
        }
    }
}
