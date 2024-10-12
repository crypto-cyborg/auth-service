﻿using AuthService.Application.Infrastructure.Interfaces;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Core.Models;
using AuthService.Persistence.Data;
using AuthService.Persistence.Data.Dtos;
using AuthService.Persistence.Extensions;
using AuthService.Persistence.Repositories.Interfaces;
using Microsoft.Extensions.Configuration;

namespace AuthService.Application.Services
{
    public class UserService
    {
        private readonly IPasswordHasher _passwordHasher;
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;
        private readonly IEmailSender _emailSender;
        private readonly UserServiceClient _userServiceClient;

        public UserService(
            IPasswordHasher passwordHasher,
            IUserRepository userRepository,
            ITokenService tokenService,
            IConfiguration configuration,
            ICacheService cacheService,
            IEmailSender emailSender,
            UserServiceClient userServiceClient)
        {
            _passwordHasher = passwordHasher;
            _userRepository = userRepository;
            _tokenService = tokenService;
            _configuration = configuration;
            _cacheService = cacheService;
            _emailSender = emailSender;
            _userServiceClient = userServiceClient;
        }

        public async Task<User> SignUp(SignUpDTO request)
        {
            string passwordHash = _passwordHasher.Generate(request.Password);

            var result = await _userServiceClient.CreateUser(request);

            //await SendVerification(user);

            return result;
        }

        public async Task<(TokenData? tokenData, Status status)> SignIn(
            string username,
            string password)
        {
            var userId = await _userServiceClient.GetUserId(username);
            var user = await _userServiceClient.GetUser(userId);

            if (user == null)
            {
                return (
                    null,
                    new()
                    {
                        Code = 404,
                        IsError = true,
                        Message = "User not found"
                    }
                );
            }

            var isPasswordValid = _passwordHasher.Verify(password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return (
                    null,
                    new()
                    {
                        Code = 401,
                        IsError = true,
                        Message = "Invalid username or password"
                    }
                );
            }

            var accesstoken = await _tokenService.Generate(user);
            var refreshToken = _tokenService.GenerateRandomToken();
            var refreshTokenExpires = DateTime.UtcNow.AddDays(1);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpires;

            //await _userRepository.SaveAsync();

            return (
                new()
                {
                    AccessToken = accesstoken,
                    RefreshToken = refreshToken,
                    RefreshTokenExpired = refreshTokenExpires,
                },
                new()
                {
                    Code = 200,
                    Message = "Sign in successful",
                    IsError = false,
                }
            );
        }

        public async Task<(TokenData? tokenData, Status status)> RefreshTokenAsync(TokenInfoDTO data)
        {
            ArgumentNullException.ThrowIfNull(data);

            var principal = await _tokenService.GetPrincipalFromExpiredToken(data.AccessToken);

            var userId = principal.Claims.FirstOrDefault(c => c.Type == "userId")?.Value;
            var user = (await _userRepository.Get(u => u.Id.ToString() == userId)).First();

            if (user is null ||
                user.RefreshToken != data.RefreshToken ||
                user.RefreshTokenExpiryTime < DateTime.Now)
            {
                return (
                    null,
                    new()
                    {
                        Code = 400,
                        Message = "Cannot refresh token",
                        IsError = true,
                    });
            }

            var accessToken = await _tokenService.Generate(user);
            var refreshToken = _tokenService.GenerateRandomToken();
            var expiryTime = DateTime.UtcNow.AddDays(1);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = expiryTime;

            //await _userRepository.SaveAsync();

            return (
                new()
                {
                    AccessToken = accessToken,
                    RefreshToken = refreshToken,
                    RefreshTokenExpired = expiryTime
                },
                new()
                {
                    Code = 200,
                    Message = "Refreshed successfully",
                    IsError = false
                }
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
                DateTime.UtcNow.AddMinutes(5));

            await _emailSender.SendAsync(user.Email, subject, body);
        }
    }
}
