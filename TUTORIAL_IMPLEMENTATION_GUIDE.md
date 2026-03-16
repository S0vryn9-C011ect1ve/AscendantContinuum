# 📚 Tutorial Implementation Guide - Onboarding Flow
**3-Step Interactive Tutorial in Emberforge**

## Goal
New players learn core mechanics in 2-3 minutes without reading walls of text.

---

## 🎯 Tutorial Design Principles

1. **Show, Don't Tell** - Players do actions, not read about them
2. **3 Steps Maximum** - Anything more = players skip
3. **Emberforge First** - Simplest realm, easiest mechanics
4. **Interruptible** - "Skip Tutorial" button for returning players
5. **Visual Hints** - Arrows, highlights, animations guide the player

---

## 📋 TUTORIAL STRUCTURE

### Step 1: Welcome & Tap to Collect (30 seconds)
**Goal:** Player understands tap-to-collect mechanic

### Step 2: Collect 10 Sparks (60 seconds)
**Goal:** Player completes first ritual

### Step 3: Unlock Journey (30 seconds)
**Goal:** Player understands progression (sigils → realm unlocks)

**Total time:** ~2-3 minutes

---

## 🛠️ IMPLEMENTATION

### Part 1: Setup Onboarding Scene (15 min)

1. **Open Onboarding.unity:**
   ```
   Assets/_Project/Scenes/Core/Onboarding.unity
   ```

2. **Scene should contain:**
   - Main Camera
   - Canvas (for tutorial UI)
   - EventSystem

3. **Add background:**
   - Use Emberforge background (`bg_emberforge.png`)
   - This tutorial happens IN Emberforge realm

4. **Create TutorialManager GameObject:**
   - Hierarchy → Create Empty
   - Rename "TutorialManager"
   - Add Component → Create "TutorialManager.cs" script (see below)

---

### Part 2: Create TutorialManager Script (30 min)

Create new script: `Assets/_Project/Scripts/Core/TutorialManager.cs`

```csharp
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class TutorialManager : MonoBehaviour
{
    [Header("Tutorial Steps")]
    public GameObject step1Panel;
    public GameObject step2Panel;
    public GameObject step3Panel;
    
    [Header("Tutorial UI")]
    public TextMeshProUGUI instructionText;
    public Button skipButton;
    public GameObject handArrow; // Visual pointer
    
    [Header("Emberforge Elements")]
    public GameObject sparkPrefab;
    public Transform sparksContainer;
    public TextMeshProUGUI sparkCountText;
    
    private int currentStep = 0;
    private int sparksCollected = 0;
    private const int SPARKS_NEEDED = 10;
    
    void Start()
    {
        // Check if player has completed tutorial before
        if (PlayerPrefs.GetInt("TutorialCompleted", 0) == 1)
        {
            // Skip to main menu
            SceneManager.LoadScene("MainMenu");
            return;
        }
        
        // Start tutorial
        skipButton.onClick.AddListener(SkipTutorial);
        StartStep1();
    }
    
    void StartStep1()
    {
        currentStep = 1;
        step1Panel.SetActive(true);
        step2Panel.SetActive(false);
        step3Panel.SetActive(false);
        
        instructionText.text = "Welcome, Seeker.\n\nTap a spark to collect it.";
        
        // Spawn ONE spark with arrow pointing to it
        GameObject spark = Instantiate(sparkPrefab, new Vector3(0, 0, 0), Quaternion.identity, sparksContainer);
        spark.GetComponent<TutorialSpark>().OnCollected += OnFirstSparkCollected;
        
        // Show arrow pointing at spark
        handArrow.SetActive(true);
        handArrow.transform.position = spark.transform.position + Vector3.down * 1.5f;
    }
    
    void OnFirstSparkCollected()
    {
        handArrow.SetActive(false);
        sparksCollected = 1;
        UpdateSparkUI();
        
        // Wait 1 second, then start step 2
        Invoke(nameof(StartStep2), 1f);
    }
    
    void StartStep2()
    {
        currentStep = 2;
        step1Panel.SetActive(false);
        step2Panel.SetActive(true);
        
        instructionText.text = $"Collect {SPARKS_NEEDED} sparks to complete the ritual.";
        
        // Spawn remaining 9 sparks
        for (int i = 0; i < SPARKS_NEEDED - sparksCollected; i++)
        {
            Vector3 randomPos = new Vector3(
                Random.Range(-7f, 7f),
                Random.Range(-4f, 4f),
                0
            );
            
            GameObject spark = Instantiate(sparkPrefab, randomPos, Quaternion.identity, sparksContainer);
            spark.GetComponent<TutorialSpark>().OnCollected += OnSparkCollected;
        }
    }
    
    void OnSparkCollected()
    {
        sparksCollected++;
        UpdateSparkUI();
        
        if (sparksCollected >= SPARKS_NEEDED)
        {
            // Wait 1 second, then show step 3
            Invoke(nameof(StartStep3), 1f);
        }
    }
    
    void StartStep3()
    {
        currentStep = 3;
        step2Panel.SetActive(false);
        step3Panel.SetActive(true);
        
        instructionText.text = "Ritual complete! You've unlocked your journey.\n\nExplore 5 mystical realms and collect sigils.";
        
        // Show "Continue" button
        // (Add button in Inspector, link to CompleteTutorial method)
    }
    
    public void CompleteTutorial()
    {
        // Mark tutorial as completed
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        
        // Load main menu
        SceneManager.LoadScene("MainMenu");
    }
    
    public void SkipTutorial()
    {
        // Player chose to skip
        PlayerPrefs.SetInt("TutorialCompleted", 1);
        PlayerPrefs.Save();
        SceneManager.LoadScene("MainMenu");
    }
    
    void UpdateSparkUI()
    {
        sparkCountText.text = $"Sparks: {sparksCollected}/{SPARKS_NEEDED}";
    }
}
```

