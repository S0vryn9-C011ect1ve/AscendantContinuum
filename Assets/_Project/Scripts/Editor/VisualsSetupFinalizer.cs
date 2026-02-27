#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace AscendantContinuum.Editor
{
    /// <summary>
    /// Menu utility to verify required packages and ensure scripting defines are set.
    /// Run after exiting Safe Mode so Unity has downloaded packages.
    /// Tools -> Ascendant Continuum -> Finalize Visual Setup
    /// </summary>
    public static class VisualsSetupFinalizer
    {
        private static ListRequest _listRequest;

        [MenuItem("Tools/Ascendant Continuum/Finalize Visual Setup")]
        public static void RunFinalize()
        {
            Debug.Log("[VisualsSetupFinalizer] Starting package list request...");
            _listRequest = Client.List(true);
            EditorApplication.update += Poll;
        }

        private static void Poll()
        {
            if (_listRequest == null) return;
            if (!_listRequest.IsCompleted) return;

            EditorApplication.update -= Poll;

            if (_listRequest.Status == StatusCode.Success)
            {
                var pkgs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var p in _listRequest.Result)
                {
                    pkgs.Add(p.name);
                }

                bool vfx   = pkgs.Contains("com.unity.visualeffectgraph");
                bool pb    = pkgs.Contains("com.unity.probuilder");
                bool cm    = pkgs.Contains("com.unity.cinemachine");

                Debug.Log($"[VisualsSetupFinalizer] Packages found: VFX={vfx}, ProBuilder={pb}, Cinemachine={cm}");

                var symbols = new List<string>();
                if (vfx) symbols.Add("HAVE_VFX_GRAPH");
                if (pb)  symbols.Add("HAVE_PROBUILDER");
                if (cm)  symbols.Add("CINEMACHINE_3_0_OR_NEWER");

                if (symbols.Count == 0)
                {
                    EditorUtility.DisplayDialog("Finalize Visual Setup", "Required packages not yet installed. Please exit Safe Mode and allow Unity to finish importing packages, then run this command again.", "OK");
                    return;
                }

                // Apply defines to common build target groups
                var groups = new BuildTargetGroup[] { BuildTargetGroup.Standalone, BuildTargetGroup.Android, BuildTargetGroup.iOS, BuildTargetGroup.WebGL };
                foreach (var g in groups)
                {
                    try
                    {
                        var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(g);
                        var list = new List<string>(current.Split(new[] {';'}, StringSplitOptions.RemoveEmptyEntries));
                        foreach (var s in symbols)
                        {
                            if (!list.Contains(s)) list.Add(s);
                        }
                        var joined = string.Join(";", list);
                        PlayerSettings.SetScriptingDefineSymbolsForGroup(g, joined);
                        Debug.Log($"[VisualsSetupFinalizer] Set defines for {g}: {joined}");
                    }
                    catch (Exception ex)
                    {
                        Debug.LogWarning($"[VisualsSetupFinalizer] Unable to set defines for group {g}: {ex.Message}");
                    }
                }

                EditorUtility.DisplayDialog("Finalize Visual Setup", "Packages detected and scripting defines applied. You can now enable full features and remove any temporary guards if desired.", "OK");
            }
            else
            {
                Debug.LogError("[VisualsSetupFinalizer] Package list request failed: " + _listRequest.Error.message);
                EditorUtility.DisplayDialog("Finalize Visual Setup", "Failed to list packages: " + _listRequest.Error.message, "OK");
            }

            _listRequest = null;
        }
    }
}
#endif
