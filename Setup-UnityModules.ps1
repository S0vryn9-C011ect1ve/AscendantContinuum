# Setup Unity Build Modules
# Installs Android, iOS, and WebGL build support for Unity 6000.3.9f1

param(
    [Parameter(Mandatory = $false)]
    [string]$UnityVersion = "6000.3.9f1",
    
    [Parameter(Mandatory = $false)]
    [switch]$AndroidOnly,
    
    [Parameter(Mandatory = $false)]
    [switch]$iOSOnly,
    
    [Parameter(Mandatory = $false)]
    [switch]$WebGLOnly
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Unity Build Modules Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$UnityHubPath = "C:\Program Files\Unity Hub\Unity Hub.exe"
if (!(Test-Path $UnityHubPath)) {
    Write-Host "Unity Hub not found at: $UnityHubPath" -ForegroundColor Red
    Write-Host "Please install Unity Hub from: https://unity.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host "Unity Hub found." -ForegroundColor Green
Write-Host "Target version: $UnityVersion" -ForegroundColor Yellow
Write-Host ""

# Check if editor version is installed
$EditorPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion"
if (!(Test-Path $EditorPath)) {
    Write-Host "Unity $UnityVersion not installed!" -ForegroundColor Red
    Write-Host "Install it via Unity Hub first." -ForegroundColor Yellow
    exit 1
}

Write-Host "Unity $UnityVersion is installed." -ForegroundColor Green
Write-Host ""

# Install modules based on flags
$modulesToInstall = @()

if ($AndroidOnly) {
    $modulesToInstall += "android"
}
elseif ($iOSOnly) {
    $modulesToInstall += "ios"
}
elseif ($WebGLOnly) {
    $modulesToInstall += "webgl"
}
else {
    # Install all by default
    $modulesToInstall += "android", "ios", "webgl"
}

Write-Host "Modules to install: $($modulesToInstall -join ', ')" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT: This will launch Unity Hub." -ForegroundColor Yellow
Write-Host "Please complete the module installation through the Unity Hub GUI." -ForegroundColor Yellow
Write-Host ""
Write-Host "Steps:" -ForegroundColor Cyan
Write-Host "1. Unity Hub will open" -ForegroundColor White
Write-Host "2. Go to: Installs → Click gear icon next to $UnityVersion → Add modules" -ForegroundColor White
Write-Host "3. Check: Android Build Support, iOS Build Support, WebGL Build Support" -ForegroundColor White
Write-Host "4. Click 'Install' and wait for completion" -ForegroundColor White
Write-Host ""

$response = Read-Host "Press Enter to open Unity Hub (or 'q' to quit)"
if ($response -eq 'q') {
    Write-Host "Setup cancelled." -ForegroundColor Yellow
    exit 0
}

# Launch Unity Hub
Start-Process $UnityHubPath

Write-Host ""
Write-Host "Unity Hub launched." -ForegroundColor Green
Write-Host "Follow the steps above to install build modules." -ForegroundColor Yellow
Write-Host ""
Write-Host "After installation completes, verify with:" -ForegroundColor Cyan
Write-Host "  .\Build.ps1 -Platform Android" -ForegroundColor White
Write-Host "  .\Build.ps1 -Platform iOS" -ForegroundColor White
Write-Host "  .\Build.ps1 -Platform WebGL" -ForegroundColor White
Write-Host ""
# Setup Unity Build Modules
# Installs Android, iOS, and WebGL build support for Unity 6000.3.9f1

param(
    [Parameter(Mandatory = $false)]
    [string]$UnityVersion = "6000.3.9f1",
    
    [Parameter(Mandatory = $false)]
    [switch]$AndroidOnly,
    
    [Parameter(Mandatory = $false)]
    [switch]$iOSOnly,
    
    [Parameter(Mandatory = $false)]
    [switch]$WebGLOnly
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Unity Build Modules Setup" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$UnityHubPath = "C:\Program Files\Unity Hub\Unity Hub.exe"
if (!(Test-Path $UnityHubPath)) {
    Write-Host "Unity Hub not found at: $UnityHubPath" -ForegroundColor Red
    Write-Host "Please install Unity Hub from: https://unity.com/download" -ForegroundColor Yellow
    exit 1
}

Write-Host "Unity Hub found." -ForegroundColor Green
Write-Host "Target version: $UnityVersion" -ForegroundColor Yellow
Write-Host ""

# Check if editor version is installed
$EditorPath = "C:\Program Files\Unity\Hub\Editor\$UnityVersion"
if (!(Test-Path $EditorPath)) {
    Write-Host "Unity $UnityVersion not installed!" -ForegroundColor Red
    Write-Host "Install it via Unity Hub first." -ForegroundColor Yellow
    exit 1
}

Write-Host "Unity $UnityVersion is installed." -ForegroundColor Green
Write-Host ""

# Install modules based on flags
$modulesToInstall = @()

if ($AndroidOnly) {
    $modulesToInstall += "android"
}
elseif ($iOSOnly) {
    $modulesToInstall += "ios"
}
elseif ($WebGLOnly) {
    $modulesToInstall += "webgl"
}
else {
    # Install all by default
    $modulesToInstall += "android", "ios", "webgl"
}

Write-Host "Modules to install: $($modulesToInstall -join ', ')" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT: This will launch Unity Hub." -ForegroundColor Yellow
Write-Host "Please complete the module installation through the Unity Hub GUI." -ForegroundColor Yellow
Write-Host ""
Write-Host "Steps:" -ForegroundColor Cyan
Write-Host "1. Unity Hub will open" -ForegroundColor White
Write-Host "2. Go to: Installs → Click gear icon next to $UnityVersion → Add modules" -ForegroundColor White
Write-Host "3. Check: Android Build Support, iOS Build Support, WebGL Build Support" -ForegroundColor White
Write-Host "4. Click 'Install' and wait for completion" -ForegroundColor White
Write-Host ""

$response = Read-Host "Press Enter to open Unity Hub (or 'q' to quit)"
if ($response -eq 'q') {
    Write-Host "Setup cancelled." -ForegroundColor Yellow
    exit 0
}

# Launch Unity Hub
Start-Process $UnityHubPath

Write-Host ""
Write-Host "Unity Hub launched." -ForegroundColor Green
Write-Host "Follow the steps above to install build modules." -ForegroundColor Yellow
Write-Host ""
Write-Host "After installation completes, verify with:" -ForegroundColor Cyan
Write-Host "  .\Build.ps1 -Platform Android" -ForegroundColor White
Write-Host "  .\Build.ps1 -Platform iOS" -ForegroundColor White
Write-Host "  .\Build.ps1 -Platform WebGL" -ForegroundColor White
Write-Host ""
