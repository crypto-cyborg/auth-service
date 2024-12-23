using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Infrastructure.Interfaces;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services.Interfaces;
using AuthService.Core.Exceptions;
using AuthService.Core.Factories;
using AuthService.Core.Models;

namespace AuthService.Application.Services
{
    public class IdentityService(
        IPasswordHasher passwordHasher,
        ITokenService tokenService,
        IEmailSender emailSender,
        UserServiceClient userServiceClient,
        InternalCacheService cacheService,
        IBinanceService binanceService
    ) : IIdentityService
    {
        public async Task<User> SignUp(SignUpDTO request)
        {
            var keyValidationResult = await binanceService.ValidateKeys(request.ApiKey, request.SecretKey);

            if (!keyValidationResult)
            {
                throw new AuthServiceExceptions("Invalid binance keys", AuthServiceExceptionTypes.INVALID_KEYS);
            }
            
            var result = await userServiceClient.CreateUser(request);

            await SendVerification(result);

            return result;
        }

        public async Task<(TokenData? tokenData, Status status)> SignIn(
            string username,
            string password
        )
        {
            var user = await userServiceClient.GetUser(username);

            if (user is null)
            {
                return (null, StatusFactory.Create(404, "User not found", true));
            }

            var isPasswordValid = passwordHasher.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return (null, StatusFactory.Create(401, "Invalid username or password", true));
            }

            var accessToken = await tokenService.Generate(user);
            var refreshToken = tokenService.GenerateRefreshToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(1);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpires;

            await userServiceClient.UpdateUser(user);

            return (
                new(
                    accessToken,
                    refreshToken,
                    refreshTokenExpires
                ),
                StatusFactory.Create(200, "Sign in successful", false)
            );
        }

        public async Task<(TokenData? tokenData, Status status)> RefreshTokenAsync(
            TokenInfoDTO data
        )
        {
            ArgumentNullException.ThrowIfNull(data);

            var principal = await tokenService.GetClaimsIdentity(data.AccessToken);

            var userId = principal.Claims.FirstOrDefault(c => c.Type == "userId")!.Value;
            var user = await userServiceClient.GetUser(new Guid(userId));

            if (
                user.RefreshToken != data.RefreshToken
                || user.RefreshTokenExpiryTime < DateTime.UtcNow
            )
            {
                return (null, StatusFactory.Create(400, "Cannot refresh token", true));
            }

            var accessToken = await tokenService.Generate(user);
            var refreshToken = tokenService.GenerateRefreshToken();
            var expiryTime = DateTime.UtcNow.AddDays(1);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;

            await userServiceClient.UpdateUser(user);

            return (
                new TokenData(
                    accessToken,
                    refreshToken,
                    expiryTime
                ),
                StatusFactory.Create(200, "Refreshed successfully", false)
            );
        }

        public async Task SignOut(TokenInfoDTO tokens)
        {
            var claimsIdentity = await tokenService.GetClaimsIdentity(tokens.AccessToken);
            var userId = new Guid(claimsIdentity.Claims.First(c => c.Type == "userId").Value);

            var user = await userServiceClient.GetUser(userId);

            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = DateTime.Now;

            await userServiceClient.UpdateUser(user);
        }

        private async Task SendVerification(User user)
        {
            var verificationToken = tokenService.GenerateEmailToken(user);

            const string subject = "Account confirmation";
            var body = emailSender.GetPrettyConfirmation(
                $"http://localhost:5062/api/account/verify?token={verificationToken}");

            await cacheService.Set(verificationToken, user.Id);

            await emailSender.SendAsync(user.Email, subject, body);
        }
    }
}