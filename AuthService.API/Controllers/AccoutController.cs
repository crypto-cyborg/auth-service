﻿using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services;
using AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccoutController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAccountService _accountService;
        private readonly InternalCacheService _cacheService;
        private readonly UserServiceClient _userServiceClient;
        private readonly ITokenService _tokenService;

        public AccoutController(
            IConfiguration configuration,
            InternalCacheService cacheService,
            UserServiceClient userServiceClient,
            IAccountService accountService,
            ITokenService tokenService
        )
        {
            _configuration = configuration;
            _cacheService = cacheService;
            _userServiceClient = userServiceClient;
            _accountService = accountService;
            _tokenService = tokenService;
        }

        [HttpGet]
        public async Task<IActionResult> Authorize()
        {
            var token = _tokenService.ReadToken(HttpContext);

            if (token is null)
            {
                return Unauthorized();
            }

            var user = await _accountService.GetSelf(token);

            return Ok(user);
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var data = await _cacheService.Get(token);

            if (data == default)
            {
                return NotFound("Token is expired or does not exist");
            }

            var user = await _userServiceClient.GetUser(data);
            user.IsEmailConfirmed = true;

            await _userServiceClient.UpdateUser(user);

            await _cacheService.Remove(token);

            Console.WriteLine($"--> Verified user {user.Username}");

            return NoContent();
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(ResetPasswordDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var token = _tokenService.ReadToken(HttpContext);

            if (token is null)
            {
                return Unauthorized();
            }

            var userId = (await _tokenService.GetClaims(token)).Claims.FirstOrDefault(c =>
                c.Type == "userId"
            );

            System.Console.WriteLine(userId.Value);

            await _accountService.ResetPassword(new Guid(userId!.Value), request);

            return NoContent();
        }
    }
}
