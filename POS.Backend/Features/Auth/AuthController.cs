using Microsoft.AspNetCore.Mvc;
using POS.Shared.Models;
using POS.Backend.Common;

namespace POS.Backend.Features.Auth
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthServices _authServices;
        private readonly ICurrentUserService _currentUser;

        public AuthController(IAuthServices authServices, ICurrentUserService currentUser)
        {
            _authServices = authServices;
            _currentUser = currentUser;
        }

        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            var result = await _authServices.LoginAsync(request);
            if (!result.IsSuccess)
                return Unauthorized(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("refresh-token")]
        [Microsoft.AspNetCore.Authorization.AllowAnonymous]
        public async Task<IActionResult> RefreshToken()
        {
            var token = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Refresh token not found.");

            var result = await _authServices.RefreshTokenAsync(token);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(result.Value);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken()
        {
            var token = Request.Cookies["refresh_token"];
            if (string.IsNullOrEmpty(token))
                return BadRequest("Refresh token not found.");

            var result = await _authServices.RevokeTokenAsync(token);
            if (!result.IsSuccess)
                return BadRequest(result.Error);

            return Ok(new { Message = "Token revoked successfully." });
        }

        [HttpGet("me")]
        public IActionResult Me()
        {
            if (!_currentUser.IsAuthenticated)
                return Unauthorized();

            return Ok(new AuthResponse
            {
                Username = _currentUser.Username,
                Role = _currentUser.Role.ToString(),
                UserId = _currentUser.UserId,
                MerchantId = _currentUser.MerchantId,
                BranchId = _currentUser.BranchId
            });
        }
    }
}
