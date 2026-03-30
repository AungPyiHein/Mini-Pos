using Microsoft.AspNetCore.Mvc;
using POS.Backend.Features.Auth;
using POS.Shared.Models.Auth;

namespace POS.Backend.Features.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;

         public AuthController(IAuthServices authServices)
        {
            _authServices = authServices;
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authServices.LoginAsync(request);
            if (!result.IsSuccess)
            {
                return Unauthorized(result.Error);
            }
            return Ok(result.Value);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok(new { Message = "Logged out successfully." });
        }
    }
}
