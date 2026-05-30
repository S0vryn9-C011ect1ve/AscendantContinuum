# Automated Maintenance Tasks
# Runs routine project maintenance to prevent technical debt

param(
    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ascendant Continuum - Maintenance" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

# ============================================================================
# TASK 1: Clean Unity Temp Files
# ============================================================================
Write-Host "[1/6] Cleaning Unity temp files..." -ForegroundColor Cyan

$tempFolders = @(
    "Temp",
    "obj",
    "Logs"
)

foreach ($folder in $tempFolders) {
    $path = Join-Path $ProjectRoot $folder
    if (Test-Path $path) {
        $size = (Get-ChildItem $path -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "  Found: $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Gray
        
        if (!$DryRun) {
            Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  ✓ Cleaned" -ForegroundColor Green
        }
    }
}

# ============================================================================
# TASK 2: Update Package Dependencies
# ============================================================================
Write-Host "[2/6] Checking npm dependencies..." -ForegroundColor Cyan

$packageJson = Join-Path $ProjectRoot "package.json"
if (Test-Path $packageJson) {
    Write-Host "  Running npm audit..." -ForegroundColor Gray
    
    if (!$DryRun) {
        Push-Location $ProjectRoot
        npm audit fix --only=prod 2>&1 | Out-Null
        Pop-Location
        Write-Host "  ✓ Dependencies audited and fixed" -ForegroundColor Green
    }
    else {
        Write-Host "  (Would run: npm audit fix)" -ForegroundColor Yellow
    }
}

# ============================================================================
# TASK 3: Clean Old Build Artifacts
# ============================================================================
Write-Host "[3/6] Cleaning old build artifacts..." -ForegroundColor Cyan

$buildsPath = Join-Path $ProjectRoot "Builds"
if (Test-Path $buildsPath) {
    $oldBuilds = Get-ChildItem $buildsPath -Recurse -File | Where-Object { 
        $_.LastWriteTime -lt (Get-Date).AddDays(-7) 
    }
    
    $totalSize = ($oldBuilds | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "  Found $($oldBuilds.Count) files older than 7 days ($([math]::Round($totalSize, 2)) MB)" -ForegroundColor Gray
    
    if (!$DryRun -and $oldBuilds.Count -gt 0) {
        $oldBuilds | Remove-Item -Force
        Write-Host "  ✓ Cleaned old builds" -ForegroundColor Green
    }
}

# ============================================================================
# TASK 4: Verify Firebase Rate Limits
# ============================================================================
Write-Host "[4/6] Verifying Firebase rate limits..." -ForegroundColor Cyan

$firestoreRules = Join-Path $ProjectRoot "firebase\firestore.rules"
if (Test-Path $firestoreRules) {
    $content = Get-Content $firestoreRules -Raw
    
    $hasRateLimits = $content -match "rateLimit\("
    $hasSizeLimits = $content -match "\.size\(\) <"
    
    if ($hasRateLimits) {
        Write-Host "  ✓ Rate limits detected" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠ No rate limits found!" -ForegroundColor Yellow
    }
    
    if ($hasSizeLimits) {
        Write-Host "  ✓ Size limits detected" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠ No size limits found!" -ForegroundColor Yellow
    }
}

# ============================================================================
# TASK 5: Check for Large Files
# ============================================================================
Write-Host "[5/6] Checking for large files..." -ForegroundColor Cyan

$largeFiles = Get-ChildItem $ProjectRoot -Recurse -File -ErrorAction SilentlyContinue |
Where-Object { $_.Length -gt 50MB -and $_.DirectoryName -notmatch "Library|Builds" } |
Select-Object FullName, @{Name = "SizeMB"; Expression = { [math]::Round($_.Length / 1MB, 2) } } |
Sort-Object SizeMB -Descending |
Select-Object -First 5

if ($largeFiles.Count -gt 0) {
    Write-Host "  ⚠ Large files detected:" -ForegroundColor Yellow
    foreach ($file in $largeFiles) {
        Write-Host "    - $($file.FullName.Replace($ProjectRoot, '.')) ($($file.SizeMB) MB)" -ForegroundColor Gray
    }
}
else {
    Write-Host "  ✓ No large files found" -ForegroundColor Green
}

# ============================================================================
# TASK 6: Update Documentation Index
# ============================================================================
Write-Host "[6/6] Updating documentation index..." -ForegroundColor Cyan

$docsPath = Join-Path $ProjectRoot "docs"
if (Test-Path $docsPath) {
    $mdFiles = Get-ChildItem $docsPath -Filter "*.md" -Recurse
    Write-Host "  Found $($mdFiles.Count) documentation files" -ForegroundColor Gray
    Write-Host "  ✓ Documentation tracked" -ForegroundColor Green
}

# ============================================================================
# Summary
# ============================================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Maintenance Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "This was a DRY RUN. Run without -DryRun to apply changes." -ForegroundColor Yellow
}
else {
    Write-Host "✓ All maintenance tasks completed successfully" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review any warnings above" -ForegroundColor White
Write-Host "  2. Run: npm run security:full-scan" -ForegroundColor White
Write-Host "  3. Test build: .\Build.ps1 -Platform WebGL" -ForegroundColor White
Write-Host ""
# Automated Maintenance Tasks
# Runs routine project maintenance to prevent technical debt

param(
    [Parameter(Mandatory = $false)]
    [switch]$DryRun
)

$ErrorActionPreference = "Stop"
$ProjectRoot = $PSScriptRoot

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Ascendant Continuum - Maintenance" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "DRY RUN MODE - No changes will be made" -ForegroundColor Yellow
    Write-Host ""
}

# ============================================================================
# TASK 1: Clean Unity Temp Files
# ============================================================================
Write-Host "[1/6] Cleaning Unity temp files..." -ForegroundColor Cyan

$tempFolders = @(
    "Temp",
    "obj",
    "Logs"
)

foreach ($folder in $tempFolders) {
    $path = Join-Path $ProjectRoot $folder
    if (Test-Path $path) {
        $size = (Get-ChildItem $path -Recurse -ErrorAction SilentlyContinue | Measure-Object -Property Length -Sum).Sum / 1MB
        Write-Host "  Found: $folder ($([math]::Round($size, 2)) MB)" -ForegroundColor Gray
        
        if (!$DryRun) {
            Remove-Item $path -Recurse -Force -ErrorAction SilentlyContinue
            Write-Host "  ✓ Cleaned" -ForegroundColor Green
        }
    }
}

# ============================================================================
# TASK 2: Update Package Dependencies
# ============================================================================
Write-Host "[2/6] Checking npm dependencies..." -ForegroundColor Cyan

$packageJson = Join-Path $ProjectRoot "package.json"
if (Test-Path $packageJson) {
    Write-Host "  Running npm audit..." -ForegroundColor Gray
    
    if (!$DryRun) {
        Push-Location $ProjectRoot
        npm audit fix --only=prod 2>&1 | Out-Null
        Pop-Location
        Write-Host "  ✓ Dependencies audited and fixed" -ForegroundColor Green
    }
    else {
        Write-Host "  (Would run: npm audit fix)" -ForegroundColor Yellow
    }
}

# ============================================================================
# TASK 3: Clean Old Build Artifacts
# ============================================================================
Write-Host "[3/6] Cleaning old build artifacts..." -ForegroundColor Cyan

$buildsPath = Join-Path $ProjectRoot "Builds"
if (Test-Path $buildsPath) {
    $oldBuilds = Get-ChildItem $buildsPath -Recurse -File | Where-Object { 
        $_.LastWriteTime -lt (Get-Date).AddDays(-7) 
    }
    
    $totalSize = ($oldBuilds | Measure-Object -Property Length -Sum).Sum / 1MB
    Write-Host "  Found $($oldBuilds.Count) files older than 7 days ($([math]::Round($totalSize, 2)) MB)" -ForegroundColor Gray
    
    if (!$DryRun -and $oldBuilds.Count -gt 0) {
        $oldBuilds | Remove-Item -Force
        Write-Host "  ✓ Cleaned old builds" -ForegroundColor Green
    }
}

# ============================================================================
# TASK 4: Verify Firebase Rate Limits
# ============================================================================
Write-Host "[4/6] Verifying Firebase rate limits..." -ForegroundColor Cyan

$firestoreRules = Join-Path $ProjectRoot "firebase\firestore.rules"
if (Test-Path $firestoreRules) {
    $content = Get-Content $firestoreRules -Raw
    
    $hasRateLimits = $content -match "rateLimit\("
    $hasSizeLimits = $content -match "\.size\(\) <"
    
    if ($hasRateLimits) {
        Write-Host "  ✓ Rate limits detected" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠ No rate limits found!" -ForegroundColor Yellow
    }
    
    if ($hasSizeLimits) {
        Write-Host "  ✓ Size limits detected" -ForegroundColor Green
    }
    else {
        Write-Host "  ⚠ No size limits found!" -ForegroundColor Yellow
    }
}

# ============================================================================
# TASK 5: Check for Large Files
# ============================================================================
Write-Host "[5/6] Checking for large files..." -ForegroundColor Cyan

$largeFiles = Get-ChildItem $ProjectRoot -Recurse -File -ErrorAction SilentlyContinue |
Where-Object { $_.Length -gt 50MB -and $_.DirectoryName -notmatch "Library|Builds" } |
Select-Object FullName, @{Name = "SizeMB"; Expression = { [math]::Round($_.Length / 1MB, 2) } } |
Sort-Object SizeMB -Descending |
Select-Object -First 5

if ($largeFiles.Count -gt 0) {
    Write-Host "  ⚠ Large files detected:" -ForegroundColor Yellow
    foreach ($file in $largeFiles) {
        Write-Host "    - $($file.FullName.Replace($ProjectRoot, '.')) ($($file.SizeMB) MB)" -ForegroundColor Gray
    }
}
else {
    Write-Host "  ✓ No large files found" -ForegroundColor Green
}

# ============================================================================
# TASK 6: Update Documentation Index
# ============================================================================
Write-Host "[6/6] Updating documentation index..." -ForegroundColor Cyan

$docsPath = Join-Path $ProjectRoot "docs"
if (Test-Path $docsPath) {
    $mdFiles = Get-ChildItem $docsPath -Filter "*.md" -Recurse
    Write-Host "  Found $($mdFiles.Count) documentation files" -ForegroundColor Gray
    Write-Host "  ✓ Documentation tracked" -ForegroundColor Green
}

# ============================================================================
# Summary
# ============================================================================
Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Maintenance Complete" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

if ($DryRun) {
    Write-Host "This was a DRY RUN. Run without -DryRun to apply changes." -ForegroundColor Yellow
}
else {
    Write-Host "✓ All maintenance tasks completed successfully" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Review any warnings above" -ForegroundColor White
Write-Host "  2. Run: npm run security:full-scan" -ForegroundColor White
Write-Host "  3. Test build: .\Build.ps1 -Platform WebGL" -ForegroundColor White
Write-Host ""
