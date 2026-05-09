using FilmMaker.DTO.Auth.Request;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Http;
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
    }
}
