# Unity IAP Implementation Guide

**Status:** Implementation required for production monetization  
**Updated:** May 29, 2026

## Current State

The `CosmicPatronManager` is **stubbed out** and uses `#if UNITY_PURCHASING` conditionals. IAP calls are simulated in the Editor but won't work in production builds without the Unity IAP package.

## Implementation Steps

### 1. Install Unity IAP Package

```
Unity Editor → Window → Package Manager → Unity Registry → In-App Purchasing → Install
```

**Package:** `com.unity.purchasing` (version 4.10+ recommended)

### 2. Enable IAP in Project Settings

```
Edit → Project Settings → Services → In-App Purchasing → Enable
```

### 3. Platform-Specific Setup

#### **Google Play (Android)**
1. Create app in Google Play Console
2. Navigate to Monetization → Products → In-app products
3. Create products with these IDs:
   - `com.ascendantcontinuum.patron_monthly` - Subscription - $1.99/month
   - `com.ascendantcontinuum.sigil_flame` - Managed Product - $0.99
   - `com.ascendantcontinuum.sigil_verdant` - Managed Product - $0.99
   - `com.ascendantcontinuum.constellation_pack` - Managed Product - $2.99
   - `com.ascendantcontinuum.seekers_archive` - Managed Product - $2.99
   - `com.ascendantcontinuum.cosmic_name_bundle` - Managed Product - $0.99
   - `com.ascendantcontinuum.tip_small` - Consumable - $0.99
   - `com.ascendantcontinuum.tip_medium` - Consumable - $2.99

4. Get Base64 Public Key from Google Play Console
5. Add to Unity: Edit → Project Settings → Services → In-App Purchasing → Google Play → Paste key

#### **Apple App Store (iOS)**
1. Create app in App Store Connect
2. Navigate to Features → In-App Purchases
3. Create products with same IDs (Apple auto-prefixes bundle ID)
4. Submit for review along with app submission

### 4. Update CosmicPatronManager.cs

Replace the stubbed `InitialiseIAP()` method:

```csharp
private void InitialiseIAP()
{
#if UNITY_PURCHASING
    var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
    
    // Subscription
    builder.AddProduct(PRODUCT_PATRON_MONTHLY, ProductType.Subscription);
    
    // Non-consumables (cosmetics)
    builder.AddProduct(PRODUCT_SIGIL_FLAME, ProductType.NonConsumable);
    builder.AddProduct(PRODUCT_SIGIL_VERDANT, ProductType.NonConsumable);
    builder.AddProduct(PRODUCT_CONSTELLATION_PACK, ProductType.NonConsumable);
    builder.AddProduct(PRODUCT_SEEKERS_ARCHIVE, ProductType.NonConsumable);
    builder.AddProduct(PRODUCT_COSMIC_NAME_BUNDLE, ProductType.NonConsumable);
    
    // Consumables (tip jar)
    builder.AddProduct(PRODUCT_TIP_JAR_SMALL, ProductType.Consumable);
    builder.AddProduct(PRODUCT_TIP_JAR_MEDIUM, ProductType.Consumable);
    
    UnityPurchasing.Initialize(this, builder);
    Debug.Log("[CosmicPatron] Unity IAP initialized.");
#else
    Debug.LogWarning("[CosmicPatron] Unity IAP package not installed. Purchases disabled.");
#endif
}
```

### 5. Implement IStoreListener Interface

Add to `CosmicPatronManager`:

```csharp
public class CosmicPatronManager : MonoBehaviour, IStoreListener
{
    private IStoreController m_StoreController;
    
    public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
    {
        m_StoreController = controller;
        Debug.Log("[CosmicPatron] IAP ready.");
    }
    
    public void OnInitializeFailed(InitializationFailureReason error)
    {
        Debug.LogError($"[CosmicPatron] IAP init failed: {error}");
    }
    
    public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args)
    {
        string productId = args.purchasedProduct.definition.id;
        ProcessSuccessfulPurchase(productId);
        return PurchaseProcessingResult.Complete;
    }
    
    public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
    {
        ProcessFailedPurchase(product.definition.id);
    }
}
```

### 6. Update InitiatePurchase Method

```csharp
private void InitiatePurchase(string productId)
{
#if UNITY_PURCHASING
    if (m_StoreController != null)
    {
        m_StoreController.InitiatePurchase(productId);
    }
    else
    {
        Debug.LogWarning("[CosmicPatron] Store not initialized.");
        OnPurchaseFailed?.Invoke(productId);
    }
#elif UNITY_EDITOR
    // Dev simulation
    Debug.Log($"[CosmicPatron] EDITOR SIMULATION: Granting {productId}");
    ProcessSuccessfulPurchase(productId);
#else
    Debug.LogWarning($"[CosmicPatron] IAP not configured. Blocked: {productId}");
    HUDManager.Instance?.ShowNotification("Purchases coming soon!", HUDManager.NotificationType.Info);
    OnPurchaseFailed?.Invoke(productId);
#endif
}
```

### 7. Test IAP

1. **Editor:** Simulation works automatically
2. **Android:** Use Google Play Internal Test Track
3. **iOS:** Use TestFlight sandbox environment

## Ethical Safeguards Implemented

1. **Spending Cap Warning:** Alert at $10/month total
2. **No Virtual Currency:** All prices in real USD
3. **Restore Purchases:** Always visible and functional
4. **Refund-Friendly:** Follow platform policies transparently
5. **Archive Unlocks:** Become free after 90 days
6. **Tip Jars:** Optional support, no rewards attached

## Testing Checklist

- [ ] Install Unity IAP package
- [ ] Configure Google Play products
- [ ] Configure App Store products
- [ ] Implement IStoreListener interface
- [ ] Test purchase flow in Editor (simulation)
- [ ] Test real purchases on Android test track
- [ ] Test real purchases on iOS TestFlight
- [ ] Verify receipt validation
- [ ] Test restore purchases functionality
- [ ] Verify subscription renewal/cancellation
- [ ] Test parental controls blocking

## Support Documentation

- [Unity IAP Documentation](https://docs.unity3d.com/Manual/UnityIAP.html)
- [Google Play Billing](https://developer.android.com/google/play/billing)
- [App Store Connect](https://developer.apple.com/app-store-connect/)
