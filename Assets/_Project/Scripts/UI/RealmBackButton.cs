using UnityEngine;
using UnityEngine.UI;
using AscendantContinuum.Core;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Attach to any Back / Exit button inside a realm scene.
    /// On Awake it registers a listener on the Button component to navigate
    /// back to the Main Menu via SceneService.
    /// </summary>
    [RequireComponent(typeof(Button))]
    public class RealmBackButton : MonoBehaviour
    {
        private void Awake()
        {
            GetComponent<Button>().onClick.AddListener(OnBackPressed);
        }

        private void OnDestroy()
        {
            // Defensive cleanup — avoids stale callbacks if the GO is reused
            var btn = GetComponent<Button>();
            if (btn != null)
                btn.onClick.RemoveListener(OnBackPressed);
        }

        private void OnBackPressed()
        {
            // Auto-save before leaving so progress is not lost
            SaveSystem.Instance?.SaveGame();

            // Transition back to the main menu
            SceneService.TryLoadScene(SceneNames.MainMenu);
        }
    }
}
