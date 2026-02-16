using UnityEditor;
using UnityEngine;
using System.IO;

namespace AscendantContinuum.Build
{
    /// <summary>
    /// Unity Editor build automation for The Ascendant Continuum.
    /// Provides methods for building Android, iOS, and WebGL versions.
    /// </summary>
    public static class BuildScript
    {
        private const string COMPANY_NAME = "AscendantContinuum";
        private const string PRODUCT_NAME = "The Ascendant Continuum";
        private const string BUNDLE_IDENTIFIER = "com.ascendantcontinuum.game";
        
        private static string BuildPath => Path.Combine(Directory.GetCurrentDirectory(), "Builds");
        
        [MenuItem("Build/Build All Platforms")]
        public static void BuildAll()
        {
            BuildAndroid();
            BuildiOS();
            BuildWebGL();
        }
        
        #region Android Build
        
        [MenuItem("Build/Build Android")]
        public static void BuildAndroid()
        {
            Debug.Log("Starting Android build...");
            
            // Setup player settings
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, BUNDLE_IDENTIFIER);
            
            // Android specific settings
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevel33;
            EditorUserBuildSettings.buildAppBundle = false; // Build APK
            
            // Graphics settings
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.Android.blitType = AndroidBlitType.Auto;
            PlayerSettings.gpuSkinning = true;
            
            // Build options
            string outputPath = Path.Combine(BuildPath, "Android", "AscendantContinuum.apk");
            EnsureDirectoryExists(Path.GetDirectoryName(outputPath));
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = outputPath,
                target = BuildTarget.Android,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };
            
            // Build
            var report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"Android build succeeded! Size: {report.summary.totalSize} bytes");
                Debug.Log($"Output: {outputPath}");
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                Debug.LogError($"Android build failed! {report.summary.totalErrors} errors");
            }
        }
        
        #endregion
        
        #region iOS Build
        
        [MenuItem("Build/Build iOS")]
        public static void BuildiOS()
        {
            Debug.Log("Starting iOS build...");
            
            // Setup player settings
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.iOS, BUNDLE_IDENTIFIER);
            
            // iOS specific settings
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            PlayerSettings.iOS.sdkVersion = iOSSdkVersion.DeviceSDK;
            PlayerSettings.iOS.targetDevice = iOSTargetDevice.iPhoneAndiPad;
            
            // Graphics settings
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.iOS.hideHomeButton = false;
            
            // Build options
            string outputPath = Path.Combine(BuildPath, "iOS");
            EnsureDirectoryExists(outputPath);
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = outputPath,
                target = BuildTarget.iOS,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };
            
            // Build
            var report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log("iOS Xcode project generated successfully!");
                Debug.Log($"Output: {outputPath}");
                Debug.Log("Open the Xcode project and build for final .ipa");
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                Debug.LogError($"iOS build failed! {report.summary.totalErrors} errors");
            }
        }
        
        #endregion
        
        #region WebGL Build
        
        [MenuItem("Build/Build WebGL")]
        public static void BuildWebGL()
        {
            Debug.Log("Starting WebGL build...");
            
            // Setup player settings
            PlayerSettings.companyName = COMPANY_NAME;
            PlayerSettings.productName = PRODUCT_NAME;
            
            // WebGL specific settings
            PlayerSettings.WebGL.memorySize = 256; // MB
            PlayerSettings.WebGL.exceptionSupport = WebGLExceptionSupport.FullWithStacktrace;
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.template = "PROJECT:Default";
            
            // Graphics settings
            PlayerSettings.colorSpace = ColorSpace.Linear;
            
            // Build options
            string outputPath = Path.Combine(BuildPath, "WebGL");
            EnsureDirectoryExists(outputPath);
            
            BuildPlayerOptions buildOptions = new BuildPlayerOptions
            {
                scenes = GetScenePaths(),
                locationPathName = outputPath,
                target = BuildTarget.WebGL,
                options = EditorUserBuildSettings.development ? BuildOptions.Development : BuildOptions.None
            };
            
            // Build
            var report = BuildPipeline.BuildPlayer(buildOptions);
            
            if (report.summary.result == UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                Debug.Log($"WebGL build succeeded! Size: {report.summary.totalSize} bytes");
                Debug.Log($"Output: {outputPath}");
                EditorUtility.RevealInFinder(outputPath);
            }
            else
            {
                Debug.LogError($"WebGL build failed! {report.summary.totalErrors} errors");
            }
        }
        
        #endregion
        
        #region Helper Methods
        
        private static string[] GetScenePaths()
        {
            // Get all scenes from build settings
            string[] scenePaths = new string[EditorBuildSettings.scenes.Length];
            for (int i = 0; i < EditorBuildSettings.scenes.Length; i++)
            {
                scenePaths[i] = EditorBuildSettings.scenes[i].path;
            }
            
            if (scenePaths.Length == 0)
            {
                Debug.LogWarning("No scenes found in Build Settings! Add scenes to Build Settings first.");
            }
            
            return scenePaths;
        }
        
        private static void EnsureDirectoryExists(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                Debug.Log($"Created directory: {path}");
            }
        }
        
        [MenuItem("Build/Open Build Folder")]
        public static void OpenBuildFolder()
        {
            EnsureDirectoryExists(BuildPath);
            EditorUtility.RevealInFinder(BuildPath);
        }
        
        [MenuItem("Build/Clean Build Folder")]
        public static void CleanBuildFolder()
        {
            if (Directory.Exists(BuildPath))
            {
                Directory.Delete(BuildPath, true);
                Debug.Log("Build folder cleaned!");
            }
            
            EnsureDirectoryExists(BuildPath);
        }
        
        #endregion
        
        #region Version Management
        
        [MenuItem("Build/Increment Version (Patch)")]
        public static void IncrementPatchVersion()
        {
            string currentVersion = PlayerSettings.bundleVersion;
            string[] parts = currentVersion.Split('.');
            
            if (parts.Length == 3)
            {
                int patch = int.Parse(parts[2]) + 1;
                string newVersion = $"{parts[0]}.{parts[1]}.{patch}";
                PlayerSettings.bundleVersion = newVersion;
                
                // Increment Android build number
                PlayerSettings.Android.bundleVersionCode++;
                
                // Increment iOS build number
                PlayerSettings.iOS.buildNumber = PlayerSettings.Android.bundleVersionCode.ToString();
                
                Debug.Log($"Version updated: {currentVersion} → {newVersion}");
                Debug.Log($"Build number: {PlayerSettings.Android.bundleVersionCode}");
            }
        }
        
        [MenuItem("Build/Increment Version (Minor)")]
        public static void IncrementMinorVersion()
        {
            string currentVersion = PlayerSettings.bundleVersion;
            string[] parts = currentVersion.Split('.');
            
            if (parts.Length == 3)
            {
                int minor = int.Parse(parts[1]) + 1;
                string newVersion = $"{parts[0]}.{minor}.0";
                PlayerSettings.bundleVersion = newVersion;
                
                Debug.Log($"Version updated: {currentVersion} → {newVersion}");
            }
        }
        
        [MenuItem("Build/Increment Version (Major)")]
        public static void IncrementMajorVersion()
        {
            string currentVersion = PlayerSettings.bundleVersion;
            string[] parts = currentVersion.Split('.');
            
            if (parts.Length >= 1)
            {
                int major = int.Parse(parts[0]) + 1;
                string newVersion = $"{major}.0.0";
                PlayerSettings.bundleVersion = newVersion;
                
                Debug.Log($"Version updated: {currentVersion} → {newVersion}");
            }
        }
        
        #endregion
        
        #region Optimization
        
        [MenuItem("Build/Optimize Project Settings")]
        public static void OptimizeProjectSettings()
        {
            Debug.Log("Optimizing project settings...");
            
            // Graphics settings
            PlayerSettings.colorSpace = ColorSpace.Linear;
            PlayerSettings.gpuSkinning = true;
            PlayerSettings.graphicsJobs = true;
            
            // Performance settings
            PlayerSettings.MTRendering = true;
            
            // Stripping settings
            PlayerSettings.stripEngineCode = true;
            PlayerSettings.managedStrippingLevel = ManagedStrippingLevel.High;
            
            // Android optimizations
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel22;
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64 | AndroidArchitecture.ARMv7;
            
            // iOS optimizations
            PlayerSettings.iOS.targetOSVersionString = "12.0";
            
            Debug.Log("✓ Project settings optimized!");
        }
        
        #endregion
    }
}
