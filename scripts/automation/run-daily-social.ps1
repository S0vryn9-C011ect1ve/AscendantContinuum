# Daily Social Media Posting Script for Windows Task Scheduler
# Runs completely FREE on your local machine

param(
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Continue"
$projectRoot = "d:\1-Ascendant Continuum Game"

# Log file
$logDir = "$projectRoot\logs"
if (!(Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}
$logFile = "$logDir\social-automation-$(Get-Date -Format 'yyyy-MM-dd').log"

function Write-Log {
    param($Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $logMessage = "[$timestamp] $Message"
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage
}

Write-Log "=========================================="
Write-Log "Daily Social Media Post - Starting"
Write-Log "=========================================="

# Change to project directory
Set-Location $projectRoot
Write-Log "Working directory: $projectRoot"

# Load environment variables from .env file if it exists
$envFile = "$projectRoot\.env"
if (Test-Path $envFile) {
    Write-Log "Loading environment variables from .env"
    Get-Content $envFile | ForEach-Object {
        if ($_ -match '^([^=]+)=(.+)$') {
            $name = $matches[1].Trim()
            $value = $matches[2].Trim()
            [Environment]::SetEnvironmentVariable($name, $value, "Process")
        }
    }
} else {
    Write-Log "WARNING: .env file not found at $envFile"
    Write-Log "Create .env file with your credentials (see .env.example)"
}

# Set DRY_RUN environment variable
if ($DryRun) {
    Write-Log "Running in DRY RUN mode (no actual posts)"
    [Environment]::SetEnvironmentVariable("DRY_RUN", "true", "Process")
} else {
    [Environment]::SetEnvironmentVariable("DRY_RUN", "false", "Process")
}

# Run the content scheduler
Write-Log "Executing: node scripts/automation/content-scheduler.js"
try {
    $output = node scripts/automation/content-scheduler.js 2>&1
    Write-Log $output
    
    if ($LASTEXITCODE -eq 0) {
        Write-Log "✅ Social media post completed successfully"
    } else {
        Write-Log "❌ Social media post failed with exit code: $LASTEXITCODE"
    }
} catch {
    Write-Log "❌ Error running content scheduler: $_"
}

Write-Log "=========================================="
Write-Log "Daily Social Media Post - Completed"
Write-Log "=========================================="
Write-Log ""
