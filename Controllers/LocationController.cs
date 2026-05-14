using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.Location.Request;
using FilmMaker.DTO.Location.Response;
using FilmMaker.Services.Interface;
using FilmMaker.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;
        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }


        [AuthorizeLocationOwner]
        [HttpGet("GetMyActiveLocations")]
        public async Task<ActionResult<ApiResponse<List<LocationResponseDto>>>> GetMyActiveLocations()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationService.GetMyActiveLocations(
                currentUserId.Value
            );

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [AuthorizeLocationOwner]
        [HttpGet("GetMyArchivedLocations")]
        public async Task<ActionResult<ApiResponse<List<LocationResponseDto>>>> GetMyArchivedLocations()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationService.GetMyArchivedLocations(
                currentUserId.Value
            );

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [AuthorizeLocationOwner]
        [HttpGet("GetMyLocationById")]
        public async Task<ActionResult<ApiResponse<LocationResponseDto>>> GetMyLocationById(
            int locationId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationService.GetMyLocationById(
                locationId,
                currentUserId.Value
            );

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [AllowAnonymous]
        [HttpGet("GetAllActiveLocations")]
        public async Task<ActionResult<ApiResponse<List<LocationResponseDto>>>> GetAllActiveLocations()
        {
            var result = await _locationService.GetAllActiveLocations();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [AllowAnonymous]
        [HttpGet("GetAllArchivedLocations")]
        public async Task<ActionResult<ApiResponse<List<LocationResponseDto>>>> GetAllArchivedLocations()
        {
            var result = await _locationService.GetAllArchivedLocations();

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [AuthorizeLocationOwner]
        [HttpPost("CreateLocation")]
        public async Task<IActionResult> CreateLocation(CreateLocationRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationService.CreateLocation(request,currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [AuthorizeLocationOwner]
        [HttpPut("UpdateLocation")]
        public async Task<ActionResult<ApiResponse<LocationResponseDto>>> UpdateLocation([FromBody] UpdateLocationRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationService.UpdateLocation(request,currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }



        [AuthorizeLocationOwner]
        [HttpPut("ArchiveLocation")]
        public async Task<ActionResult<ApiResponse<bool>>> ArchiveLocation(ArchiveLocationRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationService.ArchiveLocation(request , currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [AuthorizeLocationOwner]
        [HttpPut("RestoreArchivedLocation")]
        public async Task<ActionResult<ApiResponse<bool>>> RestoreArchivedLocation(int locationId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationService.RestoreArchivedLocation(
                locationId,
                currentUserId.Value
            );

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
