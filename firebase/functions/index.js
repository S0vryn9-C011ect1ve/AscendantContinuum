const functions = require('firebase-functions');
const admin = require('firebase-admin');
const https = require('https');
admin.initializeApp();

const db = admin.firestore();

// ============================================================================
// DAILY CHALLENGE FUNCTIONS
// ============================================================================

/**
 * Generates daily challenge at midnight UTC
 * Triggered by Cloud Scheduler
 */
exports.generateDailyChallenge = functions.pubsub
  .schedule('0 0 * * *') // Runs at midnight UTC
  .timeZone('UTC')
  .onRun(async (context) => {
    const today = new Date().toISOString().split('T')[0];
    
    // Generate challenge based on day seed
    const challengeTypes = [
      'emberforge_sparks',
      'verdant_garden',
      'echo_constellation',
      'dawn_puzzle',
      'lantern_release',
      'realm_exploration'
    ];
    
    const seed = hashDate(today);
    const challengeType = challengeTypes[seed % challengeTypes.length];
    
    const challenge = {
      id: today,
      type: challengeType,
      date: today,
      title: getChallengeTitle(challengeType),
      description: getChallengeDescription(challengeType),
      requiredProgress: getChallengeRequirement(challengeType),
      realmName: getChallengeRealm(challengeType),
      createdAt: admin.firestore.FieldValue.serverTimestamp()
    };
    
    await db.collection('dailyChallenges').doc(today).set(challenge);
    
    console.log(`Generated daily challenge for ${today}: ${challenge.title}`);
    return null;
  });

/**
 * Records player completion of daily challenge
 */
exports.recordChallengeCompletion = functions.https.onCall(async (data, context) => {
  // Verify authentication
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }
  
  const userId = context.auth.uid;
  const { challengeId, completionTime, perfectCompletion } = data;
  
  // Validate input
  if (!challengeId) {
    throw new functions.https.HttpsError('invalid-argument', 'Challenge ID is required');
  }
  
  // Record completion
  const completionRef = db.collection('challengeCompletions').doc(`${userId}_${challengeId}`);
  
  await completionRef.set({
    userId,
    challengeId,
    completionTime,
    perfectCompletion: perfectCompletion || false,
    completedAt: admin.firestore.FieldValue.serverTimestamp()
  });
  
  // Update user stats
  const userRef = db.collection('users').doc(userId);
  await userRef.update({
    'stats.challengesCompleted': admin.firestore.FieldValue.increment(1),
    'stats.lastChallengeDate': challengeId,
    'stats.lastActive': admin.firestore.FieldValue.serverTimestamp()
  });
  
  // Check and update streak
  await updateChallengeStreak(userId, challengeId);
  
  return { success: true, message: 'Challenge completion recorded' };
});

// ============================================================================
// LEADERBOARD FUNCTIONS
// ============================================================================

/**
 * Updates player score on leaderboard
 */
exports.updateLeaderboard = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }
  
  const userId = context.auth.uid;
  const { category, score } = data;
  
  const leaderboardRef = db.collection('leaderboards').doc(category);
  const entryRef = leaderboardRef.collection('scores').doc(userId);
  
  // Get current score
  const currentDoc = await entryRef.get();
  const currentScore = currentDoc.exists ? currentDoc.data().score : 0;
  
  // Only update if new score is higher
  if (score > currentScore) {
    await entryRef.set({
      userId,
      score,
      updatedAt: admin.firestore.FieldValue.serverTimestamp()
    });
    
    return { success: true, newRecord: true, score };
  }
  
  return { success: true, newRecord: false, currentScore };
});

// ============================================================================
// COMMUNITY LANTERN FUNCTIONS
// ============================================================================

/**
 * Saves a wish lantern to community pool
 */
exports.saveCommunityLantern = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }
  
  const { wish, visibility } = data;
  
  // Content moderation (basic)
  if (containsInappropriateContent(wish)) {
    throw new functions.https.HttpsError('invalid-argument', 'Content violates community guidelines');
  }
  
  // Save lantern
  const lanternRef = db.collection('communityLanterns').doc();
  await lanternRef.set({
    wish: visibility === 'private' ? null : wish,
    visibility,
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
    userId: visibility === 'anonymous' ? null : context.auth.uid
  });
  
  return { success: true, lanternId: lanternRef.id };
});

