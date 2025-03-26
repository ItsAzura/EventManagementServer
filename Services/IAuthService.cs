using EventManagementServer.Dto;
using EventManagementServer.Models;

namespace EventManagementServer.Services
{
    public interface IAuthService
    {
        Task<User?> RegisterAsync(UserDto request);
        Task<TokenResponseDto?> LoginAsync(LoginUserDto request);
        Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request);
        Task<bool> LogoutAsync(int userId, string refreshToken);
        Task<TokenResponseDto?> GoogleLoginAsync(GoogleLoginRequest request);
    }
}
