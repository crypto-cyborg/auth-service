using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.Services.Interfaces;
using AuthService.Application.Validators;
using AuthService.Persistence.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(
        IAccountService accountService,
        ITokenService tokenService,
        ICookiesService cookiesService)
        : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Authorize()
        {
            var tokenData = cookiesService.ReadToken(HttpContext);

            var user = await accountService.GetSelf(tokenData!.AccessToken);

            return Ok(user);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            var validationResult = await tokenService.Validate(token: token, lifetime: true);

            if (!validationResult.IsValid)
            {
                return BadRequest(StatusFactory.Create(500, "Invalid token", true));
            }

            await accountService.ConfirmEmail(token);

            return NoContent();
        }

        [HttpPost("reset-password")]
        [Authorize]
        public async Task<IActionResult> ResetPassword(ResetPasswordValidator validator, ResetPasswordDto request)
        {
            var validationResult = validator.Validate(request);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult);
            }

            var tokenData = cookiesService.ReadToken(HttpContext);

            await accountService.ResetPassword(tokenData!.AccessToken, request);

            return NoContent();
        }
    }
}
