using AuthService.Application.Data.Dtos;
using AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        ICookiesService cookiesService,
        IIdentityService identityService
    ) : ControllerBase
    {
        [HttpPost("token/refresh")]
        public async Task<IActionResult> RefreshToken(TokenInfoDTO? tokens)
        {
            var request = tokens ?? cookiesService.ReadToken(HttpContext);

            if (request is null) return Unauthorized();
            
            var (tokenData, status) = await identityService.RefreshTokenAsync(request);

            if (status.IsError)
            {
                return BadRequest(status);
            }

            cookiesService.WriteToken(tokenData!, HttpContext);

            return Ok(tokenData);
        }

        [HttpPost("sign-up")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await identityService.SignUp(request);

            return Created(nameof(SignUp), response);
        }

        [HttpPost("sign-in")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (tokenData, status) = await identityService.SignIn(
                request.Username,
                request.Password
            );

            if (status.IsError)
            {
                return BadRequest(status);
            }

            cookiesService.WriteToken(tokenData!, HttpContext);

            return Ok(status);
        }

        [HttpPost("sign-out")]
        public new async Task<IActionResult> SignOut()
        {
            var tokens = cookiesService.ReadToken(HttpContext);

            await identityService.SignOut(tokens!);
            cookiesService.DeleteToken(HttpContext);

            return NoContent();
        }

        [HttpGet("check")]
        [Authorize]
        public IActionResult Check()
        {
            return Ok("This works");
        }
    }
}