---

### Part 3: Create TutorialSpark Script (10 min)

Simplified spark for tutorial only.

Create: `Assets/_Project/Scripts/Tutorial/TutorialSpark.cs`

```csharp
using UnityEngine;
using System;

public class TutorialSpark : MonoBehaviour
{
    public event Action OnCollected;
    
    private bool isCollected = false;
    
    void OnMouseDown()
    {
        CollectSpark();
    }
    
    // For mobile touch
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Touch")) // Or however you handle touch
        {
            CollectSpark();
        }
    }
    
    void CollectSpark()
    {
        if (isCollected) return;
        
        isCollected = true;
        
        // Play sound effect (optional)
        // AudioManager.Instance.PlaySFX("spark_collect");
        
        // Play particle effect (optional)
        // Instantiate(collectParticle, transform.position, Quaternion.identity);
        
        // Notify TutorialManager
        OnCollected?.Invoke();
        
        // Destroy spark
        Destroy(gameObject, 0.1f);
    }
}
```

---

### Part 4: Create Tutorial UI (30 min)

1. **Create Canvas objects:**

**Step 1 Panel:**
- Canvas → Right-click → UI → Panel
- Rename "Step1Panel"
- Make it semi-transparent dark background
- Add TextMeshPro: "Welcome, Seeker.\n\nTap a spark to collect it."
- Position: Center screen

**Step 2 Panel:**
- Duplicate Step1Panel
- Rename "Step2Panel"
- Text: "Collect 10 sparks to complete the ritual."
- Set active: False (TutorialManager will show it)

**Step 3 Panel:**
- Duplicate Step2Panel
- Rename "Step3Panel"
- Text: "Ritual complete! You've unlocked your journey."
- Add Button: "Continue" → Links to TutorialManager.CompleteTutorial()
- Set active: False

**Spark Counter UI:**
- Canvas → UI → Text - TextMeshPro
- Position: Top-right
- Text: "Sparks: 0/10"
- Font size: 48