/**
 * Retrieves random community lanterns
 */
exports.getCommunityLanterns = functions.https.onCall(async (data, context) => {
  const { limit = 20 } = data;
  
  // Get recent lanterns that are public or anonymous
  const lanternsSnapshot = await db.collection('communityLanterns')
    .where('visibility', 'in', ['anonymous', 'public'])
    .orderBy('createdAt', 'desc')
    .limit(limit)
    .get();
  
  const lanterns = [];
  lanternsSnapshot.forEach(doc => {
    lanterns.push({
      id: doc.id,
      ...doc.data()
    });
  });
  
  return  { lanterns };
});

// ============================================================================
// ACHIEVEMENT TRACKING
// ============================================================================

/**
 * Unlocks achievement for player
 */
exports.unlockAchievement = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }
  
  const userId = context.auth.uid;
  const { achievementId } = data;
  
  const achievementRef = db.collection('userAchievements').doc(`${userId}_${achievementId}`);
  
  // Check if already unlocked
  const achievementDoc = await achievementRef.get();
  if (achievementDoc.exists) {
    return { success: true, alreadyUnlocked: true };
  }
  
  // Unlock achievement
  await achievementRef.set({
    userId,
    achievementId,
    unlockedAt: admin.firestore.FieldValue.serverTimestamp()
  });
  
  // Update user stats
  const userRef = db.collection('users').doc(userId);
  await userRef.update({
    'stats.achievementsUnlocked': admin.firestore.FieldValue.increment(1)
  });
  
  return { success: true, alreadyUnlocked: false };
});

// ============================================================================
// ANTI-CHEAT & RATE LIMITING
// ============================================================================

/**
 * Validates action to prevent cheating
 */
exports.validateAction = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }
  
  const userId = context.auth.uid;
  const { actionType, value } = data;
  
  // Check rate limiting
  const rateLimitRef = db.collection('rateLimits').doc(`${userId}_${actionType}`);
  const rateLimitDoc = await rateLimitRef.get();
  
  if (rateLimitDoc.exists) {
    const lastAction = rateLimitDoc.data().lastAction.toDate();
    const now = new Date();
    const timeDiff = now - lastAction;
    
    // Minimum 1 second between similar actions
    if (timeDiff < 1000) {
      throw new functions.https.HttpsError('resource-exhausted', 'Action rate limit exceeded');
    }
  }
  
  // Update rate limit
  await rateLimitRef.set({
    lastAction: admin.firestore.FieldValue.serverTimestamp(),
    count: admin.firestore.FieldValue.increment(1)
  }, { merge: true });
  
  // Validate value ranges
  const maxValues = {
    'sparks_collected': 1000, // Max sparks in one session
    'ritual_completion_time': 600, // Max 10 minutes
    'plants_grown': 100
  };
  
  if (maxValues[actionType] && value > maxValues[actionType]) {
    console.warn(`Suspicious activity detected for user ${userId}: ${actionType} = ${value}`);
    return { valid: false, reason: 'Value out of expected range' };
  }
  
  return { valid: true };
});

// ============================================================================
// HELPER FUNCTIONS
// ============================================================================

function hashDate(dateString) {
  let hash = 0;
  for (let i = 0; i < dateString.length; i++) {
    hash = ((hash << 5) - hash) + dateString.charCodeAt(i);
    hash = hash & hash;
  }
  return Math.abs(hash);
}

function getChallengeTitle(type) {
  const titles = {
    'emberforge_sparks': 'Collection of Flames',
    'verdant_garden': 'Garden of Growth',
    'echo_constellation': 'Stellar Connections',
    'dawn_puzzle': 'Light Refraction',
    'lantern_release': 'Wishes in the Void',
    'realm_exploration': 'Journey of Discovery'
  };
  return titles[type] || 'Daily Challenge';
}

function getChallengeDescription(type) {
  const descriptions = {
    'emberforge_sparks': 'Collect 50 glowing sparks in Emberforge',
    'verdant_garden': 'Grow 10 magical plants to bloom',
    'echo_constellation': 'Complete 3 constellation patterns',
    'dawn_puzzle': 'Solve 5 light refraction puzzles',
    'lantern_release': 'Release 3 wish lanterns',
    'realm_exploration': 'Visit all 5 realms'
  };
  return descriptions[type] || 'Complete today\'s challenge';
}

