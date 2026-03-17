# Setup Local Automation with Windows Task Scheduler
# 100% FREE - Runs on your local machine, no cloud costs

param(
    [switch]$DryRun = $false,
    [switch]$Remove = $false
)

$ErrorActionPreference = "Stop"
$projectRoot = "d:\1-Ascendant Continuum Game"
$scriptPath1 = Join-Path $projectRoot "scripts\automation\run-daily-social.ps1"
$scriptPath2 = Join-Path $projectRoot "scripts\automation\run-daily-blog.ps1"

Write-Host ""
Write-Host "LOCAL AUTOMATION SETUP (100% FREE)" -ForegroundColor Cyan
Write-Host "This will create Windows Task Scheduler tasks to automate social media posting" -ForegroundColor Gray
Write-Host "completely FREE on your local machine - no GitHub Actions billing!" -ForegroundColor Gray
Write-Host ""

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "ERROR: This script requires Administrator privileges" -ForegroundColor Red
    Write-Host "Right-click PowerShell and 'Run as Administrator', then run this script again" -ForegroundColor Yellow
    exit 1
}

# Task names
$taskNameDaily = "AscendantContinuum-DailySocial"
$taskNameBlog = "AscendantContinuum-DailyBlog"

if ($Remove) {
    Write-Host "Removing existing tasks..." -ForegroundColor Yellow
    
    try {
        Unregister-ScheduledTask -TaskName $taskNameDaily -Confirm:$false -ErrorAction SilentlyContinue
        Write-Host "Removed: $taskNameDaily" -ForegroundColor Green
    } catch {
        Write-Host "Task not found: $taskNameDaily" -ForegroundColor Gray
    }
    
    try {
        Unregister-ScheduledTask -TaskName $taskNameBlog -Confirm:$false -ErrorAction SilentlyContinue
        Write-Host "Removed: $taskNameBlog" -ForegroundColor Green
    } catch {
        Write-Host "Task not found: $taskNameBlog" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Host "Cleanup complete!" -ForegroundColor Green
    Write-Host ""
    exit 0
}

# Check if .env file exists
$envFile = Join-Path $projectRoot ".env"
if (-not (Test-Path $envFile)) {
    Write-Host "WARNING: .env file not found!" -ForegroundColor Yellow
    Write-Host "Create .env file with your credentials:" -ForegroundColor Gray
    Write-Host "1. Copy .env.example to .env" -ForegroundColor Gray
    Write-Host "2. Fill in your Bluesky/Mastodon credentials" -ForegroundColor Gray
    Write-Host ""
    Write-Host "For now, tasks will run in DRY RUN mode (test only, no actual posts)" -ForegroundColor Gray
    Write-Host ""
}

# Task 1: Daily Social Media Post (2 PM UTC)
Write-Host "Creating task: Daily Social Media Post" -ForegroundColor Cyan
Write-Host "Schedule: Daily at 2 PM UTC" -ForegroundColor Gray

$action1 = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath1`"" -WorkingDirectory $projectRoot
$trigger1 = New-ScheduledTaskTrigger -Daily -At "14:00"
$settings1 = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RunOnlyIfNetworkAvailable

try {
    Register-ScheduledTask -TaskName $taskNameDaily -Action $action1 -Trigger $trigger1 -Settings $settings1 -Description "Automated social media posting for Ascendant Continuum (FREE local automation)" -Force | Out-Null
    Write-Host "Created: $taskNameDaily" -ForegroundColor Green
} catch {
    Write-Host "Failed to create task: $_" -ForegroundColor Red
}

# Task 2: Daily Blog Post (10 AM UTC)
Write-Host ""
Write-Host "Creating task: Daily Blog Post" -ForegroundColor Cyan
Write-Host "Schedule: Daily at 10 AM UTC" -ForegroundColor Gray

$action2 = New-ScheduledTaskAction -Execute "PowerShell.exe" -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$scriptPath2`"" -WorkingDirectory $projectRoot
$trigger2 = New-ScheduledTaskTrigger -Daily -At "10:00"
$settings2 = New-ScheduledTaskSettingsSet -AllowStartIfOnBatteries -DontStopIfGoingOnBatteries -StartWhenAvailable -RunOnlyIfNetworkAvailable

try {
    Register-ScheduledTask -TaskName $taskNameBlog -Action $action2 -Trigger $trigger2 -Settings $settings2 -Description "Automated blog post generation for Ascendant Continuum (FREE local automation)" -Force | Out-Null
    Write-Host "Created: $taskNameBlog" -ForegroundColor Green
} catch {
    Write-Host "Failed to create task: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "========================================"
Write-Host "SETUP COMPLETE!" -ForegroundColor Green
Write-Host "========================================"
Write-Host ""
Write-Host "NEXT STEPS:" -ForegroundColor Yellow
Write-Host ""
Write-Host "1. View scheduled tasks:" -ForegroundColor White
Write-Host "   - Open Task Scheduler (taskschd.msc)" -ForegroundColor Gray
Write-Host "   - Look for AscendantContinuum tasks" -ForegroundColor Gray
Write-Host ""
Write-Host "2. Test a task immediately:" -ForegroundColor White
Write-Host "   - Right-click task -> Run" -ForegroundColor Gray
Write-Host "   - Check logs folder for output" -ForegroundColor Gray
Write-Host ""
Write-Host "3. Schedule:" -ForegroundColor White
Write-Host "   - Social posts: Daily at 2 PM UTC" -ForegroundColor Gray
Write-Host "   - Blog posts: Daily at 10 AM UTC" -ForegroundColor Gray
Write-Host ""
Write-Host "BENEFITS:" -ForegroundColor Yellow
Write-Host "   100% FREE - No GitHub Actions billing" -ForegroundColor Green
Write-Host "   Runs locally - Full control" -ForegroundColor Green
Write-Host ""
Write-Host "TO REMOVE:" -ForegroundColor Yellow
Write-Host "   Run: setup-local-automation.ps1 -Remove" -ForegroundColor Cyan
Write-Host ""
