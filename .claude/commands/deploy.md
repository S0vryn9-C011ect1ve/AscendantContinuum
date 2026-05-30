# /deploy Command

Deploy Firebase services (Firestore rules, hosting, storage rules).

## Usage

```
/deploy [target]
```

## Parameters

- `target` (optional): `rules` | `hosting` | `all` (default: `all`)

## Examples

```
/deploy
/deploy rules
/deploy hosting
```

## Implementation

```powershell
# Deploy all Firebase services
.\Deploy-Firebase.ps1 -Target All

# Deploy Firestore rules only
.\Deploy-Firebase.ps1 -Target Rules

# Deploy hosting only
.\Deploy-Firebase.ps1 -Target Hosting
```

## Validation Steps

1. **Before Deploy:** Validate rules syntax
2. **Test in Emulator:** Use Firebase Emulator Suite
3. **Deploy to Prod:** Run deploy command
4. **Verify:** Check Firebase Console for deployment status

## Cost Monitoring

After deployment, verify:
- Firestore reads/writes < 50K/day
- Storage uploads < 1GB/month
- Hosting bandwidth < 10GB/month

**Target monthly cost:** < $5
