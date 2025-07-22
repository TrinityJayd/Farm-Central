# Firebase Setup Guide

This guide will help you set up Firebase for the Farm Central project.

## 🚀 Quick Setup

### 1. Create Firebase Project
1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click "Create a project"
3. Name it "Farm Central" (or your preferred name)
4. Follow the setup wizard (you can disable Google Analytics)

### 2. Enable Services
1. **Authentication**: Click "Authentication" → "Get started" → Enable "Email/Password"
2. **Realtime Database**: Click "Realtime Database" → "Create Database" → Start in test mode

### 3. Get Configuration
1. Click the gear icon → "Project settings"
2. Scroll down to "Service accounts"
3. Click "Generate new private key" → Download the JSON file
4. Place the JSON file in your project root as `firebase-credentials.json`

### 4. Update Configuration
Open `appsettings.json` and update:

```json
{
  "Firebase": {
    "ProjectId": "your-project-id",
    "CredentialsPath": "firebase-credentials.json",
    "DatabaseUrl": "https://your-project-default-rtdb.firebaseio.com"
  }
}
```

### 5. Test Setup
1. Run the application: `dotnet run`
2. Try to register an employee
3. Check Firebase Console to see if data appears

## ✅ Done!

Your Firebase setup is complete. The application will automatically create default product types on first run.

## 🚨 Troubleshooting

- **"Credentials not found"**: Make sure the JSON file is in the project root
- **"Permission denied"**: Check that security rules are published
- **"Database URL not found"**: Verify the URL in appsettings.json

---

**That's it!** Simple, clean, and ready to go. 🎯 