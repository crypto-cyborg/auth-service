using AuthService.Application;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Interfaces;
using AuthService.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController(
        IConfiguration configuration,
        ICookiesService cookiesService,
        IIdentityService identityService,
        ITokenService tokenService
    ) : ControllerBase
    {
        private readonly IIdentityService _identityService = identityService;
        private readonly IConfiguration _configuration = configuration;
        private readonly ICookiesService _cookiesService = cookiesService;
        private readonly ITokenService _tokenService = tokenService;

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken()
        {
            var request = _cookiesService.ReadToken(HttpContext);
            var (tokenData, status) = await _identityService.RefreshTokenAsync(request);

            if (status.IsError)
            {
                return BadRequest(status);
            }

            _cookiesService.WriteToken(tokenData!, HttpContext);

            return Ok(tokenData);
        }

        [HttpPost("signup")]
        public async Task<IActionResult> SignUp([FromBody] SignUpDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var response = await _identityService.SignUp(request);

            return Created(nameof(SignUp), response);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (tokenData, status) = await _identityService.SignIn(
                request.Username,
                request.Password
            );

            if (status.IsError)
            {
                return BadRequest(status);
            }

            _cookiesService.WriteToken(tokenData!, HttpContext);

            return Ok(status);
        }

        [HttpPost("check")]
        [Authorize]
        public IActionResult Check()
        {
            return Ok("Authorized");
        }
    }
}
