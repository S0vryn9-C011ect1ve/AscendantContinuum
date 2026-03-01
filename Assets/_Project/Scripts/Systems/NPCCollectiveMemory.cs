using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Systems
{
    /// <summary>
    /// NPC Collective Memory — NPCs learn from ALL players' interactions and evolve.
    ///
    /// Three NPCs are tracked:
    ///   • Sparkus   (Emberforge)  — tracks emotion / creativity signals
    ///   • Petalina  (Verdant)     — tracks community mood (joy, peace, sadness, curiosity)
    ///   • Lumina    (Echo Fields) — tracks constellation patterns players submit
    ///
    /// Every player interaction pushes counts into PlayerPrefs. When thresholds are
    /// reached Sparkus evolves dialogue, Petalina adapts the garden mood, and Lumina
    /// creates a "Hall of Fame" constellation entry.
    ///
    /// Firebase sync queues each interaction for the cloud aggregation function.
    /// </summary>
    public sealed class NPCCollectiveMemory : MonoBehaviour
    {
        public static NPCCollectiveMemory Instance { get; private set; }

        // ── PlayerPrefs prefix ────────────────────────────────────────────
        private const string PREF = "NPC_Memory_";

        // ── NPC identifiers ───────────────────────────────────────────────
        public const string NPC_SPARKUS  = "Sparkus";
        public const string NPC_PETALINA = "Petalina";
        public const string NPC_LUMINA   = "Lumina";

        // ── Emotion categories ────────────────────────────────────────────
        public enum Emotion { Joy, Peace, Sadness, Curiosity, Creativity, Wonder, Calm }

        // ── Threshold for NPC evolution ───────────────────────────────────
        private const int SPARKUS_CREATIVITY_THRESHOLD = 1000;
        private const int PETALINA_JOY_THRESHOLD        = 5000;
        private const int LUMINA_PATTERN_THRESHOLD      = 100;

        // ── Events ────────────────────────────────────────────────────────
        public event Action<string, string> OnNPCEvolved;     // npcName, evolutionKey
        public event Action<Emotion>        OnCommunityMoodShift;

        // ── Lifecycle ─────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        /// <summary>Flush all pending PlayerPrefs writes once per pause/quit — not per interaction.</summary>
        private void OnApplicationPause(bool paused) { if (paused) PlayerPrefs.Save(); }
        private void OnApplicationQuit()             { PlayerPrefs.Save(); }

        // ── Recording ─────────────────────────────────────────────────────

        /// <summary>
        /// Record a player emotion expressed during an NPC interaction.
        /// emotion can be a plain string such as "creative", "happy", "curious", etc.
        /// The method normalises it to the nearest Emotion enum value.
        /// </summary>
        public void RecordPlayerEmotion(string npcName, string rawEmotion, string optionalMessage = "")
        {
            Emotion emotion = NormaliseEmotion(rawEmotion);
            string key = $"{PREF}{npcName}_{emotion}";
            int count = PlayerPrefs.GetInt(key, 0) + 1;
            PlayerPrefs.SetInt(key, count);
            // NOTE: PlayerPrefs.Save() is batched to OnApplicationPause/Quit — not called per interaction.

            // Check NPC evolution thresholds
            if (npcName == NPC_SPARKUS && emotion == Emotion.Creativity && count >= SPARKUS_CREATIVITY_THRESHOLD)
                TriggerSparkusEvolution("collaborative_creation");

            if (npcName == NPC_PETALINA && emotion == Emotion.Joy && count >= PETALINA_JOY_THRESHOLD)
                TriggerPetalinaEvolution("full_bloom_garden");

            // Track community mood shift
            CheckCommunityMoodShift();

            // Queue Firebase sync
            FirebaseManager.Instance?.SaveData(
                "npcMemory",
                $"{npcName}_{emotion}_{count}",
                new { npc = npcName, emotion = emotion.ToString(), message = optionalMessage }
            );
        }

        /// <summary>Record a constellation pattern submitted by a player to Lumina.</summary>
        public void RecordConstellationPattern(string patternHash, string patternName)
        {
            string countKey = $"{PREF}{NPC_LUMINA}_Pattern_{patternHash}";
            int count = PlayerPrefs.GetInt(countKey, 0) + 1;
            PlayerPrefs.SetInt(countKey, count);

            int totalPatterns = PlayerPrefs.GetInt($"{PREF}{NPC_LUMINA}_TotalPatterns", 0) + 1;
            PlayerPrefs.SetInt($"{PREF}{NPC_LUMINA}_TotalPatterns", totalPatterns);
            // Batched save — not immediate.

            if (count >= LUMINA_PATTERN_THRESHOLD)
                TriggerLuminaHallOfFame(patternName, count);
        }

        // ── Evolved Dialogue ─────────────────────────────────────────────

        /// <summary>
        /// Returns the current evolved dialogue line for the given NPC.
        /// Falls back to a neutral line if no threshold has been reached.
        /// </summary>
        public string GetEvolvingDialogue(string npcName)
        {
            return npcName switch
            {
                NPC_SPARKUS  => BuildSparkusDialogue(),
                NPC_PETALINA => BuildPetalinaDialogue(),
                NPC_LUMINA   => BuildLuminaDialogue(),
                _            => "Welcome, Seeker."
            };
        }

        /// <summary>Returns the community's dominant emotion across all NPC interactions.</summary>
        public Emotion GetDominantEmotion()
        {
            Emotion best  = Emotion.Curiosity;
            int     max   = 0;
            foreach (Emotion e in Enum.GetValues(typeof(Emotion)))
            {
                int total = 0;
                foreach (string npc in new[] { NPC_SPARKUS, NPC_PETALINA, NPC_LUMINA })
                    total += PlayerPrefs.GetInt($"{PREF}{npc}_{e}", 0);
                if (total > max) { max = total; best = e; }
            }
            return best;
        }

        /// <summary>Returns raw count of a specific emotion expressed to an NPC.</summary>
        public int GetEmotionCount(string npcName, Emotion emotion) =>
            PlayerPrefs.GetInt($"{PREF}{npcName}_{emotion}", 0);

        // ── Private: NPC dialogue builders ───────────────────────────────

        private string BuildSparkusDialogue()
        {
            int creativity = GetEmotionCount(NPC_SPARKUS, Emotion.Creativity);
            int joy        = GetEmotionCount(NPC_SPARKUS, Emotion.Joy);

            if (creativity >= SPARKUS_CREATIVITY_THRESHOLD)
                return "So many seekers are feeling creative lately! The flames must be inspiring you all. " +
                       "A new ritual — Collaborative Creation — has awakened in the forge.";
            if (creativity >= 500)
                return "The forge hums with creative energy. Something powerful is building...";
            if (joy >= 200)
                return "What joy fills these halls! The sparks burn brighter when happiness is near.";
            return "The forge awaits your creative fire, Seeker.";
        }

        private string BuildPetalinaDialogue()
        {
            Emotion mood = GetDominantEmotion();
            int sadness  = GetEmotionCount(NPC_PETALINA, Emotion.Sadness);

            if (sadness >= 500)
                return "Many seekers come here heavy-hearted lately. A Comfort Flower has bloomed just for them.";
            return mood switch
            {
                Emotion.Joy      => "The community blooms with joy! Look how brightly the flowers grow.",
                Emotion.Peace    => "Such peace settles over the garden. The plants grow calm and steady.",
                Emotion.Curiosity=> "Curiosity fills this garden. New seeds are sprouting in hidden corners.",
                Emotion.Sadness  => "Even in sadness, flowers bloom. You are not alone here, Seeker.",
                _                => "Welcome to the garden. Every seed you plant carries the whole community's hope."
            };
        }

        private string BuildLuminaDialogue()
        {
            int totalPatterns = PlayerPrefs.GetInt($"{PREF}{NPC_LUMINA}_TotalPatterns", 0);
            string hallOfFame = PlayerPrefs.GetString($"{PREF}{NPC_LUMINA}_HallOfFame_Latest", "");

            if (!string.IsNullOrEmpty(hallOfFame))
                return $"Ah! {hallOfFame} has become a sacred constellation! Seekers traced it {LUMINA_PATTERN_THRESHOLD}+ times.";
            if (totalPatterns >= 50)
                return $"The stars remember {totalPatterns} unique patterns. You seekers are writing the sky's story.";
            return "The Echo Fields hold memories of every pattern ever traced. What will yours be?";
        }

        // ── Private: NPC evolution triggers ──────────────────────────────

        private void TriggerSparkusEvolution(string evolutionKey)
        {
            if (PlayerPrefs.GetInt($"{PREF}Sparkus_Evolved_{evolutionKey}", 0) == 1) return;
            PlayerPrefs.SetInt($"{PREF}Sparkus_Evolved_{evolutionKey}", 1);

            Debug.Log($"[NPCMemory] Sparkus evolved: {evolutionKey}");
            OnNPCEvolved?.Invoke(NPC_SPARKUS, evolutionKey);
            AchievementManager.Instance?.UnlockAchievement("npc_memory_witness");
        }

        private void TriggerPetalinaEvolution(string evolutionKey)
        {
            if (PlayerPrefs.GetInt($"{PREF}Petalina_Evolved_{evolutionKey}", 0) == 1) return;
            PlayerPrefs.SetInt($"{PREF}Petalina_Evolved_{evolutionKey}", 1);

            Debug.Log($"[NPCMemory] Petalina evolved: {evolutionKey}");
            OnNPCEvolved?.Invoke(NPC_PETALINA, evolutionKey);
        }

        private void TriggerLuminaHallOfFame(string patternName, int count)
        {
            PlayerPrefs.SetString($"{PREF}{NPC_LUMINA}_HallOfFame_Latest", patternName);

            Debug.Log($"[NPCMemory] Lumina Hall of Fame: {patternName} ({count} traces)");
            OnNPCEvolved?.Invoke(NPC_LUMINA, $"hall_of_fame:{patternName}");
            AchievementManager.Instance?.UnlockAchievement("hall_of_fame_pattern");
        }

        private void CheckCommunityMoodShift()
        {
            Emotion dominant = GetDominantEmotion();
            Emotion prev = (Emotion)PlayerPrefs.GetInt($"{PREF}LastDominantEmotion", (int)Emotion.Curiosity);
            if (dominant != prev)
            {
                PlayerPrefs.SetInt($"{PREF}LastDominantEmotion", (int)dominant);
                OnCommunityMoodShift?.Invoke(dominant);
            }
        }

        private Emotion NormaliseEmotion(string raw)
        {
            string lower = raw.ToLower().Trim();
            if (lower.Contains("creat") || lower.Contains("inspir"))  return Emotion.Creativity;
            if (lower.Contains("joy") || lower.Contains("happy") || lower.Contains("excit")) return Emotion.Joy;
            if (lower.Contains("peace") || lower.Contains("calm") || lower.Contains("relax")) return Emotion.Peace;
            if (lower.Contains("sad") || lower.Contains("down") || lower.Contains("tired"))   return Emotion.Sadness;
            if (lower.Contains("won") || lower.Contains("amaze") || lower.Contains("wow"))    return Emotion.Wonder;
            if (lower.Contains("curio") || lower.Contains("interest") || lower.Contains("quest")) return Emotion.Curiosity;
            return Emotion.Curiosity; // default
        }
    }
}
