using AuthService.Application.Interfaces;
using AuthService.Persistence.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.API.Controllers
{
    [ApiController]
    public class AccoutController : ControllerBase
    {
        private readonly ICacheService _cacheService;
        private readonly IUserRepository _userRepository;

        public AccoutController(ICacheService cacheService, IUserRepository userRepository)
        {
            _cacheService = cacheService;
            _userRepository = userRepository;
        }

        [HttpPost("verify")]
        public async Task<IActionResult> Verify(string token)
        {
            var data = await _cacheService.Get<string>(token);

            if (data is null)
            {
                return NotFound("Token is expired or does not exist");
            }

            var user = (await _userRepository.Get(u => u.Id.ToString() == data)).First();
            user.IsEmailConfirmed = true;

            //await _userRepository.SaveAsync();

            Console.WriteLine($"--> Verified user {user.Username}");

            return Ok("Confirmed");
        }
    }
}
