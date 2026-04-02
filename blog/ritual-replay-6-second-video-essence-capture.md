# Ritual Replay: 6-Second Video Essence Capture

**Date:** April 2, 2026  
**Theme:** Creative Expression  
**Tags:** ritual-replay, video-capture, essence-recording, sharing, 6-seconds

---

## Your Ritual, Preserved Forever (But Only 6 Seconds)

Complete any ritual.

**System automatically captures 6 seconds** of your most critical moment.

No recording apps. No editing. Pure essence.

**Then share it. Or keep it private. Your choice.**

---

## How Ritual Replay Works

### **Automatic Capture System**

**You don't press "record."**

The game watches your entire ritual and identifies **the peak moment**:

```csharp
// From RitualReplayManager.cs
public class ReplayMoment {
    public float timestamp;
    public float intensity; // Calculated from:
                            // - Visual FX density
                            // - Audio peak
                            // - Player input velocity
                            // - Sigil activation timing
}

void AnalyzeRitual(Ritual ritual) {
    List<ReplayMoment> moments = new List<ReplayMoment>();
    
    foreach (var frame in ritual.frames) {
        float intensity = CalculateIntensity(frame);
        moments.Add(new ReplayMoment(frame.time, intensity));
    }
    
    // Find peak 6-second window
    ReplayMoment peak = moments.OrderByDescending(m => m.intensity).First();
    CaptureReplay(peak.timestamp - 3f, peak.timestamp + 3f); // 6 sec total
}
```

**The moment YOU felt the ritual climax = the moment captured.**

---

## What Gets Captured

### **Video Elements:**

✅ **Your ritual gestures** (hand movements, sigil drawing)  
✅ **Visual FX** (particle systems, light beams, auras)  
✅ **Sigil manifestation** (the moment it activates)  
✅ **Environment** (weather, time of day, moon phase)  
✅ **Audio** (ambient sounds, ritual hum, activation sound)

❌ **Your face** (camera never shows you)  
❌ **Your location** (GPS data not visible)  
❌ **UI elements** (clean capture, no HUD)

**Cinematic replay, not screen recording.**

---

## The 6-Second Philosophy

### **Why 6 Seconds?**

**TikTok/Reels:** 15-60 seconds (requires editing, hooks, retention tactics)  
**Vine (RIP):** 6 seconds (pure creativity constraint)

**We chose 6 seconds because:**

1. **Forces essence over filler** (no time for boring setup)
2. **Respects viewer attention** (you can watch 20 replays in 2 minutes)
3. **Small file size** (easier sharing, faster uploads)
4. **Creativity through constraint** (limitation breeds innovation)

**6 seconds = one perfect moment.**

---

## Replay Gallery (In-Game)

### **Your Personal Collection**

**Access:** Main menu → Rituals → Replay Gallery

**Layout:**
- Thumbnail grid (3 columns)
- Chronological or "favorites" sort
- Filter by sigil, realm, success/failure

**Each replay shows:**
- 6-second video loop
- Ritual date/time
- Sigil used
- Success/failure indicator
- View count (if shared publicly)
- Comments (if public)

**Example:**
```
REPLAY #47
March 15, 2026 - 2:47 AM
Sigil: Lunar Bloom
Result: SUCCESS
Views: 142
Comments: 8

"This moment still gives me chills."
```

---

## Sharing Options

### **1. Keep Private (Default)**

Replay stored locally only. **You're the only one who sees it.**

### **2. Share with Followers**

If you have followers (opt-in social features), they can see your replays.

**Use case:** Small friend group sharing cool moments

### **3. Public Gallery**

Upload to community gallery. **Anyone can discover it.**

**Moderation:**
- Auto-scan for inappropriate content
- Community reporting
- Manual review for flagged replays

### **4. Export to Device**

Download as MP4 (1080p, ~2 MB file size).

Share on Instagram, TikTok, Discord, anywhere.