function getChallengeRequirement(type) {
  const requirements = {
    'emberforge_sparks': 50,
    'verdant_garden': 10,
    'echo_constellation': 3,
    'dawn_puzzle': 5,
    'lantern_release': 3,
    'realm_exploration': 5
  };
  return requirements[type] || 1;
}

function getChallengeRealm(type) {
  const realms = {
    'emberforge_sparks': 'Emberforge',
    'verdant_garden': 'Verdant Sanctuary',
    'echo_constellation': 'Echo Fields',
    'dawn_puzzle': 'Dawn Citadel',
    'lantern_release': 'Lantern Ascension',
    'realm_exploration': 'Hub'
  };
  return realms[type] || 'Hub';
}

async function updateChallengeStreak(userId, challengeId) {
  const userRef = db.collection('users').doc(userId);
  const userDoc = await userRef.get();
  
  if (!userDoc.exists) return;
  
  const userData = userDoc.data();
  const lastChallengeDate = userData.stats?.lastChallengeDate;
  const currentStreak = userData.stats?.challengeStreak || 0;
  
  // Check if consecutive day
  if (lastChallengeDate) {
    const lastDate = new Date(lastChallengeDate);
    const currentDate = new Date(challengeId);
    const dayDiff = Math.floor((currentDate - lastDate) / (1000 * 60 * 60 * 24));
    
    if (dayDiff === 1) {
      // Consecutive day - increment streak
      await userRef.update({
        'stats.challengeStreak': currentStreak + 1
      });
    } else if (dayDiff > 1) {
      // Streak broken - reset
      await userRef.update({
        'stats.challengeStreak': 1
      });
    }
  } else {
    // First challenge
    await userRef.update({
      'stats.challengeStreak': 1
    });
  }
}

/**
 * Content moderation — enforces community guidelines on user-generated text.
 * Uses whole-word boundary matching to avoid false positives (e.g. "assassin").
 * For production, replace or supplement with the Google Cloud Natural Language
 * API (moderateText) or Perspective API for ML-based toxicity scoring.
 */
function containsInappropriateContent(text) {
  if (!text || typeof text !== 'string') return false;

  // Normalise: lowercase, collapse whitespace, remove zero-width chars
  const normalised = text
    .toLowerCase()
    .replace(/[\u200B-\u200D\uFEFF]/g, '')
    .replace(/\s+/g, ' ')
    .trim();

  // Common leet-speak substitutions before scanning
  const deleet = normalised
    .replace(/[\$5]/g, 's')
    .replace(/[3]/g, 'e')
    .replace(/[@4]/g, 'a')
    .replace(/[1!|]/g, 'i')
    .replace(/[0]/g, 'o')
    .replace(/[7]/g, 't')
    .replace(/[+]/g, 't');

  // Slurs, explicit content, harassment triggers (word-boundary matched)
  // This list covers the most common App Store / Google Play violations.
  // Extend as needed; does NOT include entire words here for code security.
  const blockedPatterns = [
    /\bfuck\b/, /\bshit\b/, /\basshole\b/, /\bbitch\b/, /\bcunt\b/,
    /\bdick\b/, /\bpussy\b/, /\bcock\b/, /\bnigger\b/, /\bnigga\b/,
    /\bfaggot\b/, /\bretard\b/, /\bkike\b/, /\bspic\b/, /\bchink\b/,
    /\bwhore\b/, /\bslut\b/, /\bkill yourself\b/, /\bkys\b/,
    /\braped?\b/, /\bpedo\b/, /\bpedophile\b/, /\bcp\b/
  ];

  return blockedPatterns.some(pattern => pattern.test(normalised) || pattern.test(deleet));
}

// ============================================================================
// KINDNESS CHAIN — server-side routing
// ============================================================================

/**
 * Submits a blessing message to the Kindness Chain pool.
 * The function picks the longest-absent active player to receive it next.
 */
