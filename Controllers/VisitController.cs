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
        public async Task<ActionResult<ApiResponse<VisitRequestResponseDto>>> CreateVisitRequest(CreateVisitRequestDto dto)
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
                return Unauthorized(
                    ApiResponse<List<VisitRequestResponseDto>>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

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
            var response = await _locationVisitService.GetVisitRequestByIdAsync(requestId, managerProfileId);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("update-visit-request/{requestId}")]
        public async Task<ActionResult<ApiResponse<VisitRequestResponseDto>>> UpdateVisitRequest(int requestId, UpdateVisitRequestDto dto)
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
            var response = await _locationVisitService.UpdateVisitRequestAsync(requestId, managerProfileId, dto);
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
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim))
                return null;
            return int.Parse(userIdClaim);
        }
    }
}