**No watermark.** (We're not monsters.)

---

## Replay Challenges

### **Weekly Community Challenges**

**Example:**

**"Moonlit Rituals" (Week 12)**

- Perform ANY ritual under full moon
- Capture replay
- Submit to "Moonlit Rituals" gallery
- Top 10 most-liked get exclusive moon-themed cosmetics

**Judging:**
- Community votes (50%)
- Aesthetic quality (30%)
- Creativity (20%)

**No skill barrier.** Beginners can win with beautiful shots.

---

## The Unexpected Creativity

**What we expected:**

Players sharing successful rituals (celebrations).

**What we're seeing (in alpha testing):**

### **1. Failure Montages**

Players sharing their most spectacular FAILURES:

**Example:**
- Sigil misfires
- Accidental explosion effects
- Comedic timing (ritual fails just as phone rings)

**"Fail Gallery" becomes most popular section.**

### **2. Environmental Storytelling**

Players performing rituals in visually stunning locations:

- Mountaintop at sunrise
- Beach during storm
- Snow-covered forest
- Urban rooftop

**Replay becomes photography.**

### **3. Choreographed Sequences**

Groups timing rituals to create **sequential replays**:

**Player A:** Starts ritual at 0:00  
**Player B:** Activates sigil at 0:06  
**Player C:** Completes at 0:12

**When viewed back-to-back:** Looks like one continuous ritual across space.

**Accidental collaborative art.**

---

## Technical Implementation

### **Recording System**

**Challenge:** Can't record entire ritual (battery drain, storage).

**Solution:** Rolling buffer.

```csharp
// Pseudocode
CircularBuffer<VideoFrame> buffer = new CircularBuffer<VideoFrame>(
    capacity: 180 frames // 6 seconds @ 30 FPS
);

void Update() {
    buffer.Add(CaptureCurrentFrame());
    
    if (RitualCompleted) {
        ReplayMoment peak = IdentifyPeakMoment();
        SaveReplay(buffer.GetRange(peak.startFrame, peak.endFrame));
    }
}
```

**Result:**
- Only stores last 6 seconds at any time
- Minimal memory footprint
- High quality (1080p)

### **AI Peak Detection**

**What makes a "peak moment"?**

Machine learning model trained on:
- Visual complexity (particle density)
- Audio loudness
- Input intensity (faster movements = higher intensity)
- Sigil activation (always peaks here)

**Model learns** what moments players find most exciting.

**Improves over time** with community votes.

---

## Privacy Controls

### **Default Settings:**

- ❌ Recording: OFF (opt-in only)
- ❌ Sharing: Private only
- ❌ GPS metadata: Stripped

**To enable:** Settings → Replays → "Enable Ritual Recording"

**You must actively choose** to record yourself.

**We don't record without consent.**

---

## Replay Analytics (For Creators)

**If you share publicly, you can see:**

- View count
- Like/comment count
- Replay completion rate (did they watch all 6 seconds?)
- Share count (how many exported it?)

**Example:**
```
REPLAY #47 ANALYTICS

Views: 1,247
Likes: 89
Comments: 12
Completion Rate: 94%
Shares: 23

Top Referrer: Discord (#rituals channel)
Peak Views: March 20 (347 views)
```

**Optional feature.** Can disable analytics entirely.

---

## Accessibility Features

### **For Viewers:**

- **Captions:** Auto-generated descriptions ("Lunar Bloom ritual, night sky, particles spiraling upward")
- **Audio descriptions:** Voiceover explaining visual elements
- **Pause button:** Yes, you can pause a 6-second video (for frame-by-frame analysis)

### **For Creators:**

- **Remixing:** Can re-capture same ritual multiple times (different angles, slow-mo)
- **Manual peak selection:** Override auto-detection if you didn't like chosen moment
- **Color-blind filters:** See your replay as color-blind players see it

**Inclusive by design.**

---

## Moderation & Community Guidelines

### **Prohibited Content:**

❌ Real-world identifiable locations (privacy)  
❌ Other people without consent  
❌ Offensive gestures  
❌ Spam/advertising  

**Enforcement:**

1. Auto-detection (AI scans uploads)
2. Community reports
3. Human review

**Penalty:** Replay removed, repeated violations = sharing disabled.

---

## Monetization (Or Lack Thereof)

**We will NOT:**

- Sell "premium" replay slots
- Charge for export
- Watermark videos unless you want us to
- Insert ads into replays

**Replays are YOURS.**

You created the moment. We just helped capture it.

---

## Future Features (Year 2)

### **Replay Editor (Maybe)**

**Requested features:**
- Slow-motion toggle
- Camera angle adjustment
- Music overlay
- Text captions

**Status:** Considering. Don't want to turn into iMovie.

### **Collaborative Replays**

**Idea:** Stitch together replays from multiple players performing synchronized rituals.

**Status:** Prototype phase.

---

## The Philosophy

Most games: Streaming requires OBS, overlays, editing.

**Ascendant Continuum:** Your best moment is automatically captured.

**No setup. No editing. Just share (or don't).**

**Creativity through automatic curation.**

---

## Unexpected Emotional Impact

**Alpha tester feedback:**

*"I watched my first successful ritual replay 20 times. I cried. I didn't think I could do it, and there it was—proof that I DID."*

*"My grandfather passed away. We used to stargaze together. I performed a ritual at his favorite spot. The replay is all I have left of that moment."*

*"I deleted the game. But I kept the replays. They're memories now."*

**Replays became more than gameplay.**

They became **emotional artifacts**.

---

## Storage & Lifetime

### **How Long Do Replays Last?**

- **Local storage:** Forever (until you delete app)
- **Cloud backup:** 1 year (renewable if you re-share)
- **Public gallery:** Permanent (as long as game exists)

**If we shut down servers** (hopefully never):
- You'll get 90-day warning
- Download ALL your replays
- Export as MP4 files

**Your moments won't disappear when we do.**

---

## The Anti-Thesis to "Pics or It Didn't Happen"

**Traditional social media:** Must capture EVERYTHING or it's wasted.

**Ascendant Continuum:** We captured the best part. You were present for the rest.

**You don't need to hold your phone up.**

**Just perform the ritual.**

**We'll remember the moment for you.**

---

## Final Thought

6 seconds.

That's all you get.

**Make it count.**

---

**Related Posts:**
- [Real Stargazing: Anti-Screen-Time Mechanic](real-stargazing-anti-screen-time-mechanic.md)
- [Time Capsules: Messages to Future Self](time-capsules-messages-to-future-self.md)
- [Community Mysteries: Months to Solve](community-mysteries-months-to-solve.md)