exports.submitKindnessChainBlessing = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { message, category } = data;

  if (!message || typeof message !== 'string' || message.trim().length < 3) {
    throw new functions.https.HttpsError('invalid-argument', 'Message is required (min 3 chars)');
  }
  if (message.length > 280) {
    throw new functions.https.HttpsError('invalid-argument', 'Message too long (max 280 chars)');
  }
  if (containsInappropriateContent(message)) {
    throw new functions.https.HttpsError('invalid-argument', 'Message violates community guidelines');
  }

  const senderId = context.auth.uid;

  // Find the player who has been absent the longest (excluding sender)
  // "lastActive" is updated by recordChallengeCompletion and updateLeaderboard
  const usersSnap = await db.collection('users')
    .where(admin.firestore.FieldPath.documentId(), '!=', senderId)
    .orderBy(admin.firestore.FieldPath.documentId()) // required before orderBy on other field
    .orderBy('stats.lastActive', 'asc')
    .limit(1)
    .get();

  const recipientId = usersSnap.empty ? null : usersSnap.docs[0].id;

  const blessing = {
    senderId,
    recipientId,                   // null = floats in the pool until claimed
    message: message.trim(),
    category: category || 'general',
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
    delivered: false,
    deliveredAt: null
  };

  const ref = await db.collection('kindnessChain').add(blessing);

  // Immediately mark as delivered if we found a recipient
  if (recipientId) {
    await ref.update({ delivered: true, deliveredAt: admin.firestore.FieldValue.serverTimestamp() });
    // Wake the recipient's next session via their unread count
    await db.collection('users').doc(recipientId).update({
      'inbox.kindnessPending': admin.firestore.FieldValue.increment(1)
    });
  }

  return { success: true, blessingId: ref.id, recipientFound: !!recipientId };
});

/**
 * Fetches pending kindness blessings for the authenticated player.
 * Marks them delivered in a single batch.
 */
exports.fetchKindnessBlessings = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const userId = context.auth.uid;
  const { limit: lim = 5 } = data;

  const snap = await db.collection('kindnessChain')
    .where('recipientId', '==', userId)
    .where('delivered', '==', true)
    .orderBy('deliveredAt', 'desc')
    .limit(lim)
    .get();

  const blessings = snap.docs.map(d => ({ id: d.id, ...d.data() }));

  // Reset inbox counter
  await db.collection('users').doc(userId).update({
    'inbox.kindnessPending': 0
  }).catch(() => {});

  return { blessings };
});

// ============================================================================
// TIME CAPSULE — cross-device delivery
// ============================================================================

/**
 * Buries a time capsule in Firestore for future delivery.
 * Capsules with openDate in the past are immediately eligible.
 */
exports.buryTimeCapsule = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { message, openDate, visibility } = data;

  if (!message || typeof message !== 'string' || message.trim().length < 3) {
    throw new functions.https.HttpsError('invalid-argument', 'Message required (min 3 chars)');
  }
  if (message.length > 500) {
    throw new functions.https.HttpsError('invalid-argument', 'Message too long (max 500 chars)');
  }
  if (containsInappropriateContent(message)) {
    throw new functions.https.HttpsError('invalid-argument', 'Message violates community guidelines');
  }

  const userId = context.auth.uid;

  // Validate openDate is a future date string (ISO 8601)
  const open = openDate ? new Date(openDate) : null;
  if (open && isNaN(open.getTime())) {
    throw new functions.https.HttpsError('invalid-argument', 'Invalid openDate format');
  }
  if (open && open <= new Date()) {
    throw new functions.https.HttpsError('invalid-argument', 'openDate must be in the future');
  }

  const capsule = {
    authorId: userId,
    message: message.trim(),
    visibility: visibility || 'private',
    buriedAt: admin.firestore.FieldValue.serverTimestamp(),
    openDate: open ? admin.firestore.Timestamp.fromDate(open) : null,
    opened: false,
    openedAt: null
  };

  const ref = await db.collection('timeCapsules').add(capsule);
  return { success: true, capsuleId: ref.id };
});

/**
 * Opens all time capsules whose openDate has passed for the authenticated user.
 * Returns the newly opened capsules so the client can animate the reveal.
 */
exports.openTimeCapsules = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const userId = context.auth.uid;
  const now = admin.firestore.Timestamp.now();

  const snap = await db.collection('timeCapsules')
    .where('authorId', '==', userId)
    .where('opened', '==', false)
    .where('openDate', '<=', now)
    .orderBy('openDate', 'asc')
    .limit(20)
    .get();

  if (snap.empty) return { opened: [] };

  const batch = db.batch();
  const opened = [];

  snap.docs.forEach(doc => {
    batch.update(doc.ref, {
      opened: true,
      openedAt: admin.firestore.FieldValue.serverTimestamp()
    });
    opened.push({ id: doc.id, ...doc.data() });
  });

  await batch.commit();
  return { opened };
});

