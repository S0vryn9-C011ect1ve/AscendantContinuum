# The Ascendant Continuum - Website

Landing page and marketing website for The Ascendant Continuum game.

## 🚀 Quick Start

### Local Development
Just open `index.html` in your browser! No build process needed.

### Deploy to Firebase Hosting

1. **Use the deployment script from the workspace root**:
```powershell
.\Deploy-Firebase.ps1 -Target Hosting
```

Your site will be live at: `https://ascendant-continuum.web.app`

## 📁 Structure

```
firebase/
├── public/
│   ├── index.html          # Main landing page
│   ├── css/
│   │   └── style.css       # Styles (cosmic theme)
│   ├── js/
│   │   └── app.js          # Firebase integration + animations
│   └── README.md           # This file
```

## ✨ Features

- **Responsive Design** - Works on all devices
- **Firebase Integration** - Email waitlist collection
- **Animated Particles** - Floating cosmic particles
- **Smooth Scrolling** - Polished UX
- **Accessibility-First** - WCAG compliant
- **Zero Dependencies** - Pure HTML/CSS/JS

## 🎨 Customization

### Colors
Edit CSS variables in `css/style.css`:
```css
:root {
    --color-primary: #ff6b35;    /* Ember orange */
    --color-secondary: #4ecdc4;  /* Mystic teal */
    /* ... more colors */
}
```

### Content
Edit text directly in `index.html`

### Firebase Config
Already configured in `js/app.js` with your project credentials

## 📊 Analytics

- Firebase Analytics automatically tracks page views
- Email signups saved to Firestore collection `waitlist`

## 🔒 Security

Firebase security rules needed for Firestore:
```javascript
rules_version = '2';
service cloud.firestore {
  match /databases/{database}/documents {
    match /waitlist/{email} {
      allow read: if false;  // Only admins can read
      allow write: if true;  // Anyone can submit email
    }
  }
}
```

## 📞 Contact

ascendantcontinuum@gmail.com

---

**Built with ✨ magic and 🔥 Firebase**
