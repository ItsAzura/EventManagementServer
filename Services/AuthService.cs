using EventManagementServer.Data;
using EventManagementServer.Dto;
using EventManagementServer.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace EventManagementServer.Services
{
    public class AuthService : IAuthService
    {

        private readonly EventDbContext context;
        private readonly IConfiguration configuration;
        private readonly PasswordHasher<User> passwordHasher;
        private readonly SigningCredentials credentials;

        public AuthService(EventDbContext context, IConfiguration configuration)
        {
            this.context = context;
            this.configuration = configuration;
            passwordHasher = new PasswordHasher<User>();

            // Tạo key và credentials một lần duy nhất
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["AppSettings:Token"]));
            credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha512);
        }

        public async Task<TokenResponseDto?> LoginAsync(LoginUserDto request)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null) return null;

            //Kiểm tra password có đúng không?
            if(passwordHasher.VerifyHashedPassword(user, user.PasswordHash, request.PasswordHash) == PasswordVerificationResult.Failed) 
                return null;

            return await CreateTokenResponse(user);
        }

        private async Task<TokenResponseDto?> CreateTokenResponse(User user)
        {
            return new TokenResponseDto
            {
                AccessToken = Createtoken(user),
                RefreshToken = await GenerateAndSaveRefreshTokenAsync(user)
            };
        }

        private string Createtoken(User user)
        {
            //Tạo 1 danh sách các claim chứa thông tin của user
            var claim = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.RoleID.ToString())
            };

            //Tạo token
            var tokenDescriptor = new JwtSecurityToken(
                issuer: configuration.GetValue<string>("AppSettings:Issuer"),
                audience: configuration.GetValue<string>("AppSettings:Audience"),
                claims: claim,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: credentials
            );

            //Trả về token 
            return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);

        }

        private async Task<string> GenerateAndSaveRefreshTokenAsync(User user)
        {
            //Tạo refresh token
            var refreshToken = GenerateRefreshToken();

            //Lưu refresh token vào database
            user.RefreshToken = (string?)refreshToken;

            //Lưu thời gian hết hạn của refresh token
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

            await context.SaveChangesAsync();

            return (string)refreshToken;

        }

        private object GenerateRefreshToken()
        {
            //Tạo 1 mảng byte chứa 32 byte ngẫu nhiên
            var randomNumber = new byte[32];

            //Tạo số ngẫu nhiên
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }

            //Trả về chuỗi base64 của mảng byte
            return Convert.ToBase64String(randomNumber);

        }

        public async Task<TokenResponseDto?> RefreshTokensAsync(RefreshTokenRequestDto request)
        {
            //Kiểm tra refresh token có hợp lệ không?
            var user = await context.Users.FirstOrDefaultAsync(u =>
                u.UserID == request.UserId &&
                u.RefreshToken == request.RefreshToken &&
                u.RefreshTokenExpiryTime >= DateTime.UtcNow);

            //Nếu không hợp lệ thì trả về null
            if (user is null) return null;

            //Nếu hợp lệ thì tạo mới token
            return await CreateTokenResponse(user);

        }

        private async Task<User?> ValidateRefreshTokenAsync(int userId, string refreshToken)
        {
            //Tìm user trong database
            var user = await context.Users.FirstOrDefaultAsync(u => u.UserID == userId);

            //Kiểm tra refresh token có hợp lệ không?
            if(user == null || user.RefreshToken != refreshToken || user.RefreshTokenExpiryTime < DateTime.UtcNow) 
                return null;

            //Nếu hợp lệ thì trả về user
            return user;
        }

        public async Task<User?> RegisterAsync(UserDto request)
        {
            bool existingUser = await context.Users.AnyAsync(u => u.Email == request.Email);

            if (existingUser) return null;

            var user = new User();

            user.UserName = request.UserName;
            user.Email = request.Email;

            user.PasswordHash = passwordHasher.HashPassword(user, request.PasswordHash);

            var userRole = await context.Roles.FirstOrDefaultAsync(r => r.RoleName == "User");

            if (userRole == null) return null;

            user.RoleID = userRole.RoleID;
           
            context.Users.Add(user);
            await context.SaveChangesAsync();

            return user;

        }

        public async Task<bool> LogoutAsync(int userId, string refreshToken)
        {
            var user = await context.Users.FindAsync(userId);

            if (user is null || user.RefreshToken != refreshToken)
                return false; // Không tìm thấy user hoặc token không hợp lệ

            // Xóa Refresh Token và lưu thay đổi vào database
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.UtcNow; // Coi như token đã hết hạn ngay lập tức
            await context.SaveChangesAsync();

            return true;
        }

        public async Task<TokenResponseDto?> GoogleLoginAsync(GoogleLoginRequest request)
        {
            try
            {
                var settings = new GoogleJsonWebSignature.ValidationSettings()
                {
                    Audience = new[] { configuration["Google:ClientId"] } // Lấy Google Client ID từ cấu hình
                };

                var payload = await GoogleJsonWebSignature.ValidateAsync(request.TokenId, settings);

                // Kiểm tra xem user đã tồn tại trong database chưa
                var user = await context.Users.FirstOrDefaultAsync(u => u.Email == payload.Email);

                if (user == null)
                {
                    // Tạo user mới nếu chưa tồn tại
                    user = new User
                    {
                        Email = payload.Email,
                        UserName = payload.Name,
                        RoleID = 2 // Gán role mặc định (User)
                    };
                    context.Users.Add(user);
                    await context.SaveChangesAsync();
                }

                return await CreateTokenResponse(user);
            }
            catch (Exception ex)
            {
                return null; // Trả về null nếu token không hợp lệ
            }
        }

    }
}
