using System;
using AscendantContinuum.Systems;

namespace AscendantContinuum.Core
{
    // ── Session phase enum ────────────────────────────────────────────────────
    public enum SessionPhase
    {
        Entering,
        Observing,
        Drawing,
        SkyReacting,
        Blooming,
        Reflecting,
        Nudging,
        Closing
    }

    public static class GameEvents
    {
        // ── Core ──────────────────────────────────────────────────────────────
        public static event Action<GameState, GameState>    OnGameStateChanged;
        public static event Action<string>                  OnRealmLoaded;
        public static event Action<string>                  OnRealmTransitionStarted;
        public static event Action<string>                  OnRealmTransitionCompleted;
        public static event Action<bool, string>            OnSaveCompleted;

        // ── Session arc ───────────────────────────────────────────────────────
        public static event Action<SessionPhase>            OnSessionPhaseChanged;

        // ── Sigil / drawing ───────────────────────────────────────────────────
        /// <summary>Fired per stroke point: velocity (world units/sec), curvature (0-1).</summary>
        public static event Action<float, float>            OnSigilStrokePoint;
        /// <summary>Fired when the player lifts their finger and a stroke is complete.</summary>
        public static event Action                          OnSigilStrokeEnded;
        /// <summary>Fired when the player commits the full sigil drawing.</summary>
        public static event Action<SigilAnalysisResult>    OnSigilDrawingCommitted;
        /// <summary>Fired once the sigil has been generated and saved.</summary>
        public static event Action<AscendantContinuum.Data.SigilData> OnSigilCompleted;

        // ── Collective / resonance ────────────────────────────────────────────
        public static event Action                          OnCollectivePresencePulse;
        public static event Action<float>                   OnCollectiveEnergyChanged;

        // ── Celestial / live events ───────────────────────────────────────────
        public static event Action<string>                  OnCelestialEventPeak;

        // ── Cosmetics ────────────────────────────────────────────────────────
        public static event Action<string>                  OnCosmeticUnlocked;

        // ── Chain / social ────────────────────────────────────────────────────
        public static event Action<string>                  OnChainCompleted;

        // ── Sigil crafting ────────────────────────────────────────────────────
        /// <summary>Fired when a sigil combination is successfully crafted via SigilCraftingManager.</summary>
        public static event Action<string, string, bool>    OnSigilCrafted;     // resultId, combinationName, isFirstDiscovery

        // ── Realm lifecycle ───────────────────────────────────────────────────
        /// <summary>Fired when a realm run is fully completed (score calculated, exit confirmed).</summary>
        public static event Action<string, int>             OnRealmCompleted;   // realmId, score

        // ── Pantheon / personality quiz ───────────────────────────────────────
        /// <summary>Fired when Day 4 conditions are met and the Arcane Personality Quiz should be shown.</summary>
        public static event Action                          OnPantheonQuizReady;

        // ─────────────────────────────────────── Raisers ─────────────────────

        internal static void RaiseGameStateChanged(GameState previousState, GameState newState)
            => OnGameStateChanged?.Invoke(previousState, newState);

        internal static void RaiseRealmLoaded(string realmId)
            => OnRealmLoaded?.Invoke(realmId);

        internal static void RaiseRealmTransitionStarted(string realmId)
            => OnRealmTransitionStarted?.Invoke(realmId);

        internal static void RaiseRealmTransitionCompleted(string realmId)
            => OnRealmTransitionCompleted?.Invoke(realmId);

        internal static void RaiseSaveCompleted(bool success, string detail)
            => OnSaveCompleted?.Invoke(success, detail);

        public static void RaiseSessionPhaseChanged(SessionPhase phase)
            => OnSessionPhaseChanged?.Invoke(phase);

        public static void RaiseSigilStrokePoint(float velocity, float curvature)
            => OnSigilStrokePoint?.Invoke(velocity, curvature);

        public static void RaiseSigilStrokeEnded()
            => OnSigilStrokeEnded?.Invoke();

        public static void RaiseSigilDrawingCommitted(SigilAnalysisResult analysis)
            => OnSigilDrawingCommitted?.Invoke(analysis);

        public static void RaiseSigilCompleted(AscendantContinuum.Data.SigilData sigil)
            => OnSigilCompleted?.Invoke(sigil);

        public static void RaiseCollectivePresencePulse()
            => OnCollectivePresencePulse?.Invoke();

        public static void RaiseCollectiveEnergyChanged(float energy)
            => OnCollectiveEnergyChanged?.Invoke(energy);

        public static void RaiseCelestialEventPeak(string eventId)
            => OnCelestialEventPeak?.Invoke(eventId);

        public static void RaiseCosmeticUnlocked(string cosmeticId)
            => OnCosmeticUnlocked?.Invoke(cosmeticId);

        public static void RaiseChainCompleted(string chainId)
            => OnChainCompleted?.Invoke(chainId);

        public static void RaiseSigilCrafted(string resultId, string combinationName, bool isFirstDiscovery)
            => OnSigilCrafted?.Invoke(resultId, combinationName, isFirstDiscovery);

        public static void RaisePantheonQuizReady()
            => OnPantheonQuizReady?.Invoke();

        public static void RaiseRealmCompleted(string realmId, int score)
            => OnRealmCompleted?.Invoke(realmId, score);
    }
}
