using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// A single row in the Season Pass UI representing one reward tier.
    ///
    /// Attach to the root of the tier row prefab. <see cref="SeasonPassUIManager"/>
    /// calls <see cref="Populate"/> immediately after instantiation.
    ///
    /// Visual states:
    ///   • Locked   — muted colour, claim button disabled, lock icon visible
    ///   • Unlocked — full colour, claim button enabled, lock icon hidden
    ///   • Claimed  — checkmark shown, claim button hidden
    /// </summary>
    public sealed class SeasonTierRow : MonoBehaviour
    {
        [Header("Labels")]
        [SerializeField] private TextMeshProUGUI tierNumberLabel;   // "Tier 3"
        [SerializeField] private TextMeshProUGUI rewardLabel;       // "Verdant Stamp"

        [Header("Status icons")]
        [SerializeField] private GameObject lockIcon;
        [SerializeField] private GameObject checkIcon;

        [Header("Claim")]
        [SerializeField] private Button claimButton;
        [SerializeField] private TextMeshProUGUI claimButtonLabel;  // "Claim" / "Claimed"

        [Header("Colours")]
        [SerializeField] private Graphic backgroundGraphic;
        [SerializeField] private Color unlockedColor = new Color(0.15f, 0.12f, 0.25f, 1f);
        [SerializeField] private Color lockedColor   = new Color(0.08f, 0.08f, 0.12f, 0.6f);
        [SerializeField] private Color claimedColor  = new Color(0.10f, 0.20f, 0.15f, 1f);

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>
        /// Initialises this row. Called once by <see cref="SeasonPassUIManager"/>
        /// immediately after <c>Instantiate</c>.
        /// </summary>
        /// <param name="tierNumber">1-based tier number for display.</param>
        /// <param name="label">Human-readable reward name.</param>
        /// <param name="unlocked">True when the player has met the XP threshold.</param>
        /// <param name="claimed">True when the reward has already been claimed.</param>
        /// <param name="onClaim">Callback invoked when the claim button is pressed.</param>
        public void Populate(int tierNumber, string label, bool unlocked, bool claimed, Action onClaim)
        {
            if (tierNumberLabel != null)
                tierNumberLabel.text = $"Tier {tierNumber}";

            if (rewardLabel != null)
                rewardLabel.text = label;

            // Claim button
            claimButton?.onClick.RemoveAllListeners();
            if (claimed)
            {
                SetClaimed();
            }
            else if (unlocked)
            {
                SetUnlocked(onClaim);
            }
            else
            {
                SetLocked();
            }
        }

        // ── Visual states ─────────────────────────────────────────────────

        private void SetLocked()
        {
            if (lockIcon    != null) lockIcon.SetActive(true);
            if (checkIcon   != null) checkIcon.SetActive(false);
            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(true);
                claimButton.interactable = false;
            }
            if (claimButtonLabel != null) claimButtonLabel.text = "Locked";
            if (backgroundGraphic != null) backgroundGraphic.color = lockedColor;
        }

        private void SetUnlocked(Action onClaim)
        {
            if (lockIcon    != null) lockIcon.SetActive(false);
            if (checkIcon   != null) checkIcon.SetActive(false);
            if (claimButton != null)
            {
                claimButton.gameObject.SetActive(true);
                claimButton.interactable = true;
                claimButton.onClick.AddListener(() => onClaim?.Invoke());
            }
            if (claimButtonLabel  != null) claimButtonLabel.text = "Claim";
            if (backgroundGraphic != null) backgroundGraphic.color = unlockedColor;
        }

        private void SetClaimed()
        {
            if (lockIcon    != null) lockIcon.SetActive(false);
            if (checkIcon   != null) checkIcon.SetActive(true);
            if (claimButton != null) claimButton.gameObject.SetActive(false);
            if (backgroundGraphic != null) backgroundGraphic.color = claimedColor;
        }

        // ── Accessibility ─────────────────────────────────────────────────

        /// <summary>One-line screen reader description of this row's state.</summary>
        public string GetAccessibilitySummary()
        {
            string name   = rewardLabel      != null ? rewardLabel.text      : "Reward";
            string number = tierNumberLabel  != null ? tierNumberLabel.text  : "Tier";

            if (checkIcon != null && checkIcon.activeSelf)
                return $"{number}: {name} — claimed.";

            if (lockIcon != null && lockIcon.activeSelf)
                return $"{number}: {name} — locked.";

            return $"{number}: {name} — available to claim.";
        }
    }
}
