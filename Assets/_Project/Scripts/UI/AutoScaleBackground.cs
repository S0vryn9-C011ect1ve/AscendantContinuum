using UnityEngine;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Auto-scales a sprite to fill the camera view
    /// Attach to background sprites to ensure they always fill the screen
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    public class AutoScaleBackground : MonoBehaviour
    {
        [SerializeField] private bool maintainAspectRatio = true;
        [SerializeField] private float padding = 0f; // Extra scale multiplier

        private SpriteRenderer spriteRenderer;
        private Camera mainCamera;

        private void Start()
        {
            spriteRenderer = GetComponent<SpriteRenderer>();
            mainCamera = Camera.main;

            if (mainCamera == null)
            {
                Debug.LogWarning("[AutoScaleBackground] No main camera found!");
                return;
            }

            ScaleToFitCamera();
        }

        private void ScaleToFitCamera()
        {
            if (spriteRenderer == null || spriteRenderer.sprite == null || mainCamera == null)
                return;

            // Get sprite dimensions
            float spriteWidth = spriteRenderer.sprite.bounds.size.x;
            float spriteHeight = spriteRenderer.sprite.bounds.size.y;

            // Get camera dimensions (world units)
            float cameraHeight = mainCamera.orthographicSize * 2f;
            float cameraWidth = cameraHeight * mainCamera.aspect;

            // Calculate scale needed to fill camera view
            float scaleX = cameraWidth / spriteWidth;
            float scaleY = cameraHeight / spriteHeight;

            Vector3 scale;

            if (maintainAspectRatio)
            {
                // Use the larger scale to ensure full coverage
                float uniformScale = Mathf.Max(scaleX, scaleY);
                scale = new Vector3(uniformScale, uniformScale, 1f);
            }
            else
            {
                // Stretch to exactly fit (may distort)
                scale = new Vector3(scaleX, scaleY, 1f);
            }

            // Apply padding
            scale *= (1f + padding);

            transform.localScale = scale;

            Debug.Log($"[AutoScaleBackground] Scaled {gameObject.name} to {scale}");
        }

        // Call this if screen resolution changes
        public void RefreshScale()
        {
            ScaleToFitCamera();
        }
    }
}
