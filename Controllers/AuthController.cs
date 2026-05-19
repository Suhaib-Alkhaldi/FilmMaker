using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.Auth.Request;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private string? GetCurrentUserName()
        {
            var userNameClaim = User.FindFirst("FirstName")?.Value;

            if (string.IsNullOrWhiteSpace(userNameClaim))
            {
                return null;
            }

            return userNameClaim;
        }
        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }
        [HttpPost("register-location-owner")]
        public async Task<IActionResult> RegisterLocationOwner(RegisterLocationOwnerRequestDto request)
        {
            var result = await _authService.RegisterLocationOwner(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-location-manager")]
        public async Task<IActionResult> RegisterLocationManager(RegisterLocationManagerRequestDto request)
        {
            var result = await _authService.RegisterLocationManager(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("register-production-company")]
        public async Task<IActionResult> RegisterProductionCompany(RegisterProductionCompanyRequestDto request)
        {
            var result = await _authService.RegisterProductionCompany(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("register-service-provider")]
        public async Task<IActionResult> RegisterServiceProvider(RegisterServiceProviderRequestDto request)
        {
            var result = await _authService.RegisterServiceProvider(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {

            if (User.Identity?.IsAuthenticated == true)
            {
                var userName = GetCurrentUserName();

                return Conflict(new ApiResponse<object> { MessageEn = $"User  {userName} is already logged in." ,MessageAr = $" مسجل الدخول في النظام مسبقاً {userName} المستخدم" });

            }

            var result = await _authService.Login(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