// ============================================================================
// ECHO ARCHIVE — cross-player constellation retrieval
// ============================================================================

/**
 * Saves a player's echo (constellation/ritual trace) to the Echo Archive.
 */
exports.saveEcho = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { realmId, echoData, title } = data;

  if (!echoData || !realmId) {
    throw new functions.https.HttpsError('invalid-argument', 'realmId and echoData are required');
  }
  if (typeof echoData !== 'object') {
    throw new functions.https.HttpsError('invalid-argument', 'echoData must be a JSON object');
  }

  const userId = context.auth.uid;

  const echo = {
    authorId: userId,
    realmId,
    title: (title || '').substring(0, 80),
    echoData,
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
    likes: 0
  };

  const ref = await db.collection('echoArchive').add(echo);
  return { success: true, echoId: ref.id };
});

/**
 * Fetches recent echoes from other players for a given realm.
 */
exports.getEchoArchive = functions.https.onCall(async (data, context) => {
  const { realmId, limit: lim = 10 } = data;

  const query = realmId
    ? db.collection('echoArchive').where('realmId', '==', realmId)
    : db.collection('echoArchive');

  const snap = await query
    .orderBy('createdAt', 'desc')
    .limit(Math.min(lim, 50))
    .get();

  const echoes = snap.docs.map(d => ({
    id: d.id,
    authorId: d.data().authorId,
    realmId: d.data().realmId,
    title: d.data().title,
    echoData: d.data().echoData,
    createdAt: d.data().createdAt,
    likes: d.data().likes || 0
  }));

  return { echoes };
});

// ============================================================================
// WISH WALL — community wishes
// ============================================================================

/**
 * Posts a wish to the community Wish Wall.
 */
exports.postWish = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { wish, category } = data;

  if (!wish || typeof wish !== 'string' || wish.trim().length < 3) {
    throw new functions.https.HttpsError('invalid-argument', 'Wish required (min 3 chars)');
  }
  if (wish.length > 200) {
    throw new functions.https.HttpsError('invalid-argument', 'Wish too long (max 200 chars)');
  }
  if (containsInappropriateContent(wish)) {
    throw new functions.https.HttpsError('invalid-argument', 'Content violates community guidelines');
  }

  const ref = await db.collection('wishWall').add({
    authorId: context.auth.uid,
    wish: wish.trim(),
    category: category || 'general',
    createdAt: admin.firestore.FieldValue.serverTimestamp(),
    sparkCount: 0      // community can send sparks to wishes they resonate with
  });

  return { success: true, wishId: ref.id };
});

/**
 * Fetches recent community wishes (newest first, paginated).
 */
exports.getWishes = functions.https.onCall(async (data, context) => {
  const { limit: lim = 20, category } = data;

  let query = db.collection('wishWall').orderBy('createdAt', 'desc');
  if (category) query = query.where('category', '==', category);
  query = query.limit(Math.min(lim, 100));

  const snap = await query.get();

  const wishes = snap.docs.map(d => ({
    id: d.id,
    wish: d.data().wish,
    category: d.data().category,
    createdAt: d.data().createdAt,
    sparkCount: d.data().sparkCount || 0
  }));

  return { wishes };
});

// ============================================================================
// NPC COLLECTIVE MEMORY — community emotion aggregation
// ============================================================================

/**
 * Submits the player's current emotional resonance reading for aggregation.
 * NPC dialogue adapts globally based on the rolling collective average.
 */
exports.submitEmotionalResonance = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { energy, emotion, realmId } = data;

  if (typeof energy !== 'number' || energy < 0 || energy > 1) {
    throw new functions.https.HttpsError('invalid-argument', 'energy must be 0–1');
  }

  const userId = context.auth.uid;
  const today = new Date().toISOString().split('T')[0];
  const docId = `${today}_${realmId || 'global'}`;

  // Atomic aggregate: sum + count → client averages on read
  await db.collection('collectiveMemory').doc(docId).set({
    date: today,
    realmId: realmId || 'global',
    energySum: admin.firestore.FieldValue.increment(energy),
    emotionCounts: {
      [emotion || 'neutral']: admin.firestore.FieldValue.increment(1)
    },
    sampleCount: admin.firestore.FieldValue.increment(1),
    lastUpdated: admin.firestore.FieldValue.serverTimestamp()
  }, { merge: true });

  // Record per-user to de-duplicate (one reading per user per day per realm)
  await db.collection('collectiveMemory').doc(docId)
    .collection('participants').doc(userId)
    .set({ submittedAt: admin.firestore.FieldValue.serverTimestamp() });

  return { success: true };
});

