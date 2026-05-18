using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.Visit;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [AuthorizeLocationOwner]
    [Route("api/[controller]")]
    [ApiController]
    public class LocationVisitRequestsController : ControllerBase
    {
        private readonly ILocationOwnerVisitRequestService _locationOwnerVisitRequestService;

        public LocationVisitRequestsController(ILocationOwnerVisitRequestService locationOwnerVisitRequestService)
        {
            _locationOwnerVisitRequestService = locationOwnerVisitRequestService;
        }

        [HttpGet("GetReceivedVisitRequests")]
        public async Task<ActionResult<ApiResponse<List<LocationVisitRequestResponseDto>>>> GetReceivedVisitRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationOwnerVisitRequestService.GetReceivedVisitRequests(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("GetReceivedVisitRequestById")]
        public async Task<ActionResult<ApiResponse<LocationVisitRequestResponseDto>>> GetReceivedVisitRequestById(int visitRequestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationOwnerVisitRequestService.GetReceivedVisitRequestById(visitRequestId,currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("RespondVisitRequest")]
        public async Task<ActionResult<ApiResponse<LocationVisitRequestResponseDto>>> RespondVisitRequest([FromBody] RespondLocationVisitRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationOwnerVisitRequestService.RespondVisitRequest(request,currentUserId.Value);

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
