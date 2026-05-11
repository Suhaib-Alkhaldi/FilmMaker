using FilmMaker.Attribute;
using FilmMaker.DTO.Auth.Request;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

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
            var result = await _authService.Login(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

    }
}