**Skip Button:**
- Canvas → UI → Button
- Position: Top-right corner
- Text: "Skip Tutorial"
- OnClick → TutorialManager.SkipTutorial()

**Hand Arrow (Visual hint):**
- Import hand pointing sprite OR
- Use Unity default arrow (TextMeshPro icon)
- Animate: Bobbing up/down
- Points to first spark

---

### Part 5: Animate Hand Arrow (15 min - Optional)

**Option A: Simple Script**

```csharp
// HandArrowBob.cs
using UnityEngine;

public class HandArrowBob : MonoBehaviour
{
    public float bobSpeed = 2f;
    public float bobHeight = 0.3f;
    
    private Vector3 startPos;
    
    void Start()
    {
        startPos = transform.position;
    }
    
    void Update()
    {
        float newY = startPos.y + Mathf.Sin(Time.time * bobSpeed) * bobHeight;
        transform.position = new Vector3(startPos.x, newY, startPos.z);
    }
}
```

Attach to hand arrow GameObject.

**Option B: Unity Animator**
- Create idle animation that moves Y position up/down
- Loop animation

---

### Part 6: Link Everything in Inspector (10 min)

1. **Select TutorialManager GameObject**

2. **Assign references:**
   - Step 1 Panel → Drag Step1Panel
   - Step 2 Panel → Drag Step2Panel
   - Step 3 Panel → Drag Step3Panel
   - Instruction Text → Drag from one of the panels
   - Skip Button → Drag Skip button
   - Hand Arrow → Drag arrow GameObject
   - Spark Prefab → Drag your Spark prefab
   - Sparks Container → Create empty GameObject "SparksContainer", drag it
   - Spark Count Text → Drag spark counter UI

3. **Verify all fields assigned (none say "None")**

---

### Part 7: Test Tutorial Flow (10 min)

1. **Press Play in Onboarding scene**
2. **Expected flow:**
   - Step 1 panel appears
   - ONE spark visible with arrow pointing to it
   - Click spark → arrow disappears, spark count updates
   - Step 2 panel appears
   - 9 more sparks spawn randomly
   - Collect all 10
   - Step 3 panel with "Continue" button
   - Click Continue → loads MainMenu scene

3. **Test Skip button:**
   - Restart scene
   - Click "Skip Tutorial"
   - Should load MainMenu immediately

4. **Check Console for errors**

---

## 🎨 VISUAL POLISH (Optional - 30 min)

### Add Transitions Between Steps

1. **Install DOTween (free asset):**
   - Window → Package Manager → Unity Registry
   - Search "DOTween"
   - Install

2. **Fade panels in/out:**
```csharp
using DG.Tweening;

void ShowPanel(GameObject panel)
{
    panel.SetActive(true);
    CanvasGroup cg = panel.GetComponent<CanvasGroup>();
    if (cg == null) cg = panel.AddComponent<CanvasGroup>();
    
    cg.alpha = 0;
    cg.DOFade(1, 0.5f);
}

void HidePanel(GameObject panel)
{
    CanvasGroup cg = panel.GetComponent<CanvasGroup>();
    cg.DOFade(0, 0.5f).OnComplete(() => panel.SetActive(false));
}
```

### Add Spark Collect Animation

When spark collected:
- Scale up briefly
- Fade out
- Particle burst

```csharp
void CollectSpark()
{
    transform.DOScale(1.5f, 0.2f);
    SpriteRenderer sr = GetComponent<SpriteRenderer>();
    sr.DOFade(0, 0.3f).OnComplete(() => Destroy(gameObject));
}
```

### Add Sound Effects

```csharp
void CollectSpark()
{
    AudioManager.Instance.PlaySFX("spark_collect");
    // ... rest of code
}
```

---

## 🔄 TUTORIAL FLOW DIAGRAM

