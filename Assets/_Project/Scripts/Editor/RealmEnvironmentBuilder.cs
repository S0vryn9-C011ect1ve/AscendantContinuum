using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.SceneManagement;
#endif

#if UNITY_EDITOR
namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Editor window that block-outs ProBuilder geometry for each realm.
    /// Open via: Tools > Ascendant Continuum > Realm Environment Builder
    /// Select a realm and click "Build Block-Out" to generate geometry in the
    /// active scene. Run again after clearing to regenerate.
    /// </summary>
    public class RealmEnvironmentBuilder : EditorWindow
    {
        // ------------------------------------------------------------------ //
        //  Data
        // ------------------------------------------------------------------ //

        private enum RealmType { Emberforge, DawnCitadel, EchoFields, LanternAscension, Verdant }

        private RealmType _selectedRealm = RealmType.Emberforge;
        private bool _clearExisting      = true;
        private Vector2 _scroll;

        private static readonly Dictionary<RealmType, string> RealmRootNames = new()
        {
            { RealmType.Emberforge,       "ENV_Emberforge"       },
            { RealmType.DawnCitadel,      "ENV_DawnCitadel"      },
            { RealmType.EchoFields,       "ENV_EchoFields"       },
            { RealmType.LanternAscension, "ENV_LanternAscension" },
            { RealmType.Verdant,          "ENV_Verdant"          },
        };

        private static readonly Dictionary<RealmType, string> RealmScenePaths = new()
        {
            { RealmType.Emberforge,       "Assets/_Project/Scenes/Realms/Realm_Emberforge.unity" },
            { RealmType.DawnCitadel,      "Assets/_Project/Scenes/Realms/Realm_DawnCitadel.unity" },
            { RealmType.EchoFields,       "Assets/_Project/Scenes/Realms/Realm_EchoFields.unity" },
            { RealmType.LanternAscension, "Assets/_Project/Scenes/Realms/Realm_LanternAscension.unity" },
            { RealmType.Verdant,          "Assets/_Project/Scenes/Realms/Realm_Verdant.unity" },
        };

        // ------------------------------------------------------------------ //
        //  Window
        // ------------------------------------------------------------------ //

        [MenuItem("Tools/Ascendant Continuum/Realm Environment Builder")]
        public static void Open() => GetWindow<RealmEnvironmentBuilder>("Realm Builder");

        private void OnGUI()
        {
            EditorGUILayout.Space(6);
            GUILayout.Label("Realm Environment Builder", EditorStyles.boldLabel);

            EditorGUILayout.HelpBox(
                "Generates a ProBuilder block-out for the selected realm in the active scene.\n" +
                "Geometry is parented under a root GameObject (e.g. ENV_Emberforge).\n" +
                "Refine shapes in the scene — this is a starting layout only.",
                MessageType.Info);
            EditorGUILayout.Space(4);

            _selectedRealm = (RealmType)EditorGUILayout.EnumPopup("Realm", _selectedRealm);
            _clearExisting = EditorGUILayout.Toggle("Clear Existing Root First", _clearExisting);

            EditorGUILayout.Space(8);

            if (GUILayout.Button("Build Block-Out", GUILayout.Height(36)))
                Build(_selectedRealm, _clearExisting);

            EditorGUILayout.Space(6);

            if (GUILayout.Button("Open Selected Realm Scene", GUILayout.Height(28)))
                OpenRealmScene(_selectedRealm);

            if (GUILayout.Button("Isolate Selected ENV Root", GUILayout.Height(24)))
                IsolateRealmRoot(_selectedRealm);

            EditorGUILayout.Space(12);
            GUILayout.Label("All Realms", EditorStyles.boldLabel);
            if (GUILayout.Button("Build ALL Realms", GUILayout.Height(28)))
            {
                foreach (RealmType r in System.Enum.GetValues(typeof(RealmType)))
                    Build(r, _clearExisting);
            }
        }

        // ------------------------------------------------------------------ //
        //  Build dispatcher
        // ------------------------------------------------------------------ //

    private static void Build(RealmType realm, bool clearExisting)
        {
            string rootName = RealmRootNames[realm];

            if (clearExisting)
            {
                var existing = GameObject.Find(rootName);
                if (existing != null)
                {
                    Undo.DestroyObjectImmediate(existing);
                    Debug.Log($"[RealmBuilder] Cleared existing: {rootName}");
                }
            }

            GameObject root = new GameObject(rootName);
            Undo.RegisterCreatedObjectUndo(root, $"Build {rootName}");

            switch (realm)
            {
                case RealmType.Emberforge:       BuildEmberforge(root);       break;
                case RealmType.DawnCitadel:      BuildDawnCitadel(root);      break;
                case RealmType.EchoFields:       BuildEchoFields(root);       break;
                case RealmType.LanternAscension: BuildLanternAscension(root); break;
                case RealmType.Verdant:          BuildVerdant(root);          break;
            }

            Selection.activeGameObject = root;
            SceneView.lastActiveSceneView?.FrameSelected();
            Debug.Log($"[RealmBuilder] Built block-out: {rootName}");
        }

        private static void OpenRealmScene(RealmType realm)
        {
            if (!RealmScenePaths.TryGetValue(realm, out string scenePath) || string.IsNullOrWhiteSpace(scenePath))
            {
                Debug.LogError($"[RealmBuilder] No scene path configured for {realm}");
                return;
            }

            if (!System.IO.File.Exists(scenePath))
            {
                Debug.LogError($"[RealmBuilder] Realm scene file not found: {scenePath}");
                return;
            }

            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
            {
                return;
            }

            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[RealmBuilder] Opened scene: {scenePath}");
        }

        private static void IsolateRealmRoot(RealmType realm)
        {
            foreach (var kvp in RealmRootNames)
            {
                GameObject root = GameObject.Find(kvp.Value);
                if (root != null)
                {
                    root.SetActive(kvp.Key == realm);
                }
            }

            string selectedRootName = RealmRootNames[realm];
            GameObject selectedRoot = GameObject.Find(selectedRootName);
            if (selectedRoot != null)
            {
                Selection.activeGameObject = selectedRoot;
                SceneView.lastActiveSceneView?.FrameSelected();
                Debug.Log($"[RealmBuilder] Isolated root: {selectedRootName}");
            }
            else
            {
                Debug.LogWarning($"[RealmBuilder] Could not find root to isolate: {selectedRootName}");
            }
        }

        // ================================================================== //
        //  Realm Layouts
        // ================================================================== //

        //  EMBERFORGE — forge floor + lava channels + anvil platform + pillars
        private static void BuildEmberforge(GameObject root)
        {
            // Main forge floor
            CreateBox(root, "Floor",         new Vector3(0,  -0.5f,  0), new Vector3(30, 1, 30));
            // Raised central anvil platform
            CreateBox(root, "AnvilPlatform", new Vector3(0,   0.5f,  0), new Vector3( 8, 2,  8));
            // Lava channels (thin recessed strips)
            CreateBox(root, "LavaChannel_N", new Vector3(0,  -0.3f, 10), new Vector3(30, 0.4f, 2));
            CreateBox(root, "LavaChannel_E", new Vector3(10, -0.3f,  0), new Vector3( 2, 0.4f,30));
            // Outer walls
            CreateBox(root, "Wall_N",  new Vector3( 0,  3, 15), new Vector3(30,  6, 1));
            CreateBox(root, "Wall_S",  new Vector3( 0,  3,-15), new Vector3(30,  6, 1));
            CreateBox(root, "Wall_E",  new Vector3(15,  3,  0), new Vector3( 1,  6,30));
            CreateBox(root, "Wall_W",  new Vector3(-15, 3,  0), new Vector3( 1,  6,30));
            // Forge pillars
            CreateCylinder(root, "Pillar_NE", new Vector3( 8, 4,  8), 0.6f, 10);
            CreateCylinder(root, "Pillar_NW", new Vector3(-8, 4,  8), 0.6f, 10);
            CreateCylinder(root, "Pillar_SE", new Vector3( 8, 4, -8), 0.6f, 10);
            CreateCylinder(root, "Pillar_SW", new Vector3(-8, 4, -8), 0.6f, 10);
        }

        //  DAWN CITADEL — courtyard + archway gate + towers + stairs
        private static void BuildDawnCitadel(GameObject root)
        {
            CreateBox(root, "Courtyard",       new Vector3(0, -0.5f,  0), new Vector3(40,  1, 35));
            CreateBox(root, "Wall_N",          new Vector3(0,  4, 17),    new Vector3(40,  8,  2));
            CreateBox(root, "Wall_S",          new Vector3(0,  4,-17),    new Vector3(40,  8,  2));
            CreateBox(root, "Wall_E",          new Vector3(20, 4,  0),    new Vector3( 2,  8, 35));
            CreateBox(root, "Wall_W",          new Vector3(-20,4,  0),    new Vector3( 2,  8, 35));

            // Castle gate arch pillars
            CreateBox(root, "GatePillar_L",    new Vector3(-4, 5, 17),   new Vector3(4, 10,  3));
            CreateBox(root, "GatePillar_R",    new Vector3( 4, 5, 17),   new Vector3(4, 10,  3));
            CreateBox(root, "GateLintel",      new Vector3( 0,10, 17),   new Vector3(8,  2,  3));

            // Corner towers
            CreateCylinder(root, "Tower_NE",   new Vector3( 20, 8,  17), 2.5f, 16);
            CreateCylinder(root, "Tower_NW",   new Vector3(-20, 8,  17), 2.5f, 16);
            CreateCylinder(root, "Tower_SE",   new Vector3( 20, 8, -17), 2.5f, 16);
            CreateCylinder(root, "Tower_SW",   new Vector3(-20, 8, -17), 2.5f, 16);

            // Grand staircase
            for (int i = 0; i < 5; i++)
                CreateBox(root, $"Stair_{i}",  new Vector3(0, i*0.35f - 0.25f, 15 - i*1.2f),
                          new Vector3(8, 0.7f, 1.2f));
        }

        //  ECHO FIELDS — open starfield plain + central observatory ring + telescope platform
        private static void BuildEchoFields(GameObject root)
        {
            CreateBox(root, "Ground",              new Vector3(0, -0.5f,  0), new Vector3(60, 1, 60));
            CreateCylinder(root, "ObsRing_Outer",  new Vector3(0,  0.2f,  0), 10f, 2);
            CreateCylinder(root, "ObsRing_Inner",  new Vector3(0,  0.2f,  0),  8f, 2); // hollow effect via offset
            CreateBox(root,      "TelescopePlatform", new Vector3(0, 1.5f, 0), new Vector3(4, 3, 4));

            // Stone marker columns around outside
            for (int i = 0; i < 12; i++)
            {
                float angle = i * (360f / 12f) * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(angle) * 22, 1.5f, Mathf.Cos(angle) * 22);
                CreateBox(root, $"Obelisk_{i}", pos, new Vector3(0.8f, 3f, 0.8f));
            }
        }

        //  LANTERN ASCENSION — ascending stepped platforms + lantern post positions
        private static void BuildLanternAscension(GameObject root)
        {
            CreateBox(root, "Ground",  new Vector3(0, -0.5f,  0), new Vector3(30, 1, 30));

            // Ascending platforms
            float[] yLevels = { 0, 2.5f, 5f, 7.5f, 11f };
            float[] sizes   = { 14, 11,   8,   6,    4  };
            for (int i = 0; i < yLevels.Length; i++)
                CreateBox(root, $"Platform_{i}", new Vector3(0, yLevels[i] + 0.5f, 0),
                          new Vector3(sizes[i], 1f, sizes[i]));

            // Connecting ramp / staircase on each side
            for (int i = 0; i < yLevels.Length - 1; i++)
            {
                float midY = (yLevels[i] + yLevels[i+1]) * 0.5f;
                float sz   = (sizes[i] + sizes[i+1]) * 0.5f * 0.5f;
                CreateBox(root, $"Ramp_{i}", new Vector3(sz, midY, 0),
                          new Vector3(2f, yLevels[i+1] - yLevels[i], 2f));
            }

            // Lantern post slots (empty markers)
            for (int i = 0; i < 8; i++)
            {
                float angle = i * 45f * Mathf.Deg2Rad;
                Vector3 pos = new Vector3(Mathf.Sin(angle)*12, 1f, Mathf.Cos(angle)*12);
                CreateCylinder(root, $"LanternPost_{i}", pos, 0.15f, 3.5f);
            }
        }

        //  VERDANT — rolling garden paths + mushroom caps + root arch
        private static void BuildVerdant(GameObject root)
        {
            CreateBox(root, "Garden_Floor",    new Vector3(0, -0.5f,  0), new Vector3(40, 1, 40));

            // Meandering path (approximated with boxes)
            CreateBox(root, "Path_0",  new Vector3( 0,  0.05f,  12), new Vector3(3,  0.1f, 14));
            CreateBox(root, "Path_1",  new Vector3( 6,  0.05f,   6), new Vector3(14, 0.1f,  3));
            CreateBox(root, "Path_2",  new Vector3( 6,  0.05f,  -6), new Vector3( 3, 0.1f, 14));
            CreateBox(root, "Path_3",  new Vector3(-6,  0.05f,  -6), new Vector3(14, 0.1f,  3));

            // Mushroom cap platforms
            int mushroomCount = 6;
            Vector3[] mushroomPos =
            {
                new(-8, 1.5f,  8), new(8,   2f,  6), new(-6,  1f, -4),
                new( 5, 2.5f, -8), new(-10, 1f, -2), new( 10, 2f, -4)
            };
            float[] mushroomR  = { 3.0f, 2.5f, 2.0f, 3.5f, 2.0f, 2.8f };
            for (int i = 0; i < mushroomCount; i++)
                CreateCylinder(root, $"Mush_{i}", mushroomPos[i], mushroomR[i], 0.6f);

            // Root arch at entrance
            CreateBox(root, "RootArch_L",   new Vector3(-2, 3,  18), new Vector3(1.2f, 6, 1.5f));
            CreateBox(root, "RootArch_R",   new Vector3( 2, 3,  18), new Vector3(1.2f, 6, 1.5f));
            CreateBox(root, "RootArch_Top", new Vector3( 0, 6.5f,18),new Vector3(4.5f, 1.5f, 1.5f));

            // Outer hedge walls
            CreateBox(root, "Hedge_N",  new Vector3( 0, 2,  20), new Vector3(40, 4, 1.5f));
            CreateBox(root, "Hedge_S",  new Vector3( 0, 2, -20), new Vector3(40, 4, 1.5f));
            CreateBox(root, "Hedge_E",  new Vector3( 20,2,   0), new Vector3(1.5f, 4, 40));
            CreateBox(root, "Hedge_W",  new Vector3(-20,2,   0), new Vector3(1.5f, 4, 40));
        }

        // ================================================================== //
        //  Geometry Helpers (ProBuilder-free fallback)
        // ================================================================== //

        private static GameObject CreateBox(GameObject parent, string name,
            Vector3 position, Vector3 size)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = name;
            go.transform.parent = parent.transform;
            go.transform.localPosition = position;
            go.transform.localScale = size;
            go.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            return go;
        }

        private static GameObject CreateCylinder(GameObject parent, string name,
            Vector3 position, float radius, float height)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            go.name = name;
            go.transform.parent = parent.transform;
            go.transform.localPosition = position;
            go.transform.localScale = new Vector3(radius * 2f, height * 0.5f, radius * 2f);
            go.transform.localRotation = Quaternion.identity;
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            return go;
        }
        
    }
}
#endif // UNITY_EDITOR
