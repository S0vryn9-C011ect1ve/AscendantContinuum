#!/usr/bin/env pwsh
# Quick Automation Test & Deployment Script
# Run with: .\deploy-automation.ps1

Write-Host "`n🚀 Automation Deployment & Test Script" -ForegroundColor Cyan
Write-Host "═" * 60 -ForegroundColor Gray

# Test 1: Social Media Automation (Dry Run)
Write-Host "`n📱 Test 1: Social Media Posting (DRY RUN)" -ForegroundColor Yellow
$env:DRY_RUN = "true"
try {
    node scripts/automation/content-scheduler.js
    Write-Host "✅ Social media automation test completed" -ForegroundColor Green
} catch {
    Write-Host "❌ Social media test failed: $_" -ForegroundColor Red
}

# Test 2: Blog Generation (Dry Run)
Write-Host "`n📝 Test 2: Blog Generation (DRY RUN)" -ForegroundColor Yellow
try {
    node scripts/automation/post-blog.js
    Write-Host "✅ Blog generation test completed" -ForegroundColor Green
} catch {
    Write-Host "❌ Blog generation failed: $_" -ForegroundColor Red
}

# Test 3: Deploy to Firebase
Write-Host "`n🔥 Test 3: Deploying Blog to Firebase Hosting" -ForegroundColor Yellow
Set-Location firebase
try {
    firebase deploy --only hosting
    Write-Host "✅ Firebase deployment completed" -ForegroundColor Green
} catch {
    Write-Host "❌ Firebase deployment failed: $_" -ForegroundColor Red
}
Set-Location ..

# Summary
Write-Host "`n" + ("═" * 60) -ForegroundColor Gray
Write-Host "📋 Deployment Summary" -ForegroundColor Cyan
Write-Host  ("═" * 60) -ForegroundColor Gray

Write-Host "`n✅ All tests completed!" -ForegroundColor Green
Write-Host "`n💡 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Check blog at: https://ascendantcontinuum.web.app/blog/" -ForegroundColor White
Write-Host "2. Manually trigger workflows at: https://github.com/S0vryn9-C011ect1ve/AscendantContinuum/actions" -ForegroundColor White
Write-Host "3. Monitor first auto-run tomorrow morning" -ForegroundColor White

Write-Host "`n📚 Read full guide: ./AUTOMATION_DEPLOYMENT_GUIDE.md`n" -ForegroundColor Cyan
