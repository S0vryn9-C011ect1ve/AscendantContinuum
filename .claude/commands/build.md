# /build Command

Quick build command for common platforms.

## Usage

```
/build [platform] [type]
```

## Parameters

- `platform` (optional): `android` | `ios` | `webgl` | `all` (default: `webgl`)
- `type` (optional): `development` | `release` (default: `development`)

## Examples

```
/build
/build android
/build android release
/build all release
```

## Implementation

```powershell
# WebGL Development
.\Build.ps1 -Platform WebGL -BuildType Development

# Android Release
.\Build.ps1 -Platform Android -BuildType Release

# All platforms
.\Build.ps1 -Platform All -BuildType Release
```

## Common Issues

### "Unity Android Build Support not found"
**Fix:** Run `.\Setup-UnityModules.ps1` to install Android modules

### "Unity process is running"
**Fix:** Close Unity Editor before building via script

### "Build failed with errors"
**Fix:** Check Unity Console for compilation errors first
