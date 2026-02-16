const functions = require('firebase-functions');
const admin = require('firebase-admin');
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
    const challengeType = challengeTypes[seed % challengeTypes.Length];
    
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

function containsInappropriateContent(text) {
  if (!text) return false;
  
  // Basic profanity filter (would use a proper service in production)
  const inappropriate = ['badword1', 'badword2']; // Placeholder
  const lowerText = text.toLowerCase();
  
  return inappropriate.some(word => lowerText.includes(word));
}
