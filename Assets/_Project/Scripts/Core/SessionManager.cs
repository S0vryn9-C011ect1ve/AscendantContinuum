using System.Collections;
using UnityEngine;
using AscendantContinuum.Core;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// Owns the per-session emotional arc.
    ///
    /// States: Entering → Observing → Drawing → SkyReacting → Blooming
    ///       → Reflecting → Nudging → Closing
    ///
    /// Raises <see cref="GameEvents.OnSessionPhaseChanged"/> at every transition.
    /// Also enforces a soft 5-minute session cap with a gentle fade hint at 4:30.
    /// </summary>
    public sealed class SessionManager : MonoBehaviour
    {
        public static SessionManager Instance { get; private set; }

        // ── Inspector ─────────────────────────────────────────────────────────
        [Header("Session Timing")]
        [SerializeField] private float softCapSeconds     = 300f;  // 5 minutes
        [SerializeField] private float softCapHintSeconds = 270f;  // 4:30 — show hint
        [SerializeField] private float closingDelay       = 4f;    // after Nudging

        // ── State ─────────────────────────────────────────────────────────────
        private SessionPhase _phase    = SessionPhase.Entering;
        private float        _sessionStart;
        private bool         _capHintShown;

        public SessionPhase CurrentPhase => _phase;
        public float        SessionTime  => Time.time - _sessionStart;

        // ── Lifecycle ─────────────────────────────────────────────────────────

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;

            // Increment lifetime session counter (read by PantheonDeityEffects Day4 gate)
            int total = PlayerPrefs.GetInt("SessionCount_Total", 0) + 1;
            PlayerPrefs.SetInt("SessionCount_Total", total);
            PlayerPrefs.Save();
        }

        private void Start()
        {
            _sessionStart = Time.time;
            SetPhase(SessionPhase.Entering);
            StartCoroutine(SessionArcCo());
        }

        private void Update()
        {
            // Soft cap hint
            if (!_capHintShown && SessionTime >= softCapHintSeconds)
            {
                _capHintShown = true;
                Debug.Log("[Session] Soft cap hint: 'The ritual is complete when you are ready.'");
                // HUD manager picks this up via event
                GameEvents.RaiseSessionPhaseChanged(SessionPhase.Reflecting);
            }
        }

        // ── Public API ────────────────────────────────────────────────────────

        /// <summary>
        /// Advances the session to a specific phase.
        /// Called externally by <see cref="SigilCompletionHandler"/> and
        /// <see cref="GestureDrawingController"/>.
        /// </summary>
        public void SetPhase(SessionPhase phase)
        {
            if (_phase == phase) return;
            _phase = phase;
            GameEvents.RaiseSessionPhaseChanged(phase);
            Debug.Log($"[Session] Phase → {phase}");
        }

        // ── Automatic arc coroutine ───────────────────────────────────────────

        private IEnumerator SessionArcCo()
        {
            // Entering: brief silence
            yield return new WaitForSeconds(1.5f);
            SetPhase(SessionPhase.Observing);

            // Wait for sigil commit (listening on event) or soft cap
            yield return new WaitUntil(() =>
                _phase >= SessionPhase.Blooming ||
                SessionTime >= softCapSeconds);

            if (_phase < SessionPhase.Reflecting)
                SetPhase(SessionPhase.Reflecting);

            yield return new WaitForSeconds(closingDelay);

            if (_phase < SessionPhase.Closing)
            {
                SetPhase(SessionPhase.Closing);
                FinishSession();
            }
        }

        private void FinishSession()
        {
            var data = SaveSystem.Instance?.CurrentPlayerData;
            if (data != null)
            {
                data.totalSessionsCompleted++;
                string today = System.DateTime.UtcNow.ToString("yyyy-MM-dd");
                if (data.lastSessionDate == today)
                    data.consecutiveDays++;
                else
                    data.consecutiveDays = data.lastSessionDate ==
                        System.DateTime.UtcNow.AddDays(-1).ToString("yyyy-MM-dd") ? data.consecutiveDays + 1 : 1;
                data.lastSessionDate = today;
            }
            SaveSystem.Instance?.SaveGame();
            Debug.Log("[Session] Closed — saved.");
        }
    }
}
