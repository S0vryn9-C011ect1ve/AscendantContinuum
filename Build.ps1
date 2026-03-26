# ============================================================================
# Unity Build Script for The Ascendant Continuum
# Automates building for Android, iOS, and WebGL platforms
# ============================================================================

param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("Android", "iOS", "WebGL", "All")]
    [string]$Platform = "All",

    [Parameter(Mandatory = $false)]
    [string]$UnityPath = "",

    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = "",

    [Parameter(Mandatory = $false)]
    [string]$BuildPath = "",

    [Parameter(Mandatory = $false)]
    [ValidateSet("Development", "Release")]
    [string]$BuildType = "Development",

    [Parameter(Mandatory = $false)]
    [switch]$RunTestsBeforeBuild,

    [Parameter(Mandatory = $false)]
    [switch]$CleanBeeState,

    [Parameter(Mandatory = $false)]
    [switch]$SkipCleanBeeState,

    [Parameter(Mandatory = $false)]
    [switch]$SkipUnityProcessPrecheck,

    [Parameter(Mandatory = $false)]
    [int]$UnityBuildTimeoutSeconds = 5400
)

function Resolve-UnityPath {
    param(
        [string]$RequestedPath,
        [string]$WorkspacePath
    )

    if (![string]::IsNullOrWhiteSpace($RequestedPath)) {
        return $RequestedPath
    }

    $versionFile = Join-Path $WorkspacePath "ProjectSettings\ProjectVersion.txt"
    if (Test-Path $versionFile) {
        $versionLine = Get-Content $versionFile | Where-Object { $_ -like "m_EditorVersion:*" } | Select-Object -First 1
        if ($versionLine) {
            $unityVersion = ($versionLine -split ":", 2)[1].Trim()
            $candidate = "C:\Program Files\Unity\Hub\Editor\$unityVersion\Editor\Unity.exe"
            if (Test-Path $candidate) {
                return $candidate
            }
        }
    }

    # Last-resort: scan every installed editor and pick the newest
    $editorRoot = "C:\Program Files\Unity\Hub\Editor"
    if (Test-Path $editorRoot) {
        $newest = Get-ChildItem $editorRoot -Directory -ErrorAction SilentlyContinue |
            Sort-Object Name -Descending | Select-Object -First 1
        if ($newest) {
            $fallback = Join-Path $newest.FullName "Editor\Unity.exe"
            if (Test-Path $fallback) {
                Write-Warning "[Build] Using newest installed Unity as fallback: $($newest.Name)"
                return $fallback
            }
        }
    }

    # Absolute last resort (original hardcoded path kept for CI images that pin this version)
    return "C:\Program Files\Unity\Hub\Editor\2022.3.18f1\Editor\Unity.exe"
}

function Get-PlatformSupportFolderName {
    param([string]$TargetPlatform)

    switch ($TargetPlatform) {
        "WebGL" { return "WebGLSupport" }
        "Android" { return "AndroidPlayer" }
        "iOS" { return "iOSSupport" }
        default { return $null }
    }
}

function TryResolveUnityPathWithPlatformSupport {
    param(
        [string]$TargetPlatform,
        [string]$CurrentUnityPath
    )

    $requiredFolder = Get-PlatformSupportFolderName -TargetPlatform $TargetPlatform
    if ([string]::IsNullOrWhiteSpace($requiredFolder)) {
        return $CurrentUnityPath
    }

    $currentRoot = Get-UnityEditorRoot -UnityExecutablePath $CurrentUnityPath
    $currentSupportPath = Join-Path $currentRoot "Data\PlaybackEngines\$requiredFolder"
    if (Test-Path $currentSupportPath) {
        return $CurrentUnityPath
    }

    $editorBasePath = "C:\Program Files\Unity\Hub\Editor"
    if (!(Test-Path $editorBasePath)) {
        return $CurrentUnityPath
    }

    $candidates = @()
    Get-ChildItem $editorBasePath -Directory -ErrorAction SilentlyContinue | ForEach-Object {
        $unityExe = Join-Path $_.FullName "Editor\Unity.exe"
        $supportPath = Join-Path $_.FullName "Editor\Data\PlaybackEngines\$requiredFolder"

        if ((Test-Path $unityExe) -and (Test-Path $supportPath)) {
            $versionString = $_.Name
            $normalizedVersion = ($versionString -replace "-x86_64$", "")

            $versionObj = $null
            if ([System.Version]::TryParse(($normalizedVersion -replace "f\d+$", ".0"), [ref]$versionObj)) {
                $candidates += [PSCustomObject]@{
                    Path    = $unityExe
                    Name    = $_.Name
                    Version = $versionObj
                }
            }
            else {
                $candidates += [PSCustomObject]@{
                    Path    = $unityExe
                    Name    = $_.Name
                    Version = [System.Version]::new(0, 0, 0, 0)
                }
            }
        }
    }

    if ($candidates.Count -eq 0) {
        return $CurrentUnityPath
    }

    $best = $candidates | Sort-Object Version -Descending | Select-Object -First 1
    Write-Host "Switching Unity editor to $($best.Name) because it includes $TargetPlatform support." -ForegroundColor Yellow
    return $best.Path
}

function Assert-UnityEditorNotRunning {
    $maxAttempts = 4
    $attempt = 1
    $processNames = @("Unity", "UnityPackageManager", "bee_backend", "netcorerun", "WebGLPlayerBuildProgram")

    while ($attempt -le $maxAttempts) {
        $runningUnity = @()
        foreach ($name in $processNames) {
            $runningUnity += Get-Process -Name $name -ErrorAction SilentlyContinue
        }

        $runningUnity = $runningUnity | Sort-Object Id -Unique

        if (-not $runningUnity) {
            return
        }

        foreach ($process in $runningUnity) {
            try {
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
                Write-Host "Stopped active process during precheck: $($process.ProcessName) (PID $($process.Id))" -ForegroundColor DarkYellow
            }
            catch {
                Write-Host "Could not stop process during precheck: $($process.ProcessName) (PID $($process.Id))" -ForegroundColor DarkYellow
            }
        }

        Start-Sleep -Seconds 2

        $remainingUnity = @()
        foreach ($name in $processNames) {
            $remainingUnity += Get-Process -Name $name -ErrorAction SilentlyContinue
        }

        $remainingUnity = $remainingUnity | Sort-Object Id -Unique
        if (-not $remainingUnity) {
            return
        }

        if ($attempt -lt $maxAttempts) {
            Write-Host "Detected Unity process during precheck (attempt $attempt/$maxAttempts). Retrying cleanup..." -ForegroundColor Yellow
            $attempt++
            continue
        }

        Write-Host "Unity Editor is currently running. Close Unity before batch test/build runs." -ForegroundColor Red
        Write-Host "Detected Unity PID(s): $($remainingUnity.Id -join ', ')" -ForegroundColor Yellow
        exit 1
    }
}

function Stop-BuildProcesses {
    $processNames = @("Unity", "UnityPackageManager", "bee_backend", "netcorerun", "WebGLPlayerBuildProgram")

    foreach ($name in $processNames) {
        $running = Get-Process -Name $name -ErrorAction SilentlyContinue
        if ($running) {
            foreach ($process in $running) {
                try {
                    Stop-Process -Id $process.Id -Force -ErrorAction Stop
                    Write-Host "Stopped lingering process: $name (PID $($process.Id))" -ForegroundColor DarkYellow
                }
                catch {
                    Write-Host "Could not stop process: $name (PID $($process.Id))" -ForegroundColor DarkYellow
                }
            }
        }
    }
}

function Clear-UnityDatabaseLocks {
    param([string]$WorkspacePath)

    $lockCandidates = @(
        (Join-Path $WorkspacePath "Library\SourceAssetDB-lock"),
        (Join-Path $WorkspacePath "Library\ArtifactDB-lock"),
        (Join-Path $WorkspacePath "Library\PackageManager\upm.lock")
    )

    foreach ($lockPath in $lockCandidates) {
        if (Test-Path $lockPath) {
            try {
                Remove-Item $lockPath -Force -ErrorAction Stop
                Write-Host "Removed Unity lock file: $lockPath" -ForegroundColor DarkYellow
            }
            catch {
                Write-Host "Could not remove Unity lock file: $lockPath" -ForegroundColor DarkYellow
            }
        }
    }
}

function Clear-UpmState {
    param([string]$WorkspacePath)

    Clear-UnityDatabaseLocks -WorkspacePath $WorkspacePath

    $upmCachePath = Join-Path $env:LOCALAPPDATA "Unity\cache\upm"
    if (Test-Path $upmCachePath) {
        try {
            Remove-Item $upmCachePath -Recurse -Force -ErrorAction Stop
            Write-Host "Cleared UPM cache: $upmCachePath" -ForegroundColor DarkYellow
        }
        catch {
            Write-Host "Could not clear UPM cache: $upmCachePath" -ForegroundColor DarkYellow
        }
    }
}

function Invoke-UnityUpmPreflight {
    param(
        [string]$UnityExecutablePath,
        [string]$WorkspacePath,
        [string]$LogsDirectory,
        [int]$MaxAttempts = 2
    )

    $preflightLogPath = Join-Path $LogsDirectory "upm_preflight.log"
    $attempt = 1

    while ($attempt -le $MaxAttempts) {
        Write-Host "UPM preflight attempt $attempt/$MaxAttempts..." -ForegroundColor Yellow

        Stop-BuildProcesses
        Clear-UpmState -WorkspacePath $WorkspacePath
        Start-Sleep -Seconds 3

        $arguments = @(
            "-quit",
            "-batchmode",
            "-nographics",
            "-projectPath", "`"$WorkspacePath`"",
            "-logFile", "`"$preflightLogPath`""
        )

        $process = Start-Process -FilePath $UnityExecutablePath -ArgumentList $arguments -PassThru -NoNewWindow
        $completed = $process.WaitForExit(180000)

        if (-not $completed) {
            try {
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
            }
            catch {
                Write-Host "Could not terminate timed-out UPM preflight process (PID $($process.Id))." -ForegroundColor DarkYellow
            }

            if ($attempt -ge $MaxAttempts) {
                return $false
            }

            $attempt++
            continue
        }

        $upmFailed = $false
        if (Test-Path $preflightLogPath) {
            try {
                # Check for critical UPM/IPC failures
                $hasUpmIpcError =
                    (Select-String -Path $preflightLogPath -Pattern "Could not establish a connection with the Unity Package Manager local server process." -SimpleMatch -Quiet) -or
                    (Select-String -Path $preflightLogPath -Pattern "Could not connect to IPC stream" -SimpleMatch -Quiet)
                
                # Check for compile errors
                $hasCompileErrors = Select-String -Path $preflightLogPath -Pattern "error CS\d+" -Quiet
                
                $upmFailed = $hasUpmIpcError -or $hasCompileErrors
            }
            catch {
                $upmFailed = $true
            }
        }
        
    # Accept exit code 0 or 1 (1 often just means warnings)
    # Only fail if we detected actual UPM/IPC errors or compile errors
    if (-not $upmFailed -and ($process.ExitCode -eq 0 -or $process.ExitCode -eq 1)) {
            Write-Host "UPM preflight passed." -ForegroundColor Green
            return $true
        }

        Write-Host "UPM preflight failed on attempt $attempt (exit code $($process.ExitCode))." -ForegroundColor Yellow
        if ($attempt -ge $MaxAttempts) {
            return $false
        }

        $attempt++
    }

    return $false
}

function Get-UnityEditorRoot {
    param([string]$UnityExecutablePath)
    return Split-Path -Parent $UnityExecutablePath
}

function Assert-PlatformSupportInstalled {
    param(
        [string]$TargetPlatform,
        [string]$EditorRootPath
    )

    $playbackEnginesPath = Join-Path $EditorRootPath "Data\PlaybackEngines"
    if (!(Test-Path $playbackEnginesPath)) {
        Write-Host "Could not locate PlaybackEngines path: $playbackEnginesPath" -ForegroundColor Red
        exit 1
    }

    $requiredFolder = Get-PlatformSupportFolderName -TargetPlatform $TargetPlatform

    if ([string]::IsNullOrWhiteSpace($requiredFolder)) {
        return
    }

    $supportPath = Join-Path $playbackEnginesPath $requiredFolder
    if (!(Test-Path $supportPath)) {
        Write-Host "$TargetPlatform Build Support is not installed for this Unity editor." -ForegroundColor Red
        Write-Host "Missing path: $supportPath" -ForegroundColor Yellow
        Write-Host "Install it in Unity Hub: Installs > $((Split-Path -Parent $EditorRootPath | Split-Path -Leaf)) > Add modules." -ForegroundColor Yellow
        exit 1
    }
}

function Assert-UnityLogHasNoBuildFailure {
    param(
        [string]$LogPath,
        [string]$Label
    )

    if (!(Test-Path $LogPath)) {
        Write-Host "$Label log file not found: $LogPath" -ForegroundColor Red
        exit 1
    }

    $failurePatterns = @(
        "Build Finished, Result: Failure",
        "Error building player",
        "build failed!"
    )

    $hasFailure = $false
    foreach ($pattern in $failurePatterns) {
        if (Select-String -Path $LogPath -Pattern $pattern -SimpleMatch -Quiet) {
            $hasFailure = $true
            break
        }
    }

    if ($hasFailure) {
        Write-Host "$Label reported build failure in log: $LogPath" -ForegroundColor Red
        exit 1
    }
}

function Run-UnityTests {
    Write-Host "Running Unity EditMode tests before build..." -ForegroundColor Yellow

    $resultsDir = Join-Path $BuildPath "TestResults"
    New-Item -ItemType Directory -Force -Path $resultsDir | Out-Null

    $testLogPath = Join-Path $logsPath "test_editmode.log"

    $arguments = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$ProjectPath`"",
        "-runTests",
        "-testPlatform", "EditMode",
        "-testResults", "`"$(Join-Path $resultsDir "editmode-results.xml")`"",
        "-logFile", "`"$testLogPath`""
    )

    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow
    if ($process.ExitCode -ne 0) {
        Write-Host "EditMode tests failed. Aborting build." -ForegroundColor Red
        if (Test-Path $testLogPath) {
            Write-Host "Test log: $testLogPath" -ForegroundColor Yellow
        }
        exit 1
    }

    Assert-UnityLogHasNoBuildFailure -LogPath $testLogPath -Label "EditMode tests"

    Write-Host "EditMode tests passed." -ForegroundColor Green
}

function Build-Android {
    Write-Host "Building for Android..." -ForegroundColor Yellow

    $buildMethod = "BuildMethods.BuildAndroid"
    $outputFile = Join-Path $androidBuildPath "AscendantContinuum.apk"

    $androidLogPath = Join-Path $logsPath "build_android.log"

    $arguments = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$ProjectPath`"",
        "-buildTarget", "Android",
        "-executeMethod", $buildMethod,
        "-logFile", "`"$androidLogPath`""
    )

    if ($BuildType -eq "Development") {
        $arguments += "-development"
    }

    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow

    if ($process.ExitCode -eq 0) {
        Assert-UnityLogHasNoBuildFailure -LogPath $androidLogPath -Label "Android build"
        Write-Host "Android build completed successfully." -ForegroundColor Green
        Write-Host "Output: $outputFile"
    }
    else {
        Write-Host "Android build failed. Check $androidLogPath for details." -ForegroundColor Red
        exit 1
    }
}

function Build-iOS {
    Write-Host "Building for iOS..." -ForegroundColor Yellow

    $buildMethod = "BuildMethods.BuildiOS"

    $iOSLogPath = Join-Path $logsPath "build_ios.log"

    $arguments = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$ProjectPath`"",
        "-buildTarget", "iOS",
        "-executeMethod", $buildMethod,
        "-logFile", "`"$iOSLogPath`""
    )

    if ($BuildType -eq "Development") {
        $arguments += "-development"
    }

    $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -Wait -PassThru -NoNewWindow

    if ($process.ExitCode -eq 0) {
        Assert-UnityLogHasNoBuildFailure -LogPath $iOSLogPath -Label "iOS build"
        Write-Host "iOS Xcode project generated successfully." -ForegroundColor Green
        Write-Host "Output: $iOSBuildPath"
    }
    else {
        Write-Host "iOS build failed. Check $iOSLogPath for details." -ForegroundColor Red
        exit 1
    }
}

function Build-WebGL {
    Write-Host "Building for WebGL..." -ForegroundColor Yellow
    if ($script:CleanBeeStateEffective) {
        Write-Host "Applying clean WebGL Bee state for deterministic build..." -ForegroundColor Yellow

        $beePath = Join-Path $ProjectPath "Library\Bee"
        $backupScenesPath = Join-Path $ProjectPath "Temp\__Backupscenes"

        if (Test-Path $backupScenesPath) {
            Remove-Item $backupScenesPath -Recurse -Force -ErrorAction SilentlyContinue
        }

        if (Test-Path $beePath) {
            Get-ChildItem -Path $beePath -Filter "TundraBuildState.state*" -ErrorAction SilentlyContinue |
            Remove-Item -Force -ErrorAction SilentlyContinue
        }
    }
    else {
        Write-Host "Skipping Bee state cleanup for WebGL build (CleanBeeState disabled)." -ForegroundColor DarkYellow
    }

    $buildMethod = "BuildMethods.BuildWebGL"

    $webGLLogPath = Join-Path $logsPath "build_webgl.log"

    $arguments = @(
        "-quit",
        "-batchmode",
        "-nographics",
        "-projectPath", "`"$ProjectPath`"",
        "-buildTarget", "WebGL",
        "-executeMethod", $buildMethod,
        "-logFile", "`"$webGLLogPath`""
    )

    Write-Host "Running WebGL build in a single uninterrupted batch process..." -ForegroundColor Cyan

    if ($BuildType -eq "Development") {
        $arguments += "-development"
    }

    $maxAttempts = 3
    $attempt = 1
    $unityBuildTimeoutSeconds = [Math]::Max(60, $UnityBuildTimeoutSeconds)

    while ($attempt -le $maxAttempts) {
        if ($attempt -gt 1) {
            Write-Host "Retrying WebGL build with forced Bee state cleanup (attempt $attempt/$maxAttempts)..." -ForegroundColor Yellow

            $beePath = Join-Path $ProjectPath "Library\Bee"
            $backupScenesPath = Join-Path $ProjectPath "Temp\__Backupscenes"

            if (Test-Path $backupScenesPath) {
                Remove-Item $backupScenesPath -Recurse -Force -ErrorAction SilentlyContinue
            }

            if (Test-Path $beePath) {
                Get-ChildItem -Path $beePath -Filter "TundraBuildState.state*" -ErrorAction SilentlyContinue |
                Remove-Item -Force -ErrorAction SilentlyContinue
            }

            Clear-UnityDatabaseLocks -WorkspacePath $ProjectPath
            Start-Sleep -Seconds 5
        }

        $upmReady = Invoke-UnityUpmPreflight -UnityExecutablePath $UnityPath -WorkspacePath $ProjectPath -LogsDirectory $logsPath
        if (-not $upmReady) {
            if ($attempt -ge $maxAttempts) {
                Write-Host "UPM preflight failed after multiple attempts. Check $(Join-Path $logsPath 'upm_preflight.log')." -ForegroundColor Red
                exit 1
            }

            Write-Host "UPM preflight failed for WebGL attempt $attempt. Preparing retry..." -ForegroundColor Yellow
            $attempt++
            continue
        }

        $process = Start-Process -FilePath $UnityPath -ArgumentList $arguments -PassThru -NoNewWindow
        $completed = $process.WaitForExit($unityBuildTimeoutSeconds * 1000)

        if (-not $completed) {
            Write-Host "WebGL build attempt $attempt timed out after $unityBuildTimeoutSeconds seconds. Terminating Unity process..." -ForegroundColor Yellow

            try {
                Stop-Process -Id $process.Id -Force -ErrorAction Stop
            }
            catch {
                Write-Host "Could not terminate timed-out Unity process (PID $($process.Id))." -ForegroundColor DarkYellow
            }

            Stop-BuildProcesses

            if ($attempt -ge $maxAttempts) {
                Write-Host "WebGL build timed out after $maxAttempts attempt(s). Check $webGLLogPath for details." -ForegroundColor Red
                exit 1
            }

            Write-Host "Preparing retry after timeout..." -ForegroundColor Yellow
            $attempt++
            continue
        }

        $exitCode = if ($null -ne $process.ExitCode) { $process.ExitCode } else { -1 }
        $logIndicatesSuccess = $false

        if (Test-Path $webGLLogPath) {
            try {
                $logIndicatesSuccess = Select-String -Path $webGLLogPath -Pattern "Build Finished, Result: Success." -SimpleMatch -Quiet
            }
            catch {
                $logIndicatesSuccess = $false
            }
        }

        if ($exitCode -eq 0 -or $logIndicatesSuccess) {
            Assert-UnityLogHasNoBuildFailure -LogPath $webGLLogPath -Label "WebGL build"
            Write-Host "WebGL build completed successfully." -ForegroundColor Green
            Write-Host "Output: $webGLBuildPath"
            return
        }

        $databaseLocked = $false
        if (Test-Path $webGLLogPath) {
            try {
                $databaseLocked = Select-String -Path $webGLLogPath -Pattern "database is locked" -SimpleMatch -Quiet
            }
            catch {
                $databaseLocked = $false
            }
        }

        if ($databaseLocked) {
            Write-Host "Detected Unity database lock during WebGL build attempt $attempt. Performing lock cleanup and retry." -ForegroundColor Yellow
            Stop-BuildProcesses
            Clear-UnityDatabaseLocks -WorkspacePath $ProjectPath
            Start-Sleep -Seconds 10
        }

        if ($attempt -ge $maxAttempts) {
            Stop-BuildProcesses
            Write-Host "WebGL build failed after $maxAttempts attempt(s). Check $webGLLogPath for details." -ForegroundColor Red
            exit 1
        }

        Write-Host "WebGL build attempt $attempt failed (exit code $exitCode). Preparing retry..." -ForegroundColor Yellow
        Stop-BuildProcesses
        $attempt++
    }
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "The Ascendant Continuum - Build Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ([string]::IsNullOrWhiteSpace($ProjectPath)) {
    $ProjectPath = if (![string]::IsNullOrWhiteSpace($PSScriptRoot)) { $PSScriptRoot } else { (Get-Location).Path }
}

if ([string]::IsNullOrWhiteSpace($BuildPath)) {
    $BuildPath = Join-Path $ProjectPath "Builds"
}

if ($CleanBeeState -and $SkipCleanBeeState) {
    Write-Host "Cannot pass both -CleanBeeState and -SkipCleanBeeState." -ForegroundColor Red
    exit 1
}

if ($CleanBeeState) {
    $script:CleanBeeStateEffective = $true
}
elseif ($SkipCleanBeeState) {
    $script:CleanBeeStateEffective = $false
}
else {
    $script:CleanBeeStateEffective = ($Platform -eq "WebGL" -or $Platform -eq "All")
}

$UnityPath = Resolve-UnityPath -RequestedPath $UnityPath -WorkspacePath $ProjectPath

$androidBuildPath = Join-Path $BuildPath "Android"
$iOSBuildPath = Join-Path $BuildPath "iOS"
$webGLBuildPath = Join-Path $BuildPath "WebGL"
$logsPath = Join-Path $ProjectPath "Logs"

New-Item -ItemType Directory -Force -Path $androidBuildPath | Out-Null
New-Item -ItemType Directory -Force -Path $iOSBuildPath | Out-Null
New-Item -ItemType Directory -Force -Path $webGLBuildPath | Out-Null
New-Item -ItemType Directory -Force -Path $logsPath | Out-Null

Write-Host "Platform: $Platform" -ForegroundColor Cyan
Write-Host "Build Type: $BuildType" -ForegroundColor Cyan
Write-Host "Clean Bee State: $script:CleanBeeStateEffective" -ForegroundColor Cyan
Write-Host "Unity Path: $UnityPath" -ForegroundColor Cyan
Write-Host "Project Path: $ProjectPath" -ForegroundColor Cyan
Write-Host "Build Output: $BuildPath" -ForegroundColor Cyan
Write-Host ""

if (!(Test-Path $UnityPath)) {
    Write-Host "Unity not found at: $UnityPath" -ForegroundColor Red
    Write-Host "Install Unity or pass -UnityPath explicitly." -ForegroundColor Yellow
    exit 1
}

Stop-BuildProcesses
if ($SkipUnityProcessPrecheck) {
    Write-Host "Skipping Unity process precheck by request (-SkipUnityProcessPrecheck)." -ForegroundColor Yellow
}
else {
    Assert-UnityEditorNotRunning
}

if ($Platform -ne "All") {
    $UnityPath = TryResolveUnityPathWithPlatformSupport -TargetPlatform $Platform -CurrentUnityPath $UnityPath
}

$editorRoot = Get-UnityEditorRoot -UnityExecutablePath $UnityPath

switch ($Platform) {
    "Android" { Assert-PlatformSupportInstalled -TargetPlatform "Android" -EditorRootPath $editorRoot }
    "iOS" { Assert-PlatformSupportInstalled -TargetPlatform "iOS" -EditorRootPath $editorRoot }
    "WebGL" { Assert-PlatformSupportInstalled -TargetPlatform "WebGL" -EditorRootPath $editorRoot }
    "All" {
        Assert-PlatformSupportInstalled -TargetPlatform "Android" -EditorRootPath $editorRoot
        Assert-PlatformSupportInstalled -TargetPlatform "iOS" -EditorRootPath $editorRoot
        Assert-PlatformSupportInstalled -TargetPlatform "WebGL" -EditorRootPath $editorRoot
    }
}

if ($RunTestsBeforeBuild) {
    Run-UnityTests
}

switch ($Platform) {
    "Android" { Build-Android }
    "iOS" { Build-iOS }
    "WebGL" { Build-WebGL }
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
Write-Host "Build process completed." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
