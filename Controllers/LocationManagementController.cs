using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.LocationManager;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using FilmMaker.Services.Service;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationManagementController : ControllerBase
    {
        private readonly ILocationManagementServicee _locationManagementService;
        private readonly FilmMakerDbContext _context;

        public LocationManagementController(
            ILocationManagementServicee locationManagementService,
            FilmMakerDbContext context)
        {
            _locationManagementService = locationManagementService;
            _context = context;
        }

        [AuthorizeLocationManager]
        [HttpGet("my-managed-locations")]
        public async Task<ActionResult<ApiResponse<List<LocationSummaryDto>>>> GetManagedLocations()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim))
            {
                return Unauthorized(
                    ApiResponse<List<LocationSummaryDto>>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));
            }

            int userId = int.Parse(userIdClaim);

            var user = await _context.Users
                .Include(u => u.LocationManagerProfile)
                .FirstOrDefaultAsync(u => u.Id == userId);

            if (user?.LocationManagerProfile == null)
            {
                return Unauthorized(
                    ApiResponse<List<LocationSummaryDto>>.FailureResponse(
                        "No manager profile found.",
                        "لا يوجد ملف مدير مرتبط بهذا المستخدم."
                    ));
            }

            var response = await _locationManagementService
                .GetManagedLocationsAsync(user.LocationManagerProfile.Id);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        [AuthorizeLocationManager]
        [HttpGet("my-requests")]
        public async Task<ActionResult<ApiResponse<List<ManagementRequestResponseDto>>>> GetMyRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<List<ManagementRequestResponseDto>>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var result = await _locationManagementService.GetMyRequestsAsync(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [AuthorizeLocationManager]
        [HttpPost("send-request")]
        public async Task<ActionResult<ApiResponse<ManagementRequestResponseDto>>> SendManageRequest(ManagementRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<ManagementRequestResponseDto>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var user = await _context.Users
                .Include(u => u.LocationManagerProfile)
                .FirstOrDefaultAsync(u => u.Id == currentUserId.Value);

            if (user?.LocationManagerProfile == null)
            {
                return Unauthorized(
                    ApiResponse<ManagementRequestResponseDto>.FailureResponse(
                        "No manager profile found.",
                        "لا يوجد ملف مدير مرتبط بهذا المستخدم."
                    ));
            }

            var managerProfileId = user.LocationManagerProfile.Id;

            var response = await _locationManagementService
                .SendManageRequestAsync(managerProfileId, dto);

            if (!response.Success)
                return BadRequest(response);

            return Ok(response);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue("UserId");
            if (string.IsNullOrEmpty(userIdClaim))
                return null;
            return int.Parse(userIdClaim);
        }
    }
}