using EventManagementServer.Dto;
using EventManagementServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController(IAuthService authService) : Controller
    {
        [HttpPost("register")]
        public async Task<IActionResult> RegisterAsync(UserDto request)
        {
            var user = await authService.RegisterAsync(request);

            if (user == null) return BadRequest();

            return Ok(user);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync(LoginUserDto request)
        {
            var token = await authService.LoginAsync(request);

            if (token == null) return BadRequest();

            return Ok(token);
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var result = await authService.RefreshTokensAsync(request);

            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");

            return Ok(result);
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout(LogoutRequestDto request)
        {
            var success = await authService.LogoutAsync(request.UserId, request.RefreshToken);
            if (!success)
                return BadRequest("Invalid logout request.");

            return Ok("Logged out successfully.");
        }


    }
}
