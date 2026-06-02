using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.LocationVisit;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [AuthorizeLocationManager]
    [Route("api/[controller]")]
    [ApiController]
    public class VisitController : Controller
    {
        readonly ILocationVisitService _locationVisitService;

        public VisitController(ILocationVisitService locationVisitService)
        {
            _locationVisitService = locationVisitService;

        }

        [HttpPost("create-visit-request")]
        public async Task<ActionResult<ApiResponse<VisitRequestResponseDto>>> CreateVisitRequest([FromBody]CreateVisitRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var response = await _locationVisitService
                .CreateVisitRequestAsync(currentUserId.Value, dto);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [HttpGet("my-visit-requests")]
        public async Task<ActionResult<ApiResponse<List<VisitRequestResponseDto>>>> GetMyVisitRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();
            var result = await _locationVisitService.GetVisitRequestsAsync(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("my-visit-request/{requestId}")]
        public async Task<ActionResult<ApiResponse<VisitRequestResponseDto>>> GetVisitRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();
            var response = await _locationVisitService.GetVisitRequestByIdAsync(requestId, currentUserId.Value);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("UpdateVisitRequest")]
        public async Task<ActionResult<ApiResponse<VisitRequestResponseDto>>> UpdateVisitRequest([FromBody]UpdateVisitRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();
            var response = await _locationVisitService.UpdateVisitRequestAsync(currentUserId.Value, dto);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("cancel-visit-request/{requestId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelVisitRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var response = await _locationVisitService.CancelVisitRequestAsync(requestId, currentUserId.Value);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }


        // Helper methods
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
