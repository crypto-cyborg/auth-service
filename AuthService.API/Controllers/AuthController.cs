using AuthService.Application;
using AuthService.Application.Data.Dtos;
using AuthService.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;
        private readonly ICookiesService _cookiesService;

        public AuthController(
            UserService userService,
            IConfiguration configuration,
            ICookiesService cookiesService
        )
        {
            _userService = userService;
            _configuration = configuration;
            _cookiesService = cookiesService;
        }

        [HttpPost("RefreshToken")]
        public async Task<IActionResult> RefreshToken(TokenInfoDTO request)
        {
            var (tokenData, status) = await _userService.RefreshTokenAsync(request);

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

            var response = await _userService.SignUp(request);

            return Created(nameof(SignUp), response);
        }

        [HttpPost("signin")]
        public async Task<IActionResult> SignIn([FromBody] SignInDTO request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (tokenData, status) = await _userService.SignIn(request.Username, request.Password);

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
