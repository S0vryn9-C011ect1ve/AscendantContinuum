using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using AscendantContinuum.Core;
using AscendantContinuum.Data;
using AscendantContinuum.Systems;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Provides the full Sigil Journal view — a scrollable collection of all
    /// sigils the player has drawn, showing:
    ///   • Rendered sigil image (from saved PNG or procedurally redrawn)
    ///   • Mutation stage indicator (pulse / tint / shimmer)
    ///   • Date and complexity rating
    ///   • Share/export button
    ///   • Count + consecutive-streak header
    ///
    /// UI prefab requirements (assign in Inspector or locate via tags):
    ///   • journalPanel   — root CanvasGroup
    ///   • scrollContent  — LayoutGroup parent
    ///   • sigilCardPrefab — prefab with Image, TMP labels, Button
    ///   • headerLabel    — header TextMeshProUGUI
    /// </summary>
    public sealed class SigilJournalManager : MonoBehaviour
    {
        public static SigilJournalManager Instance { get; private set; }

        // ── Inspector refs ────────────────────────────────────────────────────
        [SerializeField] private CanvasGroup       journalPanel;
        [SerializeField] private Transform         scrollContent;
        [SerializeField] private GameObject        sigilCardPrefab;
        [SerializeField] private TextMeshProUGUI   headerLabel;

        [Header("Animation")]
        [SerializeField] private float             openDuration  = 0.35f;
        [SerializeField] private float             closeDuration = 0.25f;

        // ── Runtime state ──────────────────────────────────────────────────────
        private bool _isOpen;
        private List<GameObject> _spawnedCards = new List<GameObject>();

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            if (journalPanel != null)
            {
                journalPanel.alpha          = 0f;
                journalPanel.interactable   = false;
                journalPanel.blocksRaycasts = false;
            }
        }

        private void OnEnable()
        {
            GameEvents.OnSigilCompleted += _ => RefreshHeaderIfOpen();
        }

        private void OnDisable()
        {
            GameEvents.OnSigilCompleted -= _ => RefreshHeaderIfOpen();
        }

        // ── Public API ────────────────────────────────────────────────────────

        public void Open()
        {
            if (_isOpen) return;
            _isOpen = true;
            BuildCards();
            StartCoroutine(FadeCo(0f, 1f, openDuration, true));
        }

        public void Close()
        {
            if (!_isOpen) return;
            _isOpen = false;
            StartCoroutine(FadeCo(1f, 0f, closeDuration, false));
        }

        public void Toggle() { if (_isOpen) Close(); else Open(); }

        // ── Private ───────────────────────────────────────────────────────────

        private void BuildCards()
        {
            // Clear previous
            foreach (var old in _spawnedCards)
                if (old != null) Destroy(old);
            _spawnedCards.Clear();

            if (SaveSystem.Instance == null || sigilCardPrefab == null || scrollContent == null)
                return;

            var playerData = SaveSystem.Instance.CurrentPlayerData;
            var sigilIds   = playerData.sigilIds ?? new string[0];

            RefreshHeader(sigilIds.Length, playerData.consecutiveDays);

            for (int i = sigilIds.Length - 1; i >= 0; i--)
            {
                string sigilId = sigilIds[i];
                var card       = Instantiate(sigilCardPrefab, scrollContent);
                PopulateCard(card, sigilId, i);
                _spawnedCards.Add(card);
            }
        }

        private void PopulateCard(GameObject card, string sigilId, int index)
        {
            // Load saved PNG if available
            var img = card.transform.Find("SigilImage")?.GetComponent<Image>();
            if (img != null)
            {
                string pngPath = System.IO.Path.Combine(
                    Application.persistentDataPath, "sigils", sigilId + ".png");
                if (System.IO.File.Exists(pngPath))
                {
                    byte[]    bytes   = System.IO.File.ReadAllBytes(pngPath);
                    Texture2D tex     = new Texture2D(2, 2);
                    tex.LoadImage(bytes);
                    img.sprite = Sprite.Create(tex,
                        new Rect(0, 0, tex.width, tex.height), Vector2.one * 0.5f);
                }
            }

            // Date label
            var dateLabel = card.transform.Find("DateLabel")?.GetComponent<TextMeshProUGUI>();
            if (dateLabel != null)
            {
                // sigilId format: "sigil_yyyyMMdd_HHmmss_nnn"
                string friendly = ParseFriendlyDate(sigilId);
                dateLabel.text  = friendly;
            }

            // Mutation stage indicator
            var stageDot = card.transform.Find("StageDot")?.GetComponent<Image>();
            if (stageDot != null && SigilMutationSystem.Instance != null)
            {
                var props = SigilMutationSystem.Instance.GetMutationProperties(sigilId);
                stageDot.color = props.Stage switch
                {
                    1 => new Color(0.5f, 0.8f, 1f),   // pulse — ice blue
                    2 => new Color(0.8f, 0.4f, 1f),   // tint  — violet
                    3 => new Color(1f,   0.9f, 0.4f), // shimmer — gold
                    _ => new Color(0.5f, 0.5f, 0.5f)  // fresh — grey
                };
            }

            // Share button
            var shareBtn = card.transform.Find("ShareButton")?.GetComponent<Button>();
            string capturedId = sigilId;
            shareBtn?.onClick.AddListener(() => SigilArtifactExporter.Instance?.ShareExisting(capturedId));

            // Card number
            var numLabel = card.transform.Find("NumberLabel")?.GetComponent<TextMeshProUGUI>();
            if (numLabel != null) numLabel.text = $"#{index + 1}";
        }

        private void RefreshHeader(int count, int streak)
        {
            if (headerLabel == null) return;
            headerLabel.text = streak > 1
                ? $"{count} Sigils · {streak}-day Continuum"
                : $"{count} Sigils Drawn";
        }

        private void RefreshHeaderIfOpen()
        {
            if (!_isOpen || SaveSystem.Instance == null) return;
            var d = SaveSystem.Instance.CurrentPlayerData;
            RefreshHeader(d.sigilIds?.Length ?? 0, d.consecutiveDays);
        }

        private static string ParseFriendlyDate(string sigilId)
        {
            // Expected: "sigil_20240101_120000_001"
            var parts = sigilId.Split('_');
            if (parts.Length >= 2 && parts[1].Length == 8)
            {
                if (System.DateTime.TryParseExact(parts[1], "yyyyMMdd",
                    System.Globalization.CultureInfo.InvariantCulture,
                    System.Globalization.DateTimeStyles.None, out var dt))
                    return dt.ToString("MMMM d, yyyy");
            }
            return "Unknown date";
        }

        private IEnumerator FadeCo(float from, float to, float duration, bool interactive)
        {
            if (journalPanel == null) yield break;

            journalPanel.interactable   = interactive;
            journalPanel.blocksRaycasts = interactive;

            float elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed         += Time.deltaTime;
                journalPanel.alpha = Mathf.Lerp(from, to, elapsed / duration);
                yield return null;
            }
            journalPanel.alpha = to;
        }
    }
}
