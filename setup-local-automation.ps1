# Setup Local Automation with Windows Task Scheduler
# 100% FREE - Runs on your local machine, no cloud costs

param(
    [switch]$DryRun = $false,
    [switch]$Remove = $false
)

$ErrorActionPreference = "Stop"
$projectRoot = "d:\1-Ascendant Continuum Game"

Write-Host "`n🚀 LOCAL AUTOMATION SETUP (100% FREE)`n" -ForegroundColor Cyan
Write-Host "This will create Windows Task Scheduler tasks to automate social media posting" -ForegroundColor Gray
Write-Host "completely FREE on your local machine - no GitHub Actions billing!`n" -ForegroundColor Gray

# Check if running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Host "❌ This script requires Administrator privileges" -ForegroundColor Red
    Write-Host "   Right-click PowerShell and 'Run as Administrator', then run this script again" -ForegroundColor Yellow
    exit 1
}

# Task names
$taskNameDaily = "AscendantContinuum-DailySocial"
$taskNameBlog = "AscendantContinuum-DailyBlog"

if ($Remove) {
    Write-Host "🗑️  Removing existing tasks..." -ForegroundColor Yellow
    
    try {
        Unregister-ScheduledTask -TaskName $taskNameDaily -Confirm:$false -ErrorAction SilentlyContinue
        Write-Host "   ✅ Removed: $taskNameDaily" -ForegroundColor Green
    } catch {
        Write-Host "   ℹ️  Task not found: $taskNameDaily" -ForegroundColor Gray
    }
    
    try {
        Unregister-ScheduledTask -TaskName $taskNameBlog -Confirm:$false -ErrorAction SilentlyContinue
        Write-Host "   ✅ Removed: $taskNameBlog" -ForegroundColor Green
    } catch {
        Write-Host "   ℹ️  Task not found: $taskNameBlog" -ForegroundColor Gray
    }
    
    Write-Host "`n✅ Cleanup complete!`n" -ForegroundColor Green
    exit 0
}

# Check if .env file exists
$envFile = "$projectRoot\.env"
if (-not (Test-Path $envFile)) {
    Write-Host "⚠️  WARNING: .env file not found!" -ForegroundColor Yellow
    Write-Host "   Create .env file with your credentials:" -ForegroundColor Gray
    Write-Host "   1. Copy .env.example to .env" -ForegroundColor Gray
    Write-Host "   2. Fill in your Bluesky/Mastodon credentials" -ForegroundColor Gray
    Write-Host "`n   For now, tasks will run in DRY RUN mode (test only, no actual posts)`n" -ForegroundColor Gray
}

# Task 1: Daily Social Media Post (2 PM UTC = 9 AM EST)
Write-Host "📅 Creating task: Daily Social Media Post" -ForegroundColor Cyan
Write-Host "   Schedule: Daily at 2 PM UTC (9 AM EST)" -ForegroundColor Gray

$action1 = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$projectRoot\scripts\automation\run-daily-social.ps1`"" `
    -WorkingDirectory $projectRoot

$trigger1 = New-ScheduledTaskTrigger -Daily -At "14:00" # 2 PM UTC

$settings1 = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable

try {
    Register-ScheduledTask -TaskName $taskNameDaily `
        -Action $action1 `
        -Trigger $trigger1 `
        -Settings $settings1 `
        -Description "Automated social media posting for Ascendant Continuum (FREE local automation)" `
        -Force | Out-Null
    
    Write-Host "   ✅ Created: $taskNameDaily" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Failed to create task: $_" -ForegroundColor Red
}

# Task 2: Daily Blog Post (10 AM UTC = 5 AM EST)
Write-Host "`n📅 Creating task: Daily Blog Post" -ForegroundColor Cyan
Write-Host "   Schedule: Daily at 10 AM UTC (5 AM EST)" -ForegroundColor Gray

$action2 = New-ScheduledTaskAction -Execute "PowerShell.exe" `
    -Argument "-NoProfile -ExecutionPolicy Bypass -File `"$projectRoot\scripts\automation\run-daily-blog.ps1`"" `
    -WorkingDirectory $projectRoot

$trigger2 = New-ScheduledTaskTrigger -Daily -At "10:00" # 10 AM UTC

$settings2 = New-ScheduledTaskSettingsSet `
    -AllowStartIfOnBatteries `
    -DontStopIfGoingOnBatteries `
    -StartWhenAvailable `
    -RunOnlyIfNetworkAvailable

try {
    Register-ScheduledTask -TaskName $taskNameBlog `
        -Action $action2 `
        -Trigger $trigger2 `
        -Settings $settings2 `
        -Description "Automated blog post generation for Ascendant Continuum (FREE local automation)" `
        -Force | Out-Null
    
    Write-Host "   ✅ Created: $taskNameBlog" -ForegroundColor Green
} catch {
    Write-Host "   ❌ Failed to create task: $_" -ForegroundColor Red
}

Write-Host "`n" -NoNewline
Write-Host "========================================" -ForegroundColor Green
Write-Host "✅ LOCAL AUTOMATION SETUP COMPLETE!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Green
Write-Host "`n📋 NEXT STEPS:`n" -ForegroundColor Yellow

Write-Host "1. Configure credentials (if not done yet):" -ForegroundColor White
Write-Host "   • Copy .env.example to .env" -ForegroundColor Gray
Write-Host "   • Fill in your Bluesky/Mastodon credentials" -ForegroundColor Gray

Write-Host "`n2. Test the automation:" -ForegroundColor White
Write-Host "   • Run manually: " -ForegroundColor Gray -NoNewline
Write-Host ".\scripts\automation\run-daily-social.ps1 -DryRun" -ForegroundColor Cyan
Write-Host "   • Check logs: " -ForegroundColor Gray -NoNewline
Write-Host ".\logs\" -ForegroundColor Cyan

Write-Host "`n3. View scheduled tasks:" -ForegroundColor White
Write-Host "   • Open Task Scheduler (taskschd.msc)" -ForegroundColor Gray
Write-Host "   • Look for 'AscendantContinuum-DailySocial' and 'AscendantContinuum-DailyBlog'" -ForegroundColor Gray

Write-Host "`n4. Run a task immediately:" -ForegroundColor White
Write-Host "   • Right-click task in Task Scheduler → 'Run'" -ForegroundColor Gray
Write-Host "   • Or use PowerShell: " -ForegroundColor Gray -NoNewline
Write-Host "Start-ScheduledTask -TaskName '$taskNameDaily'" -ForegroundColor Cyan

Write-Host "`n💡 BENEFITS OF LOCAL AUTOMATION:" -ForegroundColor Yellow
Write-Host "   ✅ 100% FREE - No GitHub Actions billing" -ForegroundColor Green
Write-Host "   ✅ Runs on your machine - No cloud costs" -ForegroundColor Green
Write-Host "   ✅ Full control - Edit scripts anytime" -ForegroundColor Green
Write-Host "   ✅ Logs saved locally - Easy debugging" -ForegroundColor Green

Write-Host "`n⚠️  REQUIREMENTS:" -ForegroundColor Yellow
Write-Host "   • Your computer must be ON at scheduled times" -ForegroundColor Gray
Write-Host "   • Internet connection required when posting" -ForegroundColor Gray
Write-Host "   • Tasks run even if you're logged out (as long as PC is on)" -ForegroundColor Gray

Write-Host "`n🗑️  TO REMOVE AUTOMATION:" -ForegroundColor Yellow
Write-Host "   Run: " -ForegroundColor Gray -NoNewline
Write-Host ".\setup-local-automation.ps1 -Remove" -ForegroundColor Cyan

Write-Host "`n"
