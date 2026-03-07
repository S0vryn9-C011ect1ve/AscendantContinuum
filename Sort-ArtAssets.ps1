# Sort-ArtAssets.ps1
# Automatically moves images from _Incoming into the correct Unity Art subfolder
# based on filename patterns from the ASSET_PRODUCTION_GUIDE.
#
# USAGE: Run this after copying all images from your phone into:
#        Assets\_Project\Art\_Incoming\

$root     = "D:\1-Ascendant Continuum Game\Assets\_Project\Art"
$incoming = "$root\_Incoming"

if (-not (Test-Path $incoming)) {
    Write-Host "ERROR: _Incoming folder not found at $incoming" -ForegroundColor Red
    exit 1
}

$files = Get-ChildItem $incoming -File -Include "*.png","*.jpg","*.jpeg","*.webp" -Recurse
if ($files.Count -eq 0) {
    Write-Host "No image files found in _Incoming. Copy your phone images there first." -ForegroundColor Yellow
    exit 0
}

Write-Host "`n=== Ascendant Continuum Art Sorter ===" -ForegroundColor Cyan
Write-Host "Found $($files.Count) image(s) in _Incoming`n"

# --- Routing rules: ordered from most-specific to most-general ---
$rules = [ordered]@{
    # Backgrounds
    "^bg_"                        = "Backgrounds"
    # Particle textures
    "^particle_"                  = "Particles"
    # Realm icons + shared UI icons
    "^icon_"                      = "Sprites\UI"
    "^splash_"                    = "Sprites\UI"
    "^sigil_placeholder"          = "Sprites\UI"
    "^panel_"                     = "Sprites\UI"
    "^button_"                    = "Sprites\UI"
    "^progress_bar"               = "Sprites\UI"
    "^fade_"                      = "Sprites\UI"
    "^app_icon"                   = "Sprites\UI"
    # Emberforge
    "^spark_"                     = "Sprites\Realms\Emberforge"
    # Verdant
    "^plant_|^soil_"              = "Sprites\Realms\Verdant"
    # Echo Fields
    "^star_|^constellation_"      = "Sprites\Realms\EchoFields"
    # Dawn Citadel
    "^prism_|^light_beam|^target_" = "Sprites\Realms\DawnCitadel"
    # Lantern Ascension
    "^lantern_|^wish_"            = "Sprites\Realms\LanternAscension"
}

$moved   = @()
$unknown = @()

foreach ($file in $files) {
    $name        = $file.Name.ToLower()
    $destination = $null

    foreach ($pattern in $rules.Keys) {
        if ($name -match $pattern) {
            $destination = Join-Path $root $rules[$pattern]
            break
        }
    }

    if ($null -ne $destination) {
        New-Item -ItemType Directory -Force -Path $destination | Out-Null
        $destFile = Join-Path $destination $file.Name

        # Warn on overwrite
        if (Test-Path $destFile) {
            Write-Host "  OVERWRITE  $($file.Name)  →  $($rules[$($rules.Keys | Where-Object { $name -match $_ } | Select-Object -First 1)])" -ForegroundColor Yellow
        } else {
            Write-Host "  OK  $($file.Name)  →  $($destination.Replace($root, ''))" -ForegroundColor Green
        }

        Move-Item -Path $file.FullName -Destination $destFile -Force
        $moved += $file.Name
    } else {
        $unknown += $file.Name
        Write-Host "  ??  $($file.Name)  — no matching rule, left in _Incoming" -ForegroundColor Magenta
    }
}

Write-Host "`n--- Summary ---" -ForegroundColor Cyan
Write-Host "Sorted:    $($moved.Count) file(s)" -ForegroundColor Green
if ($unknown.Count -gt 0) {
    Write-Host "Unknown:   $($unknown.Count) file(s) left in _Incoming — see below" -ForegroundColor Magenta
    $unknown | ForEach-Object { Write-Host "  - $_" }
    Write-Host "`nFor unknown files, rename them to match the ASSET_PRODUCTION_GUIDE naming"
    Write-Host "convention (e.g. bg_emberforge.png, spark_small.png) and run this script again."
} else {
    Write-Host "All files sorted successfully!" -ForegroundColor Green
}

Write-Host "`nNext step: Open Unity and run:"
Write-Host "  Ascendant Continuum → Setup → Full Game Setup (Run All)" -ForegroundColor Cyan
