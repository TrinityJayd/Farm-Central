using Firebase.Auth;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using PROG7311_P2.Models;
using Newtonsoft.Json;

namespace PROG7311_P2.Services
{
    public class FirebaseAuthService : IFirebaseAuthService
    {
        private readonly FirebaseAuthProvider _authProvider;
        private readonly ILogger<FirebaseAuthService> _logger;
        private readonly IConfiguration _configuration;

        public FirebaseAuthService(IConfiguration configuration, ILogger<FirebaseAuthService> logger)
        {
            _configuration = configuration;
            _logger = logger;
            
            var apiKey = _configuration["Firebase:ApiKey"];
            if (string.IsNullOrEmpty(apiKey))
            {
                throw new InvalidOperationException("Firebase API key is not configured");
            }
            
            _authProvider = new FirebaseAuthProvider(new FirebaseConfig(apiKey));
        }

        public async Task<string?> GetUserTokenAsync(string email, string password)
        {
            try
            {
                var authLink = await _authProvider.SignInWithEmailAndPasswordAsync(email, password);
                var token = authLink.FirebaseToken;
                
                _logger.LogInformation("User authenticated successfully: {Email}", email);
                return token;
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError(ex, "Firebase authentication failed for user: {Email}", email);
                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during authentication for user: {Email}", email);
                return null;
            }
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            try
            {
                var user = await _authProvider.GetUserAsync(token);
                return user != null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Token validation failed");
                return false;
            }
        }

        public async Task<string?> GetUserIdFromTokenAsync(string token)
        {
            try
            {
                var user = await _authProvider.GetUserAsync(token);
                return user?.LocalId;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get user ID from token");
                return null;
            }
        }

        public async Task<bool> CreateUserAsync(string email, string password, string displayName)
        {
            try
            {
                var authLink = await _authProvider.CreateUserWithEmailAndPasswordAsync(email, password);
                
                // Update display name
                if (!string.IsNullOrEmpty(displayName))
                {
                    await UpdateUserDisplayNameAsync(authLink.User.LocalId, displayName);
                }
                
                _logger.LogInformation("User created successfully: {Email}", email);
                return true;
            }
            catch (FirebaseAuthException ex)
            {
                _logger.LogError(ex, "Failed to create user: {Email}", email);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error creating user: {Email}", email);
                return false;
            }
        }

        public async Task<bool> UpdateUserDisplayNameAsync(string uid, string displayName)
        {
            try
            {
                // This would require additional Firebase Admin SDK calls
                // For now, we'll just log it
                _logger.LogInformation("Display name update requested for user {Uid}: {DisplayName}", uid, displayName);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to update display name for user: {Uid}", uid);
                return false;
            }
        }
    }
} 