using System.Security.Claims;
using System.Text;
using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace AuthService.API.Extensions
{
    public static class ApiExtensions
    {
        public static void AddApiAuthentication(
            this IServiceCollection services,
            IConfiguration configuration
        )
        {
            var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();

            services
                .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(
                    JwtBearerDefaults.AuthenticationScheme,
                    opts =>
                    {
                        opts.TokenValidationParameters = new TokenValidationParameters
                        {
                            ValidateIssuer = false,
                            ValidateAudience = false,
                            ValidateLifetime = true,
                            ClockSkew = TimeSpan.Zero,
                            ValidateIssuerSigningKey = true,
                            IssuerSigningKey = new SymmetricSecurityKey(
                                Encoding.UTF8.GetBytes(jwtOptions!.SecretKey)
                            ),
                            RoleClaimType = ClaimTypes.Role,
                        };

                        opts.Events = new JwtBearerEvents
                        {
                            OnMessageReceived = context =>
                            {
                                context.Token = context.Request.Cookies["tasty-cookies"];

                                return Task.CompletedTask;
                            },
                            // OnAuthenticationFailed = async (context) =>
                            // {
                            //     if (context.Exception is SecurityTokenExpiredException)
                            //     {
                            //         if (context.Request.Cookies.TryGetValue("tasty-cookies", out var accessToken) &&
                            //             context.Request.Cookies.TryGetValue("refresh-tasty-cookies",
                            //                 out var refreshToken))
                            //         {
                            //             var identityService = context.HttpContext.RequestServices
                            //                 .GetRequiredService<IIdentityService>();
                            //
                            //             var currentTokens = new TokenInfoDTO
                            //                 { AccessToken = accessToken, RefreshToken = refreshToken };
                            //
                            //             var (newTokens, status) =
                            //                 await identityService.RefreshTokenAsync(currentTokens);
                            //
                            //             if (status.IsError)
                            //             {
                            //                 throw new Exception();
                            //             }
                            //
                            //             context.Response.Cookies.Append("tasty-cookies", newTokens!.AccessToken);
                            //             context.Response.Cookies.Append("refresh-tasty-cookies", newTokens!.RefreshToken);
                            //             context.Success();
                            //             await context.Response.CompleteAsync();
                            //         }
                            //     }
                            // },
                        };
                    }
                );

            services.AddAuthorization();
        }
    }
}