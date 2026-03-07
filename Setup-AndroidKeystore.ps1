<#
.SYNOPSIS
    Creates the Android release keystore for The Ascendant Continuum.

.DESCRIPTION
    Runs keytool (from the JDK bundled with Unity or from PATH) to generate
    a 4096-bit RSA keystore valid for 30 years.

    The generated file is   android-release.keystore
    The key alias is        ascendant-key

    IMPORTANT: Store the keystore file and passwords somewhere secure
    (a password manager, 1Password, Bitwarden, etc.).  If you lose the
    keystore you cannot update your app on Google Play.

.NOTES
    Run this script ONCE before your first Play Store submission.
    After running, open Unity → Project Settings → Player → Android →
    Publishing Settings and point to the keystore file.

    Unity Hub default JDK path (adjust if yours differs):
      C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe
#>

param(
    [string]$KeystoreFile   = "android-release.keystore",
    [string]$KeyAlias       = "ascendant-key",
    [string]$Validity       = "10950",   # 30 years in days
    [string]$KeySize        = "4096",
    [string]$Dname          = "CN=AscendantContinuum, OU=Game, O=AscendantContinuum, L=Unknown, ST=Unknown, C=US"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ── Locate keytool ───────────────────────────────────────────────────────────
$keytool = $null

$candidates = @(
    "keytool",   # system PATH
    "${env:JAVA_HOME}\bin\keytool.exe",
    "C:\Program Files\Unity\Hub\Editor\6000.3.9f1\Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\keytool.exe",
    "C:\Program Files\Microsoft\jdk-17.0.9.8-hotspot\bin\keytool.exe"
)

foreach ($c in $candidates) {
    if ($c -and (Get-Command $c -ErrorAction SilentlyContinue)) {
        $keytool = $c
        break
    }
}

if (-not $keytool) {
    Write-Error @"
keytool not found. Please either:
  1. Add your JDK bin/ to PATH, or
  2. Edit this script's \$candidates array with the correct path.
JDK is bundled with Unity: Editor\Data\PlaybackEngines\AndroidPlayer\OpenJDK\bin\
"@
    exit 1
}

Write-Host "Using keytool: $keytool" -ForegroundColor Cyan

# ── Check for existing keystore ──────────────────────────────────────────────
if (Test-Path $KeystoreFile) {
    Write-Warning "Keystore already exists at: $KeystoreFile"
    $overwrite = Read-Host "Overwrite? This CANNOT be undone if it's already used on Play Store [y/N]"
    if ($overwrite -ne "y") { exit 0 }
}

# ── Prompt for passwords ─────────────────────────────────────────────────────
Write-Host ""
Write-Host "Enter a strong password for the keystore (min 6 chars). SAVE THIS PASSWORD." -ForegroundColor Yellow
$ksPass  = Read-Host -AsSecureString "  Keystore password"
$ksPTxt  = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
               [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($ksPass))

Write-Host "Enter a password for the key alias (can be the same as keystore). SAVE THIS." -ForegroundColor Yellow
$keyPass = Read-Host -AsSecureString "  Key alias password"
$keyPTxt = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto(
               [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($keyPass))

if ($ksPTxt.Length -lt 6 -or $keyPTxt.Length -lt 6) {
    Write-Error "Passwords must be at least 6 characters."
    exit 1
}

# ── Generate keystore ────────────────────────────────────────────────────────
Write-Host "`nGenerating keystore..." -ForegroundColor Green

& $keytool `
    -genkeypair `
    -v `
    -keystore $KeystoreFile `
    -alias $KeyAlias `
    -keyalg RSA `
    -keysize $KeySize `
    -validity $Validity `
    -storepass $ksPTxt `
    -keypass $keyPTxt `
    -dname $Dname

if ($LASTEXITCODE -ne 0) {
    Write-Error "keytool failed with exit code $LASTEXITCODE"
    exit $LASTEXITCODE
}

# ── Write a .env.keystore reminder (DO NOT COMMIT) ──────────────────────────
$envContent = @"
# Android keystore — DO NOT COMMIT THIS FILE
KEYSTORE_FILE=$KeystoreFile
KEYSTORE_ALIAS=$KeyAlias
# Passwords stored here for reference only — use a password manager instead
# KEYSTORE_PASS=<your-password>
# KEY_PASS=<your-alias-password>
"@

$envFile = ".env.keystore"
Set-Content -Path $envFile -Value $envContent
Write-Host "Reminder file written to $envFile (add it to .gitignore)" -ForegroundColor Cyan

# ── Verify ───────────────────────────────────────────────────────────────────
& $keytool -list -v -keystore $KeystoreFile -storepass $ksPTxt

Write-Host @"

━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
✅  Keystore created: $KeystoreFile
    Alias:            $KeyAlias

Next steps:
  1. Open Unity → Project Settings → Player → Android →
     Publishing Settings
  2. Enable 'Custom Keystore'
  3. Browse to: $(Resolve-Path $KeystoreFile)
  4. Enter the passwords you just created
  5. BACK UP the keystore file + passwords — losing them means
     you cannot update the app on Google Play EVER.

Add to .gitignore:
  android-release.keystore
  .env.keystore
━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━
"@ -ForegroundColor Green
