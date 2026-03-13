using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using AscendantContinuum.Core;
using AscendantContinuum.Systems;
using AscendantContinuum.UI;

namespace AscendantContinuum.EchoFields
{
    /// <summary>
    /// The Echo Archive — Archaeological Player History System.
    ///
    /// Every ritual completion across all realms leaves a "memory echo" in Echo Fields.
    /// After 30 days echoes fossilise; after 365 days they become Ancient; the launch-week
    /// echoes are Founder class (Legendary rarity).
    ///
    /// Current players can excavate these fossils to discover what past seekers did, 
    /// earning achievements (archaeologist, time_traveler, founder_echo_discovered) and
    /// unlocking lore fragments.
    ///
    /// "Prophecy" echoes are developer-seeded future hints, discoverable any time.
    /// </summary>
    public sealed class EchoArchiveManager : MonoBehaviour
    {
        public static EchoArchiveManager Instance { get; private set; }

        // ── PlayerPrefs keys ─────────────────────────────────────────────
        private const string PREF_ECHO_COUNT   = "EchoArchive_Count";
        private const string PREF_ECHO_PREFIX  = "EchoArchive_Echo_";
        private const string PREF_FOSSIL_COUNT = "EchoArchive_FossilsFound";
        private const string PREF_INSTALL_DATE = "EchoArchive_InstallDate";

        // ── Fossil type thresholds (days) ────────────────────────────────
        private const int FRESH_MAX    = 7;
        private const int AGED_MAX     = 30;
        private const int FOSSIL_MAX   = 365;

        // ── Developer Prophecy echoes seeded at startup ──────────────────
        private static readonly string[] PROPHECY_HINTS =
        {
            "A seventh realm stirs beyond the horizon",
            "When a million sparks ignite, reality shifts",
            "The stars hold a constellation no one has traced yet",
            "The first seeker walks among you still, unseen",
        };

        // ── Types ────────────────────────────────────────────────────────
        public enum FossilType { Fresh, Aged, Fossil, Ancient, Founder, Prophecy }

        [Serializable]
        public class MemoryEcho
        {
            public string seekerTag;       // "Seeker #47283"
            public string realmId;
            public string ritualName;
            public string optionalMessage;
            public long   epochDayBuried;  // days since 2025-01-01 epoch
            public bool   isFossilized;
            public bool   isFounder;       // buried in first 7 epoch-days
            public bool   isProphecy;
        }

        // ── State ────────────────────────────────────────────────────────
        private List<MemoryEcho> _localEchoes = new List<MemoryEcho>();
        private int _fossilsFound;
        private long _installEpochDay;

        // ── Events ───────────────────────────────────────────────────────
        public event Action<MemoryEcho> OnEchoDiscovered;

        // ── Epoch helper ─────────────────────────────────────────────────
        private static readonly DateTime EPOCH = new DateTime(2025, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        private long TodayEpochDay => (long)(DateTime.UtcNow - EPOCH).TotalDays;

        // ── Lifecycle ────────────────────────────────────────────────────
        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            LoadEchoes();
            SeedProphecyEchoes();
            RecordInstallDate();
            _fossilsFound = PlayerPrefs.GetInt(PREF_FOSSIL_COUNT, 0);
        }

        // ── Recording ────────────────────────────────────────────────────

