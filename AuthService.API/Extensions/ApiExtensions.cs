using System.Net;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;

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
                .AddAuthentication(opts =>
                {
                    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(
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
                                context.Token = context.Request.Cookies[configuration["cookie-name"]!];

                                return Task.CompletedTask;
                            },
                            OnAuthenticationFailed = async context =>
                            {
                                var endpoint = context.HttpContext.GetEndpoint();
                                var requiresAuth = endpoint?.Metadata?.GetMetadata<AuthorizeAttribute>() is not null;

                                if (!requiresAuth) return; 
                                
                                Console.WriteLine("--> Token expired");
                                
                                if (context.Exception is not SecurityTokenExpiredException) return;

                                var accessToken = context.Request.Cookies["tasty-cookies"];
                                var refreshToken = context.Request.Cookies["refresh-tasty-cookies"];

                                if (accessToken is null || refreshToken is null) return;

                                var identityService =
                                    context.HttpContext.RequestServices.GetRequiredService<IIdentityService>();

                                var (newTokens, status) = await identityService.RefreshTokenAsync(
                                    new TokenInfoDTO(accessToken, refreshToken));

                                var baseAddress = new Uri(context.Request.Host.Value);
                                var cookies = new CookieContainer();

                                using var handler = new HttpClientHandler();
                                handler.CookieContainer = cookies;

                                using var client = new HttpClient(handler);
                                client.BaseAddress = baseAddress;

                                Console.WriteLine($"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}{context.Request.Path}");

                                cookies.Add(baseAddress,
                                [
                                    new Cookie("tasty-cookies", newTokens!.AccessToken),
                                    new Cookie("refresh-tasty-cookies", newTokens.RefreshToken)
                                ]);

                                var message = new HttpRequestMessage
                                {
                                    Method = new HttpMethod(context.Request.Method),
                                    RequestUri = new Uri($"{context.HttpContext.Request.Scheme}://{context.HttpContext.Request.Host}/{context.Request.Path}"),
                                    Content = new StreamContent(context.Request.Body)
                                };

                                foreach (var header in context.Request.Headers)
                                {
                                    message.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
                                }

                                var res = await client.SendAsync(message);

                                Console.WriteLine(await res.Content.ReadAsStringAsync());
                            }
                        };
                    }
                );

            services.AddAuthorization();
        }
    }
}