# Render Production Setup

## 🔧 Fix Firebase Credentials in Render

The app is failing because Firebase credentials aren't configured in production. Here's how to fix it:

### 1. Get Your Firebase Credentials JSON
1. Go to [Firebase Console](https://console.firebase.google.com/)
2. Click gear icon → "Project settings"
3. Go to "Service accounts" tab
4. Click "Generate new private key"
5. Download the JSON file

### 2. Convert JSON to Environment Variable
1. Open the downloaded JSON file
2. Copy ALL the content (including the curly braces)
3. In Render dashboard, go to "Environment" tab
4. Add new environment variable:

**Key:** `FIREBASE_CREDENTIALS_JSON`
**Value:** Paste the entire JSON content here

### 3. Add Other Environment Variables
Also add these in Render:

```
ASPNETCORE_ENVIRONMENT=Production
Firebase__ProjectId=your-project-id
Firebase__DatabaseUrl=https://your-project-default-rtdb.firebaseio.com
```

### 4. Redeploy
1. Push your code changes to GitHub
2. Render will auto-deploy
3. Check the logs to see if Firebase initializes successfully

## ✅ Done!

Your app should now work in production with proper Firebase authentication.

---

**That's it!** The app will now use environment variable credentials instead of file-based ones. 🎯 