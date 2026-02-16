# ============================================================================
# Unity Build Script for The Ascendant Continuum
# Automates building for Android, iOS, and WebGL platforms
# ============================================================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("Android", "iOS", "WebGL", "All")]
    [string]$Platform = "All",
    
    [Parameter(Mandatory=$false)]
    [string]$UnityPath = "C:\Program Files\Unity\Hub\Editor\2022.3.18f1\Editor\Unity.exe",
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectPath = "D:\1-Ascendant Continuum Game",
    
    [Parameter(Mandatory=$false)]
    [string]$BuildPath = "D:\1-Ascendant Continuum Game\Builds",
    
    [Parameter(Mandatory=$false)]
    [ValidateSet("Development", "Release")]
    [string]$BuildType = "Development"
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "The Ascendant Continuum - Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Create build directories
$androidBuildPath = Join-Path $BuildPath "Android"
$iOSBuildPath = Join-Path $BuildPath "iOS"
$webGLBuildPath = Join-Path $BuildPath "WebGL"

New-Item -ItemType Directory -Force -Path $androidBuildPath | Out-Null
New-Item -ItemType Directory -Force -Path $iOSBuildPath | Out-Null
New-Item -ItemType Directory -Force -Path $webGLBuildPath | Out-Null

# Build flags
$developmentFlag = if ($BuildType -eq "Development") { "-development" } else { "" }

# ============================================================================
# ANDROID BUILD
# ============================================================================

function Build-Android {
    Write-Host "Building for Android..." -ForegroundColor Yellow
    
    $buildMethod = "AscendantContinuum.Build.BuildAndroid"
    $outputFile = Join-Path $androidBuildPath "AscendantContinuum.apk"
    
    $arguments = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$ProjectPath`"",
        "-buildTarget", "Android",
        "-executeMethod", $buildMethod,
        "-logFile", "build_android.log"
    )
    
    if ($BuildType -eq "Development") {
        $arguments += $developmentFlag
    }
    
    Write-Host "Starting Unity build process..."
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✓ Android build completed successfully!" -ForegroundColor Green
        Write-Host "Output: $outputFile"
    } else {
        Write-Host "✗ Android build failed! Check build_android.log for details." -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# iOS BUILD
# ============================================================================

function Build-iOS {
    Write-Host "Building for iOS..." -ForegroundColor Yellow
    
    $buildMethod = "AscendantContinuum.Build.BuildiOS"
    
    $arguments = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$ProjectPath`"",
        "-buildTarget", "iOS",
        "-executeMethod", $buildMethod,
        "-logFile", "build_ios.log"
    )
    
    if ($BuildType -eq "Development") {
        $arguments += $developmentFlag
    }
    
    Write-Host "Starting Unity build process..."
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✓ iOS Xcode project generated successfully!" -ForegroundColor Green
        Write-Host "Output: $iOSBuildPath"
        Write-Host "Note: Open the Xcode project and build from Xcode for final .ipa" -ForegroundColor Cyan
    } else {
        Write-Host "✗ iOS build failed! Check build_ios.log for details." -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# WEBGL BUILD
# ============================================================================

function Build-WebGL {
    Write-Host "Building for WebGL..." -ForegroundColor Yellow
    
    $buildMethod = "AscendantContinuum.Build.BuildWebGL"
    
    $arguments = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$ProjectPath`"",
        "-buildTarget", "WebGL",
        "-executeMethod", $buildMethod,
        "-logFile", "build_webgl.log"
    )
    
    if ($BuildType -eq "Development") {
        $arguments += $developmentFlag
    }
    
    Write-Host "Starting Unity build process..."
    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    
    if ($process.ExitCode -eq 0) {
        Write-Host "✓ WebGL build completed successfully!" -ForegroundColor Green
        Write-Host "Output: $webGLBuildPath"
    } else {
        Write-Host "✗ WebGL build failed! Check build_webgl.log for details." -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Host "Platform: $Platform" -ForegroundColor Cyan
Write-Host "Build Type: $BuildType" -ForegroundColor Cyan
Write-Host "Unity Path: $UnityPath" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Cyan
Write-Host "Build Output: $BuildPath" -ForegroundColor Cyan
Write-Host ""

# Verify Unity exists
if (!(Test-Path $UnityPath)) {
    Write-Host "✗ Unity not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Please install Unity 2022.3 LTS or update the UnityPath parameter." -ForegroundColor Yellow
    exit 1
}

# Execute builds
switch ($Platform) {
    "Android" {
        Build-Android
    }
    "iOS" {
        Build-iOS
    }
    "WebGL" {
        Build-WebGL
    }
    "All" {
        Build-Android
        Write-Host ""
        Build-iOS
        Write-Host ""
        Build-WebGL
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Build process completed!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