```
Player opens game
    ↓
Check PlayerPrefs: "TutorialCompleted"?
    ↓ NO
Load Onboarding Scene
    ↓
[STEP 1: Welcome + Tap Spark]
    → Spawn 1 spark with arrow
    → Player taps
    → Spark collected ✅
    ↓
[STEP 2: Collect 10 Sparks]
    → Spawn 9 more sparks
    → Player collects all
    → Counter: 10/10 ✅
    ↓
[STEP 3: Journey Unlocked]
    → Show success message
    → "Continue" button appears
    → Player clicks
    ↓
Save "TutorialCompleted" = 1
    ↓
Load MainMenu Scene
```

---

## 🧪 TESTING CHECKLIST

Before moving on, verify:

- [ ] Tutorial starts automatically on first launch
- [ ] Step 1 shows ONE spark with visual hint
- [ ] Tapping first spark advances to Step 2
- [ ] Step 2 spawns 9 more sparks (10 total minus the 1 already collected)
- [ ] All sparks are tappable
- [ ] Spark counter updates correctly (0/10 → 1/10 → ... → 10/10)
- [ ] Completing Step 2 advances to Step 3
- [ ] Step 3 shows "Continue" button
- [ ] Clicking "Continue" loads MainMenu
- [ ] Tutorial saves completion state
- [ ] Restarting game skips tutorial (goes straight to MainMenu)
- [ ] "Skip Tutorial" button works at any step
- [ ] No errors in Console

---

## 🎯 ADVANCED: Optional 7-Day Onboarding

If you want to implement the full 7_DAY_EXPERIENCE.md plan:

**Day 1:** Emberforge tutorial (✅ covered above)  
**Day 2:** Unlock Verdant Sanctuary hint  
**Day 3:** First daily challenge appears  
**Day 4:** Cosmic Identity quiz + Pantheon deity selection  
**Day 5:** Sigil crafting unlocked  
**Day 6:** Time capsule feature introduced  
**Day 7:** Community mystery hint + full freedom

Each day = short 30-second popup explaining new feature on login.

**Implementation:**
1. Track "DaysSinceLaunch" in PlayerPrefs
2. Show tooltip on MainMenu based on day
3. Use same popup panel as tutorial

---

## 🆘 COMMON ISSUES

**Tutorial loops infinitely:**
- Check PlayerPrefs.SetInt("TutorialCompleted", 1) is called
- Verify SceneManager.LoadScene("MainMenu") is called after tutorial

**Sparks don't spawn:**
- Check sparkPrefab is assigned
- Check sparksContainer exists
- Add Debug.Log in spawn loop to verify it's running

**Spark tap not working:**
- Verify Spark has Collider2D component
- Check "Is Trigger" is enabled
- Ensure EventSystem is in scene (for UI and touch)
- Try OnMouseDown() for testing (works in editor)

**Skip button doesn't work:**
- Check button's OnClick event is linked to TutorialManager.SkipTutorial()
- Verify TutorialManager GameObject is active

**Hand arrow doesn't appear:**
- Check it's assigned in TutorialManager
- Verify it's not hidden behind UI (Z position)
- Check SetActive(true) is called in StartStep1()

---

## ✅ SUCCESS CRITERIA

Tutorial is complete when:
- [ ] New player sees tutorial on first launch
- [ ] Tutorial teaches tap-to-collect in <30 seconds
- [ ] Player completes first ritual successfully
- [ ] Tutorial explains realm progression
- [ ] Tutorial can be skipped
- [ ] Returning players bypass tutorial automatically
- [ ] Tutorial feels smooth, not tedious
- [ ] No critical bugs

---

## 🚀 NEXT STEPS

After tutorial complete:
1. **Test with fresh PlayerPrefs** (delete PlayerPrefs to simulate new player)
2. **Get feedback** from 2-3 people unfamiliar with game
3. **Iterate** based on confusion points
4. **Move to Firebase integration** (save tutorial completion to cloud)

Your game now has:
- ✅ Interactive tutorial
- ✅ Realm mechanics demonstration
- ✅ Onboarding for new players

Ready for Firebase setup! 🔥
