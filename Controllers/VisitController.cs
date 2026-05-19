using FilmMaker.Common;
using FilmMaker.DTO.LocationVisit;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    public class VisitController : Controller
    {
        readonly ILocationVisitService _locationVisitService;
        readonly FilmMakerDbContext _context;

        public VisitController(ILocationVisitService locationVisitService, FilmMakerDbContext context)
        {
            _locationVisitService = locationVisitService;
            _context = context;
        }

        [HttpPost("create-visit-request")]
        public async Task<ActionResult<ApiResponse<VisitRequestResponseDto>>> CreateVisitRequest([FromBody]CreateVisitRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var user = await _context.Users
                .Include(u => u.LocationManagerProfile)
                .FirstOrDefaultAsync(u => u.Id == currentUserId.Value);

            if (user?.LocationManagerProfile == null)
            {
                return Unauthorized(
                    ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "No manager profile found.",
                        "لا يوجد ملف مدير مرتبط بهذا المستخدم."
                    ));
            }

            var managerProfileId = user.LocationManagerProfile.Id;

            var response = await _locationVisitService
                .CreateVisitRequestAsync(managerProfileId, dto);

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

        [HttpDelete("cancel-visit-request/{requestId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelVisitRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<bool>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var user = await _context.Users
                .Include(u => u.LocationManagerProfile)
                .FirstOrDefaultAsync(u => u.Id == currentUserId.Value);

            if (user?.LocationManagerProfile == null)
            {
                return Unauthorized(
                    ApiResponse<bool>.FailureResponse(
                        "No manager profile found.",
                        "لا يوجد ملف مدير مرتبط بهذا المستخدم."
                    ));
            }

            var managerProfileId = user.LocationManagerProfile.Id;

            var response = await _locationVisitService.CancelVisitRequestAsync(requestId, managerProfileId);

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