        /// <summary>
        /// Called by realm controllers when a ritual completes. Records a memory echo
        /// for the current player and queues it for Firebase upload.
        /// </summary>
        public void RecordRitualEcho(string realmId, string ritualName, string message = "")
        {
            long today = TodayEpochDay;
            bool isFounder = today <= 7;   // first week after EPOCH launch

            int seekerNum = PlayerPrefs.GetInt("Profile_SeekerId", UnityEngine.Random.Range(10000, 99999));

            MemoryEcho echo = new MemoryEcho
            {
                seekerTag       = $"Seeker #{seekerNum}",
                realmId         = realmId,
                ritualName      = ritualName,
                optionalMessage = message,
                epochDayBuried  = today,
                isFossilized    = false,
                isFounder       = isFounder,
                isProphecy      = false
            };

            _localEchoes.Add(echo);
            PersistEchoes();

            // Queue for Firebase backend
            FirebaseManager.Instance?.SaveData(
                "ritualCompletions",
                $"{seekerNum}_{today}_{ritualName}",
                new
                {
                    seeker    = echo.seekerTag,
                    realm     = realmId,
                    ritual    = ritualName,
                    message   = message,
                    day       = today,
                    founder   = isFounder
                }
            );

            Debug.Log($"[EchoArchive] Recorded echo: {realmId}/{ritualName} day={today} founder={isFounder}");
        }

        // ── Discovery ────────────────────────────────────────────────────

        /// <summary>
        /// Randomly surface one echo for the player to discover.
        /// Biased toward older (rarer) fossils.
        /// </summary>
        public MemoryEcho DiscoverRandomFossil()
        {
            if (_localEchoes.Count == 0) return null;

            // Weight toward ancient / founder / prophecy types
            MemoryEcho chosen = WeightedPick();
            if (chosen == null) return null;

            FossilType type = GetFossilType(chosen);
            _fossilsFound++;
            PlayerPrefs.SetInt(PREF_FOSSIL_COUNT, _fossilsFound);

            // Achievements
            if (_fossilsFound >= 100)
                AchievementManager.Instance?.UnlockAchievement("archaeologist");

            if (type == FossilType.Founder)
                AchievementManager.Instance?.UnlockAchievement("founder_echo_discovered");

            if (type == FossilType.Ancient || type == FossilType.Founder)
                AchievementManager.Instance?.UnlockAchievement("time_traveler");

            OnEchoDiscovered?.Invoke(chosen);
            return chosen;
        }

        /// <summary>Returns the fossil classification for a given echo.</summary>
        public FossilType GetFossilType(MemoryEcho echo)
        {
            if (echo.isProphecy) return FossilType.Prophecy;
            if (echo.isFounder)  return FossilType.Founder;

            long age = TodayEpochDay - echo.epochDayBuried;
            if (age < FRESH_MAX)   return FossilType.Fresh;
            if (age < AGED_MAX)    return FossilType.Aged;
            if (age < FOSSIL_MAX)  return FossilType.Fossil;
            return FossilType.Ancient;
        }

        public string DescribeAge(MemoryEcho echo)
        {
            long age = TodayEpochDay - echo.epochDayBuried;
            if (age <= 0)   return "moments ago";
            if (age == 1)   return "yesterday";
            if (age < 7)    return $"{age} days ago";
            if (age < 30)   return $"{age / 7} weeks ago";
            if (age < 365)  return $"{age / 30} months ago";
            return $"{age / 365} years ago";
        }

        public int TotalFossilsFound  => _fossilsFound;
        public int TotalEchoes        => _localEchoes.Count;

        // ── Coroutine display ────────────────────────────────────────────

        /// <summary>
        /// Full immersive fossil-discovery sequence with HUD notification.
        /// Safe to call from any MonoBehaviour via StartCoroutine.
        /// </summary>
        public IEnumerator PlayDiscoverySequence()
        {
            MemoryEcho echo = DiscoverRandomFossil();
            if (echo == null) yield break;

            FossilType type = GetFossilType(echo);
            string rarity   = type switch
            {
                FossilType.Prophecy => "✦ PROPHECY",
                FossilType.Founder  => "⚡ FOUNDER",
                FossilType.Ancient  => "★ ANCIENT",
                FossilType.Fossil   => "◈ FOSSIL",
                FossilType.Aged     => "◇ AGED",
                _                   => "○ FRESH"
            };

            string msg = echo.isProphecy
                ? $"A prophecy stirs: \"{echo.optionalMessage}\""
                : $"{rarity} echo — {echo.seekerTag} • {echo.ritualName} in {echo.realmId} • {DescribeAge(echo)}";

            HUDManager.Instance?.ShowNotification(msg, HUDManager.NotificationType.Info);
            yield return new WaitForSeconds(4f);
        }

        // ── Private helpers ───────────────────────────────────────────────

        private MemoryEcho WeightedPick()
        {
            // Probability: Prophecy 5%, Founder 15%, Ancient 25%, rest shared
            float roll = UnityEngine.Random.value;
            List<MemoryEcho> prophecy = _localEchoes.FindAll(e => e.isProphecy);
            List<MemoryEcho> founder  = _localEchoes.FindAll(e => e.isFounder && !e.isProphecy);
            List<MemoryEcho> ancient  = _localEchoes.FindAll(e => !e.isProphecy && !e.isFounder && GetFossilType(e) == FossilType.Ancient);
            List<MemoryEcho> rest     = _localEchoes.FindAll(e => !e.isProphecy && !e.isFounder && GetFossilType(e) != FossilType.Ancient);

            if (roll < 0.05f && prophecy.Count > 0) return prophecy[UnityEngine.Random.Range(0, prophecy.Count)];
            if (roll < 0.20f && founder.Count  > 0) return founder[UnityEngine.Random.Range(0, founder.Count)];
            if (roll < 0.45f && ancient.Count  > 0) return ancient[UnityEngine.Random.Range(0, ancient.Count)];
            if (rest.Count > 0) return rest[UnityEngine.Random.Range(0, rest.Count)];
            return _localEchoes[UnityEngine.Random.Range(0, _localEchoes.Count)];
        }

        private void RecordInstallDate()
        {
            if (!PlayerPrefs.HasKey(PREF_INSTALL_DATE))
                PlayerPrefs.SetString(PREF_INSTALL_DATE, TodayEpochDay.ToString());
        }

        private void SeedProphecyEchoes()
        {
            // Only seed once per install
            if (PlayerPrefs.HasKey("EchoArchive_PropheciesSeeded")) return;

            foreach (string hint in PROPHECY_HINTS)
            {
                _localEchoes.Add(new MemoryEcho
                {
                    seekerTag       = "The Cosmos",
                    realmId         = "void",
                    ritualName      = "Prophecy",
                    optionalMessage = hint,
                    epochDayBuried  = -999,
                    isFossilized    = true,
                    isFounder       = false,
                    isProphecy      = true
                });
            }
            PlayerPrefs.SetInt("EchoArchive_PropheciesSeeded", 1);
            PersistEchoes();
        }

        // ── Persistence ───────────────────────────────────────────────────

        private void PersistEchoes()
        {
            PlayerPrefs.SetInt(PREF_ECHO_COUNT, _localEchoes.Count);
            for (int i = 0; i < _localEchoes.Count; i++)
            {
                PlayerPrefs.SetString(PREF_ECHO_PREFIX + i, JsonUtility.ToJson(_localEchoes[i]));
            }
            PlayerPrefs.Save();
        }

        private void LoadEchoes()
        {
            _localEchoes.Clear();
            int count = PlayerPrefs.GetInt(PREF_ECHO_COUNT, 0);
            for (int i = 0; i < count; i++)
            {
                string json = PlayerPrefs.GetString(PREF_ECHO_PREFIX + i, "");
                if (!string.IsNullOrEmpty(json))
                {
                    string trimmed = json.TrimStart();
                    if (!trimmed.StartsWith("{"))
                    {
                        Debug.LogWarning($"[EchoArchive] Skipping malformed echo payload at index {i}.");
                        continue;
                    }

                    try
                    {
                        MemoryEcho echo = JsonUtility.FromJson<MemoryEcho>(json);
                        if (echo != null) _localEchoes.Add(echo);
                    }
                    catch (Exception e)
                    {
                        Debug.LogWarning($"[EchoArchive] Failed to parse echo at index {i}: {e.Message}");
                    }
                }
            }
        }
    }
}
