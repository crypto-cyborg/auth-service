using System.Security.Claims;
using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Infrastructure.Interfaces;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Core.Models;
using AuthService.Persistence.Extensions;

namespace AuthService.Application.Services
{
    public class UserService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly ITokenService _tokenService;
        private readonly ICacheService _cacheService;
        private readonly IEmailSender _emailSender;
        private readonly UserServiceClient _userServiceClient;

        public UserService(
            IPasswordHasher passwordHasher,
            ITokenService tokenService,
            ICacheService cacheService,
            IEmailSender emailSender,
            UserServiceClient userServiceClient
        )
        {
            _passwordHasher = passwordHasher;
            _tokenService = tokenService;
            _cacheService = cacheService;
            _emailSender = emailSender;
            _userServiceClient = userServiceClient;
        }

        public async Task<User> SignUp(SignUpDTO request)
        {
            var result = await _userServiceClient.CreateUser(request);

            //await SendVerification(user);

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

            var principal = await _tokenService.GetPrincipalFromExpiredToken(data.AccessToken);

            var username = principal
                .Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier)
                ?.Value;
            var user = await _userServiceClient.GetUser(username);

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

            string subject = "Academy account confirmation";
            string body = $"https://localhost:7171/verify?token={verificationToken}";

            await _cacheService.Set<string>(
                verificationToken,
                user.Id.ToString(),
                DateTime.UtcNow.AddMinutes(5)
            );

            await _emailSender.SendAsync(user.Email, subject, body);
        }
    }
}
