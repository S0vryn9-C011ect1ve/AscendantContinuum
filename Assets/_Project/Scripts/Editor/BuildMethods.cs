using UnityEditor;
using UnityEditor.Build;
using UnityEngine;
using System.IO;

/// <summary>
/// Direct batch build methods in global namespace for reliable batch mode discovery.
/// </summary>
public static class BuildMethods
{
    private const string COMPANY_NAME = "AscendantContinuum";
    private const string PRODUCT_NAME = "The Ascendant Continuum";
    private const string BUNDLE_IDENTIFIER = "com.ascendantcontinuum.game";

    private static string BuildPath => Path.Combine(Directory.GetCurrentDirectory(), "Builds");

    [UnityEditor.MenuItem("Build/WebGL", priority = 100)]
    /// <summary>Build WebGL</summary>
    public static void BuildWebGL()
    {
        Debug.Log("[BuildMethods] Starting WebGL build...");

        try
        {
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.SetScriptingBackend(NamedBuildTarget.WebGL, ScriptingImplementation.IL2CPP);
            PlayerSettings.SetIl2CppCompilerConfiguration(NamedBuildTarget.WebGL, Il2CppCompilerConfiguration.Debug);
            EditorUserBuildSettings.development = true;

            string outputPath = Path.Combine(BuildPath, "WebGL");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            // Get scene paths from build settings
            var scenePaths = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenePaths[i] = EditorBuildSettings.scenes[i].path;
                Debug.Log($"[BuildMethods] Scene {i}: {scenePaths[i]}");
            }

            if (scenePaths.Length == 0)
            {
                Debug.LogWarning("[BuildMethods] No scenes in Build Settings, build will fail");
            }

            var buildOptions = new BuildPlayerOptions
            {
                scenes = scenePaths,
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };

            Debug.Log($"[BuildMethods] Building {scenePaths.Length} scenes to {outputPath}");
            var report = BuildPipeline.BuildPlayer(buildOptions);

            Debug.Log($"[BuildMethods] Build completed with result: {report.summary.result}");
            Debug.Log($"[BuildMethods] Total errors: {report.summary.totalErrors}");
            Debug.Log($"[BuildMethods] Total warnings: {report.summary.totalWarnings}");

            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"[BuildMethods] WebGL build succeeded! Size: {report.summary.totalSize} bytes");
            }
            else
            {
                string message = $"WebGL build failed! {report.summary.totalErrors} errors";
                Debug.LogError(message);
                throw new UnityEditor.Build.BuildFailedException(message);
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
            throw;
        }
    }

    /// <summary>Build Android</summary>
    public static void BuildAndroid()
    {
        Debug.Log("[BuildMethods] Starting Android build...");

        try
        {
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel25;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            EditorUserBuildSettings.buildAppBundle = false;

            string outputPath = Path.Combine(BuildPath, "Android");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = Path.Combine(outputPath, "AscendantContinuum.apk"),
                target = BuildTarget.Android,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new UnityEditor.Build.BuildFailedException($"Android build failed! {report.summary.totalErrors} errors");
            }

            Debug.Log("[BuildMethods] Android build succeeded!");
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    /// <summary>Build iOS</summary>
    public static void BuildiOS()
    {
        Debug.Log("[BuildMethods] Starting iOS build...");

        try
        {
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.iOS.targetOSVersionString = "14.0";

            string outputPath = Path.Combine(BuildPath, "iOS");
            if (!Directory.Exists(outputPath))
                Directory.CreateDirectory(outputPath);

            var buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };

            var report = BuildPipeline.BuildPlayer(buildOptions);

            if (report.summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                throw new UnityEditor.Build.BuildFailedException($"iOS build failed! {report.summary.totalErrors} errors");
            }

            Debug.Log("[BuildMethods] iOS build succeeded!");
        }
        catch (System.Exception ex)
        {
            Debug.LogException(ex);
        }
    }

    private static string[] GetScenePaths()
    {
        string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
        for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
        {
            scenePaths[i] = EditorBuildSettings.scenes[i].path;
        }
        return scenePaths;
    }
}
