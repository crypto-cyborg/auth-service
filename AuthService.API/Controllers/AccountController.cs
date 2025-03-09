using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.Services.Interfaces;
using AuthService.Application.Validators;
using AuthService.Core.Extesions;
using AuthService.Core.Factories;
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
        public async Task<IActionResult> Authorize()
        {
            var tokenData = cookiesService.ReadToken(HttpContext);

            var user = await accountService.GetSelf(tokenData!.AccessToken);

            return Ok(user.MapToResponse());
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

        [HttpPost("password/reset")]
        public async Task<IActionResult> ResetPassword(ResetPasswordValidator validator, ResetPasswordDto request)
        {
            var validationResult = await validator.ValidateAsync(request);
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
