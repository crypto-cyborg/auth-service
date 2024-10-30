using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Infrastructure.Interfaces;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services.Interfaces;
using AuthService.Core.Models;
using AuthService.Persistence.Extensions;

namespace AuthService.Application.Services
{
    public class IdentityService(
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailSender emailSender,
        UserServiceClient userServiceClient,
        InternalCacheService cacheService
    ) : IIdentityService
    {
        private readonly IPasswordHasher _passwordHasher = passwordHasher;
        private readonly ITokenService _tokenService = tokenService;
        private readonly InternalCacheService _cacheService = cacheService;
        private readonly IEmailSender _emailSender = emailSender;
        private readonly UserServiceClient _userServiceClient = userServiceClient;

        public async Task<User> SignUp(SignUpDTO request)
        {
            var result = await _userServiceClient.CreateUser(request);

            await SendVerification(result);

            return result;
        }

        public async Task<(TokenData? tokenData, Status status)> SignIn(
            string username,
            string password
        )
        {
            var user = await _userServiceClient.GetUser(username);

            if (user == null)
            {
                return (null, StatusFactory.Create(404, "User not found", true));
            }

            var isPasswordValid = _passwordHasher.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return (null, StatusFactory.Create(401, "Invalid username or password", true));
            }

            var accesstoken = await _tokenService.Generate(user);
            var refreshToken = _tokenService.GenerateRandomToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(1);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpires;

            await _userServiceClient.UpdateUser(user);

            return (
                new()
                {
                    AccessToken = accesstoken,
                    RefreshToken = refreshToken,
                    RefreshTokenExpired = refreshTokenExpires,
                },
                StatusFactory.Create(200, "Sign in successful", false)
            );
        }

        public async Task<(TokenData? tokenData, Status status)> RefreshTokenAsync(
            TokenInfoDTO data
        )
        {
            ArgumentNullException.ThrowIfNull(data);

            var principal = await _tokenService.GetClaims(data.AccessToken);

            var userId = principal.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var user = await _userServiceClient.GetUser(new Guid(userId));

            if (
                user is null
                || user.RefreshToken != data.RefreshToken
                || user.RefreshTokenExpiryTime < DateTime.Now
            )
            {
                return (null, StatusFactory.Create(400, "Cannot refresh token", true));
            }

            var accessToken = await _tokenService.Generate(user);
            var refreshToken = _tokenService.GenerateRandomToken();
            var expiryTime = DateTime.UtcNow.AddDays(1);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;

            await _userServiceClient.UpdateUser(user);

            return (
                new()
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    RefreshTokenExpired = expiryTime,
                },
                StatusFactory.Create(200, "Refreshed successfully", false)
            );
        }

        private async Task SendVerification(User user)
        {
            var verificationToken = _tokenService.GenerateRandomToken();

            string subject = "Account confirmation";
            string body = $"http://localhost:5062/verify?token={verificationToken}";

            await _cacheService.Set(verificationToken, user.Id);

            await _emailSender.SendAsync(user.Email, subject, body);
        }
    }
}