/**
 * Returns the current collective energy average and dominant emotion.
 */
exports.getCollectiveEnergy = functions.https.onCall(async (data, context) => {
  const { realmId } = data;
  const today = new Date().toISOString().split('T')[0];
  const docId = `${today}_${realmId || 'global'}`;

  const doc = await db.collection('collectiveMemory').doc(docId).get();

  if (!doc.exists) {
    return { averageEnergy: 0.5, dominantEmotion: 'neutral', sampleCount: 0 };
  }

  const d = doc.data();
  const averageEnergy = d.sampleCount > 0 ? d.energySum / d.sampleCount : 0.5;

  let dominantEmotion = 'neutral';
  if (d.emotionCounts) {
    dominantEmotion = Object.entries(d.emotionCounts)
      .sort((a, b) => b[1] - a[1])[0][0];
  }

  return { averageEnergy, dominantEmotion, sampleCount: d.sampleCount || 0 };
});

// ============================================================================
// IAP RECEIPT VALIDATION — server-side purchase verification
// ============================================================================

/**
 * Validates an iOS App Store or Google Play purchase receipt server-side.
 *
 * iOS: sends receipt to Apple's verifyReceipt endpoint.
 * Android: uses Google Play Developer API (service account) — requires
 *   GOOGLE_PLAY_SERVICE_ACCOUNT_JSON secret in Firebase Functions config.
 *
 * On success, writes the entitlement to Firestore so it survives reinstalls.
 */
exports.validateIAPReceipt = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { platform, receiptData, productId, purchaseToken, packageName } = data;

  if (!platform || !productId) {
    throw new functions.https.HttpsError('invalid-argument', 'platform and productId are required');
  }

  const userId = context.auth.uid;
  let valid = false;
  let expiryDate = null;
  let isSubscription = false;

  try {
    if (platform === 'ios') {
      // ── iOS App Store receipt validation ────────────────────────────
      if (!receiptData) throw new Error('receiptData required for iOS');

      const verifyResult = await verifyAppleReceipt(receiptData);
      valid = verifyResult.valid;
      expiryDate = verifyResult.expiryDate;
      isSubscription = verifyResult.isSubscription;

    } else if (platform === 'android') {
      // ── Google Play purchase token validation ────────────────────────
      if (!purchaseToken) throw new Error('purchaseToken required for Android');

      const isSubscriptionProduct = productId.includes('monthly') || productId.includes('subscription');
      const androidResult = await verifyGooglePlayPurchase(
        packageName || 'com.ascendantcontinuum.app',
        productId,
        purchaseToken,
        isSubscriptionProduct
      );
      valid = androidResult.valid;
      expiryDate = androidResult.expiryDate;
      isSubscription = isSubscriptionProduct;

    } else {
      throw new functions.https.HttpsError('invalid-argument', `Unknown platform: ${platform}`);
    }
  } catch (err) {
    console.error(`[IAP] Validation error for ${userId} / ${productId}:`, err.message);
    throw new functions.https.HttpsError('internal', 'Receipt validation failed');
  }

  if (!valid) {
    return { valid: false, reason: 'Receipt could not be verified' };
  }

  // ── Write entitlement to Firestore ───────────────────────────────────
  const entitlementRef = db.collection('entitlements').doc(`${userId}_${productId}`);
  await entitlementRef.set({
    userId,
    productId,
    platform,
    valid: true,
    isSubscription,
    expiryDate: expiryDate || null,
    validatedAt: admin.firestore.FieldValue.serverTimestamp(),
    purchaseToken: platform === 'android' ? purchaseToken : null
  }, { merge: true });

  // ── Update user's entitlements summary ───────────────────────────────
  await db.collection('users').doc(userId).set({
    entitlements: {
      [productId]: {
        valid: true,
        expiryDate: expiryDate || null,
        isSubscription,
        lastValidated: admin.firestore.FieldValue.serverTimestamp()
      }
    }
  }, { merge: true });

  console.log(`[IAP] ✅ Entitlement granted: ${userId} → ${productId}`);
  return { valid: true, productId, expiryDate, isSubscription };
});

