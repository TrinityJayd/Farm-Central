using PROG7311_P2.Models;

namespace PROG7311_P2.Services
{
    public interface IFirebaseAuthService
    {
        Task<string?> GetUserTokenAsync(string email, string password);
        Task<bool> ValidateTokenAsync(string token);
        Task<string?> GetUserIdFromTokenAsync(string token);
        Task<bool> CreateUserAsync(string email, string password, string displayName);
        Task<bool> UpdateUserDisplayNameAsync(string uid, string displayName);
    }
} 