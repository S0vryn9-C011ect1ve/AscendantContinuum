param(
    [Parameter(Mandatory = $false)]
    [ValidateSet("All", "Rules", "Functions", "Hosting")]
    [string]$Target = "All",

    [Parameter(Mandatory = $false)]
    [string]$ProjectId = "ascendant-continuum",

    [Parameter(Mandatory = $false)]
    [switch]$Production
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Firebase Deployment Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$WorkspaceRoot = $PSScriptRoot
$FirebaseRoot = Join-Path $WorkspaceRoot "firebase"
$FirebaseToken = $env:FIREBASE_TOKEN

if (!(Test-Path (Join-Path $FirebaseRoot "firebase.json"))) {
    Write-Host "firebase/firebase.json not found at: $FirebaseRoot" -ForegroundColor Red
    exit 1
}

Set-Location $FirebaseRoot

if (!(Get-Command firebase -ErrorAction SilentlyContinue)) {
    Write-Host "Firebase CLI not found. Install with: npm install -g firebase-tools" -ForegroundColor Red
    exit 1
}

Write-Host "Checking Firebase authentication..." -ForegroundColor Yellow
if ([string]::IsNullOrWhiteSpace($FirebaseToken)) {
    firebase projects:list --json | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Please login to Firebase:" -ForegroundColor Yellow
        firebase login
        if ($LASTEXITCODE -ne 0) {
            Write-Host "Firebase login failed." -ForegroundColor Red
            exit 1
        }
    }
}

function Invoke-FirebaseDeploy {
    param(
        [string]$Only
    )

    if ([string]::IsNullOrWhiteSpace($FirebaseToken)) {
        firebase deploy --project $ProjectId --only $Only
    }
    else {
        firebase deploy --project $ProjectId --only $Only --token $FirebaseToken
    }

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Deployment failed for target: $Only" -ForegroundColor Red
        exit 1
    }
}

function DeployFirestoreRules {
    Write-Host ""
    Write-Host "Deploying Firestore rules..." -ForegroundColor Yellow
    Invoke-FirebaseDeploy -Only "firestore:rules"
    Write-Host "Firestore rules deployed successfully." -ForegroundColor Green
}

function DeployStorageRules {
    Write-Host ""
    Write-Host "Deploying Storage rules..." -ForegroundColor Yellow
    Invoke-FirebaseDeploy -Only "storage"
    Write-Host "Storage rules deployed successfully." -ForegroundColor Green
}

function DeployFunctions {
    Write-Host ""
    Write-Host "Installing Cloud Functions dependencies..." -ForegroundColor Yellow

    Push-Location (Join-Path $FirebaseRoot "functions")
    npm install
    if ($LASTEXITCODE -ne 0) {
        Pop-Location
        Write-Host "Failed to install Cloud Functions dependencies." -ForegroundColor Red
        exit 1
    }
    Pop-Location

    Write-Host "Deploying Cloud Functions..." -ForegroundColor Yellow
    Invoke-FirebaseDeploy -Only "functions"
    Write-Host "Cloud Functions deployed successfully." -ForegroundColor Green
}

function DeployHosting {
    Write-Host ""
    Write-Host "Deploying Firebase Hosting..." -ForegroundColor Yellow

    $WebGLBuildPath = Join-Path $WorkspaceRoot "Builds\WebGL"
    $PlayPath = Join-Path $FirebaseRoot "public\play"

    if (Test-Path $WebGLBuildPath) {
        Write-Host "Found WebGL build. Copying to public/play..." -ForegroundColor Yellow
        if (!(Test-Path $PlayPath)) {
            New-Item -ItemType Directory -Force -Path $PlayPath | Out-Null
        }
        Copy-Item -Path "$WebGLBuildPath\*" -Destination $PlayPath -Recurse -Force
        Write-Host "WebGL build copied successfully." -ForegroundColor Green
    }
    else {
        Write-Host "No WebGL build found at Builds/WebGL. Deploying website only." -ForegroundColor Yellow
    }

    Invoke-FirebaseDeploy -Only "hosting"
    Write-Host "Firebase Hosting deployed successfully." -ForegroundColor Green
    Write-Host "Live at: https://$ProjectId.web.app" -ForegroundColor Cyan
}

Write-Host "Target: $Target" -ForegroundColor Cyan
Write-Host "Project: $ProjectId" -ForegroundColor Cyan
Write-Host "Environment: $(if ($Production) { 'Production' } else { 'Development' })" -ForegroundColor Cyan
Write-Host ""

if ($Production) {
    Write-Host "WARNING: Deploying to PRODUCTION" -ForegroundColor Yellow
    $confirm = Read-Host "Are you sure? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "Deployment cancelled." -ForegroundColor Yellow
        exit 0
    }
}

switch ($Target) {
    "Rules" {
        DeployFirestoreRules
        DeployStorageRules
    }
    "Functions" {
        DeployFunctions
    }
    "Hosting" {
        DeployHosting
    }
    "All" {
        DeployFirestoreRules
        DeployStorageRules
        DeployFunctions
        DeployHosting
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment completed successfully." -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
