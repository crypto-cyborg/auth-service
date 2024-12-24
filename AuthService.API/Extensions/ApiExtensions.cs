using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AuthService.Application.Data;
using AuthService.Application.Data.Dtos;
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
                                if (context.Exception is not SecurityTokenExpiredException) return;

                                var httpContext = context.HttpContext;
                                var accessToken =
                                    httpContext.Request.Cookies["tasty-cookies"];

                                var refreshToken =
                                    httpContext.Request.Cookies["refresh-tasty-cookies"];

                                if (string.IsNullOrEmpty(refreshToken)) return;

                                var refreshEndpoint =
                                    $"{httpContext.Request.Scheme}://{httpContext.Request.Host}/api/auth/token/refresh";

                                var client = httpContext.RequestServices.GetRequiredService<IHttpClientFactory>().CreateClient();

                                var response = await client.PostAsJsonAsync(refreshEndpoint, new TokenInfoDTO(accessToken, refreshToken));

                                if (response.IsSuccessStatusCode)
                                {
                                    var newTokens = await response.Content.ReadFromJsonAsync<TokenInfoDTO>();
                                    if (newTokens is not null)
                                    {
                                        httpContext.Response.Cookies.Append("tasty-cookies", newTokens.AccessToken, new CookieOptions { HttpOnly = true });
                                        httpContext.Response.Cookies.Append("refresh-tasty-cookies", newTokens.RefreshToken, new CookieOptions { HttpOnly = true });

                                        httpContext.Request.Headers.Authorization = $"Bearer {newTokens.AccessToken}";

                                        var newToken = new JwtSecurityToken(newTokens.AccessToken);
                                        var principal = new ClaimsPrincipal(new ClaimsIdentity(newToken.Claims, "jwt"));

                                        context.Principal = principal;
                                        context.Success();
                                    }
                                }
                            }
                        };
                    }
                );

            services.AddAuthorization();
        }
    }
}
