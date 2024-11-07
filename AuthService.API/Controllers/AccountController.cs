using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services;
using AuthService.Application.Services.Interfaces;
using AuthService.Persistence.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(
        IAccountService accountService,
        ITokenService tokenService)
        : ControllerBase
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Authorize()
        {
            var tokenData = tokenService.ReadToken(HttpContext);

            var user = await accountService.GetSelf(tokenData!.AccessToken);

            return Ok(user);
        }

        [HttpGet("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var validationResult = await tokenService.Validate(token);

            if (!validationResult.IsValid)
            {
                return BadRequest(StatusFactory.Create(500, "Invalid token", true));
            }

            await accountService.ConfirmEmail(token);

            return NoContent();
        }
    }
}
