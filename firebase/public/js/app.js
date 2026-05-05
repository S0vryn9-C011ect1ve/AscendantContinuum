// Firebase Configuration
import { initializeApp } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-app.js";
import { getFirestore, collection, addDoc } from "https://www.gstatic.com/firebasejs/10.7.1/firebase-firestore.js";

// Firebase config — Analytics disabled: zero tracking by design
const firebaseConfig = {
    apiKey: "AIzaSyAOlS9yuWVG4Owh5mO9iLRNhRw0Q14vfVc",
    authDomain: "ascendant-continuum.firebaseapp.com",
    projectId: "ascendant-continuum",
    storageBucket: "ascendant-continuum.firebasestorage.app",
    messagingSenderId: "892573648674",
    appId: "1:892573648674:web:085e5cf12657fa5676477b"
};

// Initialize Firebase (Firestore only — no Analytics, no tracking)
const app = initializeApp(firebaseConfig);
const db = getFirestore(app);

console.log("🔥 Firebase initialized!");

// Particle Animation for Hero
function createParticles() {
    const particlesContainer = document.getElementById('particles');
    const particleCount = 50;

    for (let i = 0; i < particleCount; i++) {
        const particle = document.createElement('div');
        particle.className = 'particle';
        particle.style.cssText = `
            position: absolute;
            width: ${Math.random() * 4 + 2}px;
            height: ${Math.random() * 4 + 2}px;
            background: rgba(255, 255, 255, ${Math.random() * 0.5 + 0.3});
            border-radius: 50%;
            left: ${Math.random() * 100}%;
            top: ${Math.random() * 100}%;
            animation: float ${Math.random() * 10 + 10}s linear infinite;
            animation-delay: ${Math.random() * 5}s;
        `;
        particlesContainer.appendChild(particle);
    }

    // Add floating animation
    const style = document.createElement('style');
    style.textContent = `
        @keyframes float {
            0% { transform: translateY(0) translateX(0); opacity: 0; }
            10% { opacity: 1; }
            90% { opacity: 1; }
            100% { transform: translateY(-100vh) translateX(${Math.random() * 100 - 50}px); opacity: 0; }
        }
    `;
    document.head.appendChild(style);
}

// Email Signup Handler
document.getElementById('signup-form').addEventListener('submit', async (e) => {
    e.preventDefault();

    const emailInput = e.target.querySelector('input[type="email"]');
    const email = emailInput.value;
    const button = e.target.querySelector('button');

    // Disable button
    button.textContent = 'Joining...';
    button.disabled = true;

    try {
        // Add to Firestore
        await addDoc(collection(db, 'waitlist'), {
            email: email,
            timestamp: new Date(),
            source: 'website'
        });

        // Success
        button.textContent = '✨ Welcome, Seeker!';
        button.style.background = '#4ecdc4';
        emailInput.value = '';

        console.log('📧 Email added to waitlist');

        // Reset after 3 seconds
        setTimeout(() => {
            button.textContent = 'Join the Waitlist';
            button.disabled = false;
            button.style.background = '';
        }, 3000);

    } catch (error) {
        console.error('Error adding email:', error);
        button.textContent = '❌ Error - Try Again';
        button.disabled = false;

        setTimeout(() => {
            button.textContent = 'Join the Waitlist';
        }, 3000);
    }
});

// Smooth scroll for anchor links
document.querySelectorAll('a[href^="#"]').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault();
        const target = document.querySelector(this.getAttribute('href'));
        if (target) {
            target.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    });
});

// Initialize particles when page loads
window.addEventListener('load', () => {
    createParticles();
});

// Intersection Observer for fade-in animations
const observerOptions = {
    threshold: 0.1,
    rootMargin: '0px 0px -50px 0px'
};

const observer = new IntersectionObserver((entries) => {
    entries.forEach(entry => {
        if (entry.isIntersecting) {
            entry.target.style.opacity = '1';
            entry.target.style.transform = 'translateY(0)';
        }
    });
}, observerOptions);

// Observe all cards
document.querySelectorAll('.feature-card, .realm-card, .access-feature').forEach(card => {
    card.style.opacity = '0';
    card.style.transform = 'translateY(20px)';
    card.style.transition = 'all 0.6s ease';
    observer.observe(card);
});

console.log('✨ The Ascendant Continuum website loaded!');
