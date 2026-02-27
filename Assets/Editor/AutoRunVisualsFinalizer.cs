using UnityEditor;

// This script runs once after the Editor domain reloads and triggers the
// VisualsSetupFinalizer if the expected scripting defines are not present.
[InitializeOnLoad]
internal static class AutoRunVisualsFinalizer
{
    static AutoRunVisualsFinalizer()
    {
        // DelayCall ensures Editor is ready
        EditorApplication.delayCall += RunIfNeeded;
    }

    private static void RunIfNeeded()
    {
        EditorApplication.delayCall -= RunIfNeeded;

        var defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(BuildTargetGroup.Standalone);
        bool hasVfx = defines.Contains("HAVE_VFX_GRAPH");
        bool hasPb  = defines.Contains("HAVE_PROBUILDER");
        bool hasCm  = defines.Contains("CINEMACHINE_3_0_OR_NEWER");

        if (hasVfx && hasPb && hasCm)
        {
            // All set — nothing to do.
            return;
        }

        // Try to execute the finalizer menu item (it will show messages if packages missing)
        if (EditorApplication.ExecuteMenuItem("Tools/Ascendant Continuum/Finalize Visual Setup"))
        {
            UnityEngine.Debug.Log("AutoRunVisualsFinalizer: Finalizer executed.");
        }
        else
        {
            UnityEngine.Debug.LogWarning("AutoRunVisualsFinalizer: Finalizer menu not found or failed to run.");
        }
    }
}
