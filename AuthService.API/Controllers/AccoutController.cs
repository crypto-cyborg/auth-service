using AuthService.Application.ServiceClients;
using AuthService.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Protocols.Configuration;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccoutController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly IConfiguration _configuration;

        // private readonly ICacheService _cacheService;
        private readonly InternalCacheService _cacheService;
        private readonly UserServiceClient _userServiceClient;

        public AccoutController(
            UserService userService,
            IConfiguration configuration,
            InternalCacheService cacheService,
            UserServiceClient userServiceClient
        )
        {
            _userService = userService;
            _configuration = configuration;
            _cacheService = cacheService;
            _userServiceClient = userServiceClient;
        }

        [HttpGet]
        public async Task<IActionResult> Authorize()
        {
            var token = HttpContext.Request.Cookies[
                _configuration.GetSection("cookie-name").Value
                    ?? throw new InvalidConfigurationException("Cannot find cookie key")
            ];

            if (token is null)
            {
                return Unauthorized();
            }

            var user = await _userService.GetSelf(token);

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
    }
}
