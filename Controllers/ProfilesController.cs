using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.Profile.Request;
using FilmMaker.DTO.Profile.Response;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProfilesController : ControllerBase
    {
        private readonly IProfileService _profileService;
        public ProfilesController(IProfileService profileService)
        {
            _profileService = profileService;
        }

        [AuthorizeLocationManager]
        [HttpGet("GetMyLocationManagerProfile")]
        public async Task<ActionResult<ApiResponse<LocationManagerProfileResponseDto>>> GetMyLocationManagerProfile()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.GetMyLocationManagerProfile(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [AuthorizeProductionCompany]
        [HttpGet("GetMyProductionCompanyProfile")]
        public async Task<ActionResult<ApiResponse<ProductionCompanyProfileResponseDto>>> GetMyProductionCompanyProfile()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.GetMyProductionCompanyProfile(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [AuthorizeServiceProvider]
        [HttpGet("GetMyServiceProviderProfile")]
        public async Task<ActionResult<ApiResponse<ServiceProviderProfileResponseDto>>> GetMyServiceProviderProfile()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.GetMyServiceProviderProfile(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [AuthorizeLocationOwner]
        [HttpGet("GetMyLocationOwnerProfile")]
        public async Task<ActionResult<ApiResponse<LocationOwnerProfileResponseDto>>> GetMyLocationOwnerProfile()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.GetMyLocationOwnerProfile(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [AuthorizeLocationManager]
        [HttpPut("CompleteLocationManagerProfile")]
        public async Task<ActionResult<ApiResponse<LocationManagerProfileResponseDto>>> CompleteLocationManagerProfile(
            [FromBody] CompleteLocationManagerProfileRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.CompleteLocationManagerProfile(request, currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [AuthorizeLocationManager]
        [HttpPut("UpdateLocationManagerProfile")]
        public async Task<ActionResult<ApiResponse<LocationManagerProfileResponseDto>>> UpdateLocationManagerProfile(
            [FromBody] UpdateLocationManagerProfileRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.UpdateLocationManagerProfile(request, currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [AuthorizeProductionCompany]
        [HttpPut("UpdateProductionCompanyProfile")]
        public async Task<ActionResult<ApiResponse<ProductionCompanyProfileResponseDto>>> UpdateProductionCompanyProfile(
                    [FromBody] UpdateProductionCompanyProfileRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.UpdateProductionCompanyProfile(request, currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        


        [AuthorizeServiceProvider]
        [HttpPut("UpdateServiceProviderProfile")]
        public async Task<ActionResult<ApiResponse<ServiceProviderProfileResponseDto>>> UpdateServiceProviderProfile(
            [FromBody] UpdateServiceProviderProfileRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.UpdateServiceProviderProfile(request, currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        

        [AuthorizeLocationOwner]
        [HttpPut("UpdateLocationOwnerProfile")]
        public async Task<ActionResult<ApiResponse<LocationOwnerProfileResponseDto>>> UpdateLocationOwnerProfile(
                [FromBody] UpdateLocationOwnerProfileRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _profileService.UpdateLocationOwnerProfile(request, currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return null;
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}
