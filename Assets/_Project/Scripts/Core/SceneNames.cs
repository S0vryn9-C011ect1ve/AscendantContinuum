namespace AscendantContinuum.Core
{
    /// <summary>
    /// Central registry of every scene name used in The Ascendant Continuum.
    /// All code that loads scenes must reference constants from this class
    /// rather than using raw string literals.
    /// </summary>
    public static class SceneNames
    {
        // ── Bootstrap ──────────────────────────────────────────────────────
        /// <summary>First scene loaded; creates all persistent managers then transitions to MainMenu.</summary>
        public const string Bootstrap = "Bootstrap";

        // ── Core ──────────────────────────────────────────────────────────
        public const string MainMenu   = "MainMenu";
        public const string Onboarding = "Onboarding";

        // ── Realms ────────────────────────────────────────────────────────
        public const string Emberforge      = "Realm_Emberforge";
        public const string Verdant         = "Realm_Verdant";
        public const string EchoFields      = "Realm_EchoFields";
        public const string DawnCitadel     = "Realm_DawnCitadel";
        public const string LanternAscension = "Realm_LanternAscension";

        // ── Helpers ───────────────────────────────────────────────────────

        /// <summary>Returns the scene name constant for a given realm identifier string.</summary>
        public static string FromRealmId(string realmId)
        {
            return realmId?.ToLowerInvariant() switch
            {
                "emberforge"           => Emberforge,
                "verdant"              => Verdant,
                "verdantsanctuary"     => Verdant,
                "verdant_sanctuary"    => Verdant,
                "echo"                 => EchoFields,
                "echofields"           => EchoFields,
                "echo_fields"          => EchoFields,
                "dawn"                 => DawnCitadel,
                "dawncitadel"          => DawnCitadel,
                "dawn_citadel"         => DawnCitadel,
                "lantern"              => LanternAscension,
                "lanternascension"     => LanternAscension,
                "lantern_ascension"    => LanternAscension,
                _                       => MainMenu
            };
        }

        /// <summary>Returns the realm identifier string for a given scene name constant.</summary>
        public static string ToRealmId(string sceneName)
        {
            return sceneName switch
            {
                Emberforge       => "emberforge",
                Verdant          => "verdant_sanctuary",
                EchoFields       => "echo_fields",
                DawnCitadel      => "dawn_citadel",
                LanternAscension => "lantern_ascension",
                _                => string.Empty
            };
        }

        /// <summary>All ordered build scenes; must match the Build Settings scene list.</summary>
        public static readonly string[] AllScenes =
        {
            Bootstrap,
            MainMenu,
            Onboarding,
            Emberforge,
            Verdant,
            EchoFields,
            DawnCitadel,
            LanternAscension
        };
    }
}