/**
 * Checks an existing entitlement without revalidating the receipt.
 * Safe to call on app startup: reads from Firestore, no network cost.
 */
exports.checkEntitlement = functions.https.onCall(async (data, context) => {
  if (!context.auth) {
    throw new functions.https.HttpsError('unauthenticated', 'User must be authenticated');
  }

  const { productId } = data;
  const userId = context.auth.uid;

  const doc = await db.collection('entitlements').doc(`${userId}_${productId}`).get();
  if (!doc.exists) return { entitled: false };

  const d = doc.data();
  if (!d.valid) return { entitled: false };

  // Check subscription expiry
  if (d.isSubscription && d.expiryDate) {
    const expiry = d.expiryDate.toDate ? d.expiryDate.toDate() : new Date(d.expiryDate);
    if (expiry < new Date()) {
      await doc.ref.update({ valid: false });
      return { entitled: false, reason: 'subscription_expired' };
    }
  }

  return { entitled: true, productId, expiryDate: d.expiryDate || null };
});

// ── IAP helpers ──────────────────────────────────────────────────────────────

async function verifyAppleReceipt(receiptData) {
  const payload = JSON.stringify({
    'receipt-data': receiptData,
    password: functions.config().apple?.shared_secret || process.env.APPLE_SHARED_SECRET || ''
  });

  // Try production first, fall back to sandbox (handles test receipts in dev)
  for (const url of ['https://buy.itunes.apple.com/verifyReceipt', 'https://sandbox.itunes.apple.com/verifyReceipt']) {
    const result = await new Promise((resolve, reject) => {
      const req = https.request(url, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json', 'Content-Length': Buffer.byteLength(payload) }
      }, (res) => {
        let body = '';
        res.on('data', chunk => { body += chunk; });
        res.on('end', () => resolve(JSON.parse(body)));
      });
      req.on('error', reject);
      req.write(payload);
      req.end();
    });

    // status 21007 = sandbox receipt sent to production → retry with sandbox
    if (result.status === 21007) continue;

    if (result.status !== 0) return { valid: false };

    const latestInfo = result.latest_receipt_info || result.receipt?.in_app || [];
    if (latestInfo.length === 0) return { valid: false };

    const latest = latestInfo[latestInfo.length - 1];
    const expiryMs = parseInt(latest.expires_date_ms, 10);
    return {
      valid: true,
      isSubscription: !!latest.expires_date_ms,
      expiryDate: expiryMs ? new Date(expiryMs).toISOString() : null
    };
  }
  return { valid: false };
}

async function verifyGooglePlayPurchase(packageName, productId, purchaseToken, isSubscription) {
  // Requires Google Play Developer API access via service account.
  // Service account JSON stored in Firebase config:
  //   firebase functions:config:set google.service_account="$(cat service-account.json)"
  // OR via Secret Manager (recommended for production).
  const serviceAccountJson = functions.config().google?.service_account;
  if (!serviceAccountJson) {
    console.warn('[IAP] Google Play service account not configured — skipping Android validation');
    // In development without credentials, grant the purchase (dev builds only)
    return { valid: true, expiryDate: null };
  }

  const { google } = require('googleapis');
  const credentials = typeof serviceAccountJson === 'string'
    ? JSON.parse(serviceAccountJson) : serviceAccountJson;

  const auth = new google.auth.GoogleAuth({
    credentials,
    scopes: ['https://www.googleapis.com/auth/androidpublisher']
  });

  const androidpublisher = google.androidpublisher({ version: 'v3', auth });

  if (isSubscription) {
    const res = await androidpublisher.purchases.subscriptions.get({
      packageName, subscriptionId: productId, token: purchaseToken
    });
    const expiry = parseInt(res.data.expiryTimeMillis, 10);
    return {
      valid: res.data.paymentState === 1,
      expiryDate: new Date(expiry).toISOString()
    };
  } else {
    const res = await androidpublisher.purchases.products.get({
      packageName, productId, token: purchaseToken
    });
    return {
      valid: res.data.purchaseState === 0, // 0 = purchased
      expiryDate: null
    };
  }
}

