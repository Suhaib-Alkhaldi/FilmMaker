using System.Security.Claims;
using FilmMaker.Common;
using FilmMaker.DTOs.ProductionCompany;
using FilmMaker.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [ApiController]
    [Route("api/production-companies")]
    public class ProductionCompanyController : ControllerBase
    {
        private readonly IProductionCompanyService _service;

        public ProductionCompanyController(IProductionCompanyService service)
        {
            _service = service;
        }
        
      

     
        [HttpGet("profile")]
        [Authorize(Roles = "Production Company")]
        public async Task<IActionResult> GetProfile()
        {
            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(ApiResponse<object>.FailureResponse(
                    "Invalid token.",
                    "رمز المصادقة غير صالح."));

            var result = await _service.GetProfileAsync(userId.Value);
            return Ok(result);
        }

      
        [HttpPut("profile")]
        [Authorize(Roles = "Production Company")]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProductionCompanyProfileRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(ApiResponse<object>.FailureResponse(
                    "Invalid token.",
                    "رمز المصادقة غير صالح."));

            var result = await _service.UpdateProfileAsync(userId.Value, request);
            return Ok(result);
        }

       
        [HttpPatch("profile/password")]
        [Authorize(Roles = "Production Company")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var userId = GetCurrentUserId();
            if (userId is null)
                return Unauthorized(ApiResponse<object>.FailureResponse(
                    "Invalid token.",
                    "رمز المصادقة غير صالح."));

            var result = await _service.ChangePasswordAsync(userId.Value, request);
            return Ok(result);
        }


        private int? GetCurrentUserId()
        {
            var claim = User.FindFirstValue("UserId");
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}