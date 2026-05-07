# Daily Blog Post Script for Windows Task Scheduler
# Runs completely FREE on your local machine

param(
    [switch]$DryRun = $false
)

$ErrorActionPreference = "Stop"
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$projectRoot = (Resolve-Path (Join-Path $scriptDir "..\.." )).Path
$hadError = $false

# Log file
$logDir = "$projectRoot\logs"
if (!(Test-Path $logDir)) {
    New-Item -ItemType Directory -Path $logDir -Force | Out-Null
}
$logFile = "$logDir\blog-automation-$(Get-Date -Format 'yyyy-MM-dd').log"

function Write-Log {
    param([Parameter(Mandatory = $true)][object]$Message)
    $timestamp = Get-Date -Format "yyyy-MM-dd HH:mm:ss"
    $text = ($Message | Out-String).TrimEnd()
    $logMessage = "[$timestamp] $text"
    Write-Host $logMessage
    Add-Content -Path $logFile -Value $logMessage
}

function Test-RequiredCommand {
    param([Parameter(Mandatory = $true)][string]$Name)
    if (-not (Get-Command $Name -ErrorAction SilentlyContinue)) {
        Write-Log "ERROR: Required command not found: $Name"
        $script:hadError = $true
    }
    else {
        Write-Log "OK: Found command: $Name"
    }
}

function Set-EnvFromDotEnvLine {
    param([AllowEmptyString()][string]$Line)

    if ([string]::IsNullOrWhiteSpace($Line)) { return }
    $trimmed = $Line.Trim()
    if ($trimmed.StartsWith('#')) { return }
    if ($trimmed -notmatch '^([^=]+)=(.*)$') { return }

    $name = $matches[1].Trim()
    $value = $matches[2].Trim()

    if (($value.StartsWith('"') -and $value.EndsWith('"')) -or ($value.StartsWith("'") -and $value.EndsWith("'"))) {
        $value = $value.Substring(1, $value.Length - 2)
    }

    [Environment]::SetEnvironmentVariable($name, $value, "Process")
}

Write-Log "=========================================="
Write-Log "Daily Blog Post - Starting"
Write-Log "=========================================="

# Change to project directory
Set-Location $projectRoot
Write-Log "Working directory: $projectRoot"

Test-RequiredCommand -Name "node"
Test-RequiredCommand -Name "git"
Test-RequiredCommand -Name "npx"
if ($hadError) {
    Write-Log "ERROR: Preflight checks failed"
    exit 1
}

# Load environment variables from .env file
$envFile = "$projectRoot\.env"
if (Test-Path $envFile) {
    Write-Log "Loading environment variables from .env"
    Get-Content $envFile | ForEach-Object { Set-EnvFromDotEnvLine -Line $_ }
}
else {
    Write-Log "WARNING: .env file not found at $envFile"
}

# Set DRY_RUN environment variable
if ($DryRun) {
    Write-Log "Running in DRY RUN mode (no actual posts)"
    [Environment]::SetEnvironmentVariable("DRY_RUN", "true", "Process")
}
else {
    [Environment]::SetEnvironmentVariable("DRY_RUN", "false", "Process")
}

# Run the blog poster
Write-Log "Executing: node scripts/automation/post-blog.js"
try {
    $output = & node scripts/automation/post-blog.js 2>&1
    Write-Log $output
    
    if ($LASTEXITCODE -eq 0) {
        Write-Log "OK: Blog post completed successfully"

        if ($DryRun) {
            Write-Log "DRY RUN: Skipping git commit/push and Firebase deploy"
        }
        
        # Git commit and push if there are changes
        if (-not $DryRun) {
            Write-Log "Checking for blog changes to commit..."
            $gitStatus = git status --porcelain firebase/public/blog/
            if ($gitStatus) {
                Write-Log "Changes detected, committing..."
                & git add firebase/public/blog/
                & git commit -m "blog: automated daily post [skip ci]"
                & git push origin main
                Write-Log "OK: Changes committed and pushed"
                
                # Deploy to Firebase
                Write-Log "Deploying to Firebase..."
                Set-Location "$projectRoot\firebase"
                & npx firebase deploy --only hosting
                if ($LASTEXITCODE -ne 0) {
                    Write-Log "ERROR: Firebase deployment failed with exit code: $LASTEXITCODE"
                    $hadError = $true
                }
                Set-Location $projectRoot
                if (-not $hadError) {
                    Write-Log "OK: Deployed to Firebase"
                }
            }
            else {
                Write-Log "No blog changes to commit"
            }
        }
    }
    else {
        Write-Log "ERROR: Blog post failed with exit code: $LASTEXITCODE"
        $hadError = $true
    }
}
catch {
    Write-Log "ERROR: Error running blog poster: $_"
    $hadError = $true
}

Write-Log "=========================================="
Write-Log "Daily Blog Post - Completed"
Write-Log "=========================================="
Write-Log ""

if ($hadError) {
    exit 1
}

exit 0
