using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.LocationBooking;
using FilmMaker.DTO.LocationManager;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [AuthorizeLocationManager]
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly ILocationBookingService _locationBookingService;
        private readonly FilmMakerDbContext _context;

        public BookingController(
            ILocationBookingService locationBookingService,
            FilmMakerDbContext context)
        {
            _locationBookingService = locationBookingService;
            _context = context;
        }

        [HttpPost("create-booking-request")]
        public async Task<ActionResult<ApiResponse<LocationOwnerBookingRequestResponseDto>>> CreateBookingRequest(CreateBookingRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<LocationOwnerBookingRequestResponseDto>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var response = await _locationBookingService.CreateBookingRequestAsync(currentUserId.Value, dto);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("my-booking-requests")]
        public async Task<ActionResult<ApiResponse<List<LocationOwnerBookingRequestResponseDto>>>> GetMyBookingRequests()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<List<LocationOwnerBookingRequestResponseDto>>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var response = await _locationBookingService.GetBookingRequestsAsync(currentUserId.Value);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpGet("my-booking-requests/{requestId}")]
        public async Task<ActionResult<ApiResponse<LocationOwnerBookingRequestResponseDto>>> GetBookingRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<LocationOwnerBookingRequestResponseDto>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var response = await _locationBookingService.GetBookingRequestByIdAsync(requestId, currentUserId.Value);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpPut("update-booking-request/{requestId}")]
        public async Task<ActionResult<ApiResponse<LocationOwnerBookingRequestResponseDto>>> UpdateBookingRequest(int requestId, UpdateBookingRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<LocationOwnerBookingRequestResponseDto>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));
            var response = await _locationBookingService.UpdateBookingRequestAsync(requestId, currentUserId.Value, dto);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [HttpDelete("cancel-booking-request/{requestId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelBookingRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == null)
                return Unauthorized(
                    ApiResponse<bool>.FailureResponse(
                        "Unauthorized.",
                        "غير مصرح."
                    ));

            var response = await _locationBookingService.CancelBookingRequestAsync(requestId, currentUserId.Value);
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