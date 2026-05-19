using AuthService.DTOs;
using AuthService.Models;

namespace AuthService.Services
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterRequestDTO request);
        string GenerateToken(User user);
        string GenerateRefreshToken();
        Task SaveRefreshTokenAsync(User user, string refreshToken);
    }
}
