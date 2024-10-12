using AuthService.API.Extensions;
using AuthService.Application.Data;
using AuthService.Application.Infrastructure;
using AuthService.Application.Infrastructure.Interfaces;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services;
using AuthService.Application.Validators;
using FluentValidation;
using Microsoft.AspNetCore.CookiePolicy;

Console.WriteLine("--> Application started");

var builder = WebApplication.CreateBuilder(args);

Console.WriteLine($"--> Current environment: {builder.Environment.EnvironmentName}");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApiAuthentication(builder.Configuration);
builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(nameof(JwtOptions)));

builder.Services.AddScoped<UserService>();

builder.Services.AddScoped<IPasswordHasher, PasswordHasher>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ICacheService, CacheService>();
builder.Services.AddScoped<IEmailSender, EmailSender>();

builder.Services.AddScoped<UserServiceClient>();

builder.Services.AddValidatorsFromAssemblyContaining<SignUpValidator>();
builder.Services.AddValidatorsFromAssemblyContaining<SignInValidator>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCookiePolicy(
    new CookiePolicyOptions
    {
        MinimumSameSitePolicy = SameSiteMode.Strict,
        HttpOnly = HttpOnlyPolicy.Always,
        Secure = CookieSecurePolicy.Always,
    }
);

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
