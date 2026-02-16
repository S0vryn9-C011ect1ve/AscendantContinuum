# ============================================================================
# Firebase Deployment Script
# Deploys Firestore rules, Storage rules, and Cloud Functions
# ============================================================================

param(
    [Parameter(Mandatory=$false)]
    [ValidateSet("All", "Rules", "Functions", "Hosting")]
    [string]$Target = "All",
    
    [Parameter(Mandatory=$false)]
    [string]$ProjectId = "ascendant-continuum",
    
    [Parameter(Mandatory=$false)]
    [switch]$Production
)

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Firebase Deployment Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Change to project directory
Set-Location "D:\1-Ascendant Continuum Game"

# Check if Firebase CLI is installed
if (!(Get-Command firebase -ErrorAction SilentlyContinue)) {
    Write-Host "✗ Firebase CLI not found!" -ForegroundColor Red
    Write-Host "Install it with: npm install -g firebase-tools" -ForegroundColor Yellow
    exit 1
}

# Login check
Write-Host "Checking Firebase authentication..." -ForegroundColor Yellow
$loginStatus = firebase login:ci
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please login to Firebase:" -ForegroundColor Yellow
    firebase login
}

# Set project
Write-Host "Setting Firebase project: $ProjectId" -ForegroundColor Cyan
firebase use $ProjectId

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ Failed to set Firebase project!" -ForegroundColor Red
    exit 1
}

# ============================================================================
# DEPLOY FIRESTORE RULES
# ============================================================================

function Deploy-FirestoreRules {
    Write-Host ""
    Write-Host "Deploying Firestore security rules..." -ForegroundColor Yellow
    
    firebase deploy --only firestore:rules
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Firestore rules deployed successfully!" -ForegroundColor Green
    } else {
        Write-Host "✗ Firestore rules deployment failed!" -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# DEPLOY STORAGE RULES
# ============================================================================

function Deploy-StorageRules {
    Write-Host ""
    Write-Host "Deploying Storage security rules..." -ForegroundColor Yellow
    
    firebase deploy --only storage:rules
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Storage rules deployed successfully!" -ForegroundColor Green
    } else {
        Write-Host "✗ Storage rules deployment failed!" -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# DEPLOY CLOUD FUNCTIONS
# ============================================================================

function Deploy-Functions {
    Write-Host ""
    Write-Host "Installing Cloud Functions dependencies..." -ForegroundColor Yellow
    
    Set-Location "firebase\functions"
    npm install
    
    if ($LASTEXITCODE -ne 0) {
        Write-Host "✗ Failed to install dependencies!" -ForegroundColor Red
        Set-Location "..\..\"
        exit 1
    }
    
    Set-Location "..\..\"
    
    Write-Host "Deploying Cloud Functions..." -ForegroundColor Yellow
    firebase deploy --only functions
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Cloud Functions deployed successfully!" -ForegroundColor Green
    } else {
        Write-Host "✗ Cloud Functions deployment failed!" -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# DEPLOY HOSTING
# ============================================================================

function Deploy-Hosting {
    Write-Host ""
    Write-Host "Deploying Firebase Hosting..." -ForegroundColor Yellow
    
    # Check if WebGL build exists
    if (!(Test-Path "Builds\WebGL")) {
        Write-Host "✗ WebGL build not found! Build the project first." -ForegroundColor Red
        exit 1
    }
    
    firebase deploy --only hosting
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✓ Firebase Hosting deployed successfully!" -ForegroundColor Green
        Write-Host "Live at: https://$ProjectId.web.app" -ForegroundColor Cyan
    } else {
        Write-Host "✗ Firebase Hosting deployment failed!" -ForegroundColor Red
        exit 1
    }
}

# ============================================================================
# MAIN EXECUTION
# ============================================================================

Write-Host "Target: $Target" -ForegroundColor Cyan
Write-Host "Project: $ProjectId" -ForegroundColor Cyan
Write-Host "Environment: $(if ($Production) { 'Production' } else { 'Development' })" -ForegroundColor Cyan
Write-Host ""

# Confirmation for production
if ($Production) {
    Write-Host "⚠️  WARNING: Deploying to PRODUCTION!" -ForegroundColor Yellow
    $confirm = Read-Host "Are you sure? (yes/no)"
    if ($confirm -ne "yes") {
        Write-Host "Deployment cancelled." -ForegroundColor Yellow
        exit 0
    }
}

# Execute deployments
switch ($Target) {
    "Rules" {
        Deploy-FirestoreRules
        Deploy-StorageRules
    }
    "Functions" {
        Deploy-Functions
    }
    "Hosting" {
        Deploy-Hosting
    }
    "All" {
        Deploy-FirestoreRules
        Deploy-StorageRules
        Deploy-Functions
        Deploy-Hosting
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Green
Write-Host "Deployment completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
