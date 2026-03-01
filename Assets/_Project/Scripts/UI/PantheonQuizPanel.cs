using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.UI
{
    /// <summary>
    /// Arcane Personality Quiz — presented on Day 4 when
    /// <see cref="PantheonDeityEffects.ShouldShowPantheonQuiz"/> returns true.
    ///
    /// Five questions, each with three answers that map to the six deities.
    /// On submit, calls <see cref="PantheonDeityEffects.CalculateQuizResult"/>
    /// then <see cref="PantheonDeityEffects.SelectDeity"/> and shows a reveal card.
    ///
    /// Hook up in the Inspector: wire to GameEvents.OnPantheonQuizReady or
    /// call <see cref="ShowQuiz"/> directly from OnboardingController.
    /// </summary>
    public sealed class PantheonQuizPanel : MonoBehaviour
    {
        // ── Inspector refs ────────────────────────────────────────────────
        [Header("Root")]
        [SerializeField] private GameObject panelRoot;
        [SerializeField] private CanvasGroup canvasGroup;

        [Header("Question view")]
        [SerializeField] private TextMeshProUGUI questionLabel;
        [SerializeField] private TextMeshProUGUI progressLabel;       // "Question 2 / 5"
        [SerializeField] private Button[] answerButtons;              // expects exactly 3
        [SerializeField] private TextMeshProUGUI[] answerLabels;      // paired with buttons

        [Header("Result reveal")]
        [SerializeField] private GameObject resultPanel;
        [SerializeField] private TextMeshProUGUI deityNameLabel;
        [SerializeField] private TextMeshProUGUI deityDescriptionLabel;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button retakeButton;

        [Header("Timing")]
        [SerializeField] private float fadeDuration = 0.35f;

        // ── Quiz data ─────────────────────────────────────────────────────
        // Each question maps answer indices 0/1/2 to deity weights.
        // Row format: { question text, answer0, answer1, answer2,
        //               deity0, deity1, deity2, deity3, deity4, deity5  <-- weight per answer (6 per answer = 18 per row) }
        // Encoded as parallel arrays for clarity.

        private static readonly string[] QUESTIONS =
        {
            "When you first enter a new realm, what pulls your attention?",
            "A rare sigil appears — what do you feel first?",
            "The ritual is nearly complete. One last stroke remains. You…",
            "You notice a hidden pattern others have missed. You…",
            "A friend struggles with a ritual. You offer…",
        };

        private static readonly string[][] ANSWERS =
        {
            new[] { "The glow at the edges — something to chase",
                    "The stillness — I breathe it in",
                    "The structure — I map it immediately" },
            new[] { "Excitement — I want to collect it",
                    "Wonder — I just stare for a moment",
                    "Curiosity — I wonder what it means" },
            new[] { "Make it bold — maximum impact",
                    "Make it gentle — let the realm absorb it",
                    "Replay the sequence in my head first" },
            new[] { "Share it, quietly, as a gift",
                    "Write it down before I forget",
                    "Experiment with it to see how deep it goes" },
            new[] { "A shortcut that makes it easier",
                    "Quiet encouragement, no pressure",
                    "A detailed breakdown of what's going wrong" },
        };

        // Deity vote weights per answer. Index: [question][answer][deity]
        // Deities: 0=Flame, 1=Herald, 2=Archivist, 3=Mechanic, 4=Scribe, 5=Nurturer
        private static readonly int[][][] WEIGHTS =
        {
            // Q0 — "what pulls your attention"
            new[] { new[] {2,1,0,0,0,0}, new[] {0,0,1,0,0,2}, new[] {0,0,0,2,1,0} },
            // Q1 — "rare sigil appears"
            new[] { new[] {2,0,1,0,0,0}, new[] {0,2,0,0,1,0}, new[] {0,1,0,1,2,0} },
            // Q2 — "one last stroke"
            new[] { new[] {2,1,0,0,0,0}, new[] {0,0,0,0,0,2}, new[] {0,0,2,1,0,0} },
            // Q3 — "hidden pattern"
            new[] { new[] {0,1,0,0,0,2}, new[] {0,0,2,0,1,0}, new[] {0,1,0,2,1,0} },
            // Q4 — "friend struggles"
            new[] { new[] {0,0,0,2,1,0}, new[] {0,1,0,0,0,2}, new[] {0,0,1,1,2,0} },
        };

        private static readonly string[] DEITY_DESCRIPTIONS =
        {
            "Drawn to the spark of creation — you chase ritual variations and rare drops with fierce joy.",
            "Brimming with curiosity — hidden sigils glow for you and serendipity finds you more often.",
            "A keeper of meaning — your Echo Archive recordings are elevated and Memory Buffs restore focus.",
            "A builder at heart — crafting probabilities reveal themselves and bonus variants reward your precision.",
            "A seeker of deep truths — hints go deeper for you and the whisper of a Sixth Realm may find you.",
            "A gentle presence — you never lose sparks on failure and your lantern light glows softer for others.",
        };

        // ── State ─────────────────────────────────────────────────────────
        private int   _currentQuestion;
        private int[] _answers;
        private int   _pendingDeity = -1;
        private bool  _transitioning;

        // ── Unity lifecycle ───────────────────────────────────────────────
        private void Awake()
        {
            if (panelRoot != null) panelRoot.SetActive(false);
            if (resultPanel != null) resultPanel.SetActive(false);
        }

        private void OnEnable()
        {
            GameEvents.OnPantheonQuizReady += ShowQuiz;
        }

        private void OnDisable()
        {
            GameEvents.OnPantheonQuizReady -= ShowQuiz;
        }

        // ── Public API ────────────────────────────────────────────────────

        /// <summary>Opens the quiz from the start.</summary>
        public void ShowQuiz()
        {
            if (PantheonDeityEffects.Instance != null &&
                !PantheonDeityEffects.Instance.ShouldShowPantheonQuiz())
                return;

            _answers = new int[QUESTIONS.Length];
            _currentQuestion = 0;

            if (panelRoot != null) panelRoot.SetActive(true);
            if (resultPanel != null) resultPanel.SetActive(false);

            WireAnswerButtons();
            WireResultButtons();
            ShowQuestion(_currentQuestion);
            StartCoroutine(FadeIn());
        }

        // ── Question flow ─────────────────────────────────────────────────

        private void WireAnswerButtons()
        {
            for (int i = 0; i < answerButtons.Length; i++)
            {
                int captured = i;
                answerButtons[i].onClick.RemoveAllListeners();
                answerButtons[i].onClick.AddListener(() => OnAnswerSelected(captured));
            }
        }

        private void WireResultButtons()
        {
            confirmButton?.onClick.RemoveAllListeners();
            confirmButton?.onClick.AddListener(OnConfirmDeity);

            retakeButton?.onClick.RemoveAllListeners();
            retakeButton?.onClick.AddListener(OnRetakeQuiz);
        }

        private void ShowQuestion(int index)
        {
            if (questionLabel != null)
                questionLabel.text = QUESTIONS[index];

            if (progressLabel != null)
                progressLabel.text = $"Question {index + 1} / {QUESTIONS.Length}";

            string[] opts = ANSWERS[index];
            for (int i = 0; i < answerLabels.Length && i < opts.Length; i++)
                if (answerLabels[i] != null) answerLabels[i].text = opts[i];
        }

        private void OnAnswerSelected(int answerIndex)
        {
            if (_transitioning) return;

            _answers[_currentQuestion] = answerIndex;
            _currentQuestion++;

            if (_currentQuestion >= QUESTIONS.Length)
            {
                ShowResult();
            }
            else
            {
                ShowQuestion(_currentQuestion);
            }
        }

        // ── Result ────────────────────────────────────────────────────────

        private void ShowResult()
        {
            // Tally weighted votes
            int[] tally = new int[6];
            for (int q = 0; q < _answers.Length; q++)
            {
                int a = _answers[q];
                int[] w = WEIGHTS[q][a];
                for (int d = 0; d < 6; d++)
                    tally[d] += w[d];
            }

            // Also use CalculateQuizResult for consistency
            _pendingDeity = PantheonDeityEffects.CalculateQuizResult(_answers);

            string name = PantheonDeityEffects.DEITY_NAMES[_pendingDeity];
            string desc = DEITY_DESCRIPTIONS[_pendingDeity];

            if (deityNameLabel       != null) deityNameLabel.text       = $"✦ {name}";
            if (deityDescriptionLabel != null) deityDescriptionLabel.text = desc;

            // Swap panels
            foreach (var btn in answerButtons) btn.gameObject.SetActive(false);
            if (questionLabel  != null) questionLabel.gameObject.SetActive(false);
            if (progressLabel  != null) progressLabel.gameObject.SetActive(false);
            if (resultPanel    != null) resultPanel.SetActive(true);
        }

        private void OnConfirmDeity()
        {
            if (_pendingDeity < 0) return;
            PantheonDeityEffects.Instance?.SelectDeity(_pendingDeity);
            StartCoroutine(FadeOutAndClose());
        }

        private void OnRetakeQuiz()
        {
            // Reset to question 1
            _pendingDeity = -1;
            _currentQuestion = 0;
            _answers = new int[QUESTIONS.Length];

            if (resultPanel != null) resultPanel.SetActive(false);
            foreach (var btn in answerButtons) btn.gameObject.SetActive(true);
            if (questionLabel  != null) questionLabel.gameObject.SetActive(true);
            if (progressLabel  != null) progressLabel.gameObject.SetActive(true);

            ShowQuestion(0);
        }

        // ── Fade helpers ──────────────────────────────────────────────────

        private IEnumerator FadeIn()
        {
            if (canvasGroup == null) yield break;
            _transitioning = true;
            canvasGroup.alpha = 0f;
            float t = 0f;
            while (t < fadeDuration)
            {
                t += Time.deltaTime;
                canvasGroup.alpha = t / fadeDuration;
                yield return null;
            }
            canvasGroup.alpha = 1f;
            _transitioning = false;
        }

        private IEnumerator FadeOutAndClose()
        {
            if (canvasGroup != null)
            {
                _transitioning = true;
                float t = fadeDuration;
                while (t > 0f)
                {
                    t -= Time.deltaTime;
                    canvasGroup.alpha = t / fadeDuration;
                    yield return null;
                }
                canvasGroup.alpha = 0f;
                _transitioning = false;
            }
            if (panelRoot != null) panelRoot.SetActive(false);
        }

        // ── Accessibility ─────────────────────────────────────────────────

        /// <summary>Returns current question text for screen readers.</summary>
        public string GetAccessibilitySummary()
        {
            if (_currentQuestion >= QUESTIONS.Length)
                return _pendingDeity >= 0
                    ? $"Your deity is {PantheonDeityEffects.DEITY_NAMES[_pendingDeity]}. {DEITY_DESCRIPTIONS[_pendingDeity]}"
                    : "Quiz complete.";

            return $"Question {_currentQuestion + 1} of {QUESTIONS.Length}: {QUESTIONS[_currentQuestion]}";
        }
    }
}
