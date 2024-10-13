using AuthService.Application.Interfaces;
using AuthService.Application.ServiceClients;
using AuthService.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccoutController : ControllerBase
    {
        private readonly UserService _userService;

        // private readonly ICacheService _cacheService;

        public AccoutController(UserService userService)
        {
            _userService = userService;
            // _cacheService = cacheService;
        }

        [HttpGet]
        public async Task<IActionResult> Authorize()
        {
            var token = HttpContext.Request.Cookies["tasty-cookies"];

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
            // var data = await _cacheService.Get<string>(token);

            // if (data is null)
            // {
            //     return NotFound("Token is expired or does not exist");
            // }

            // var user = (await _userRepository.Get(u => u.Id.ToString() == data)).First();
            // user.IsEmailConfirmed = true;

            // //await _userRepository.SaveAsync();

            // Console.WriteLine($"--> Verified user {user.Username}");

            return Ok("Confirmed");
        }
    }
}
