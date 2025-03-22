using EventManagementServer.Dto;
using EventManagementServer.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EventManagementServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : Controller
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<IActionResult> RegisterAsync(UserDto request)
        {
            var user = await _authService.RegisterAsync(request);

            if (user == null) return BadRequest();

            _logger.LogInformation($"User {user.UserID} registered.");

            return Ok(user);
        }

        [HttpPost("login")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<IActionResult> LoginAsync(LoginUserDto request)
        {
            var token = await _authService.LoginAsync(request);

            if (token == null) return BadRequest();

            _logger.LogInformation($"User {request.Email} logged in.");

            return Ok(token);
        }

        [HttpPost("refresh-token")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<IActionResult> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            var result = await _authService.RefreshTokensAsync(request);

            if (result is null || result.AccessToken is null || result.RefreshToken is null)
                return Unauthorized("Invalid refresh token.");

            _logger.LogInformation($"User refreshed token.");

            return Ok(result);
        }

        [HttpPost("logout")]
        [EnableRateLimiting("FixedWindowLimiter")]
        public async Task<IActionResult> Logout(LogoutRequestDto request)
        {
            var success = await _authService.LogoutAsync(request.UserId, request.RefreshToken);
            if (!success)
                return BadRequest("Invalid logout request.");

            _logger.LogInformation($"User {request.UserId} logged out.");

            return Ok("Logged out successfully.");
        }


    }
}
