# Deploy to Render

This guide will help you deploy Farm Central to Render for free hosting.

## 🚀 Quick Deployment

### 1. Prepare Your Code
- Make sure your code is pushed to GitHub
- The `render.yaml` file is already configured

### 2. Create Render Account
1. Go to [render.com](https://render.com)
2. Click "Get Started for Free"
3. Sign up with your GitHub account
4. No credit card required for free tier

### 3. Deploy Your App
1. In Render dashboard, click "New" → "Web Service"
2. Connect your GitHub repository
3. Render will auto-detect it's a .NET project
4. Click "Create Web Service"

### 4. Configure Environment Variables
In your Render dashboard, go to "Environment" tab and add:

```
ASPNETCORE_ENVIRONMENT=Production
Firebase__ProjectId=your-firebase-project-id
Firebase__DatabaseUrl=https://your-project-default-rtdb.firebaseio.com
```

### 5. Upload Firebase Credentials
1. In Render dashboard, go to "Files" tab
2. Upload your Firebase JSON credentials file
3. Name it `firebase-credentials.json`

### 6. Update appsettings.json
Make sure your `appsettings.json` has the correct Firebase path:

```json
{
  "Firebase": {
    "ProjectId": "your-project-id",
    "CredentialsPath": "firebase-credentials.json",
    "DatabaseUrl": "https://your-project-default-rtdb.firebaseio.com"
  }
}
```

## ✅ Done!

Your app will be available at: `https://your-app-name.onrender.com`

## 🚨 Troubleshooting

- **Build fails**: Check that all NuGet packages are in the .csproj file
- **Firebase errors**: Verify environment variables are set correctly
- **App won't start**: Check the logs in Render dashboard

## 🔗 Share Your Portfolio

Once deployed, you can share your live demo URL in your portfolio!

---

**That's it!** Your Farm Central app will be live and accessible to anyone. 🎯