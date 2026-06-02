using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.LocationBooking;
using FilmMaker.DTO.LocationManager;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
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


        [Authorize(Roles = "Location Manager , Production Company")]
        [HttpPost("create-booking-request")]
        public async Task<ActionResult<ApiResponse<BookingRequestDto>>> CreateBookingRequest([FromBody]CreateBookingRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var response = await _locationBookingService.CreateBookingRequestAsync(currentUserId.Value, dto);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }


        [Authorize(Roles = "Location Manager , Production Company")]
        [HttpGet("my-booking-requests")]
        public async Task<ActionResult<ApiResponse<List<BookingRequestDto>>>> GetMyBookingRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var response = await _locationBookingService.GetMyBookingRequestsAsync(currentUserId.Value);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize(Roles = "Location Manager , Production Company")]
        [HttpGet("my-booking-requests/{requestId}")]
        public async Task<ActionResult<ApiResponse<BookingRequestDto>>> GetBookingRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var response = await _locationBookingService.GetBookingRequestByIdAsync(requestId, currentUserId.Value);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize(Roles = "Location Manager , Production Company")]
        [HttpGet("GetLocationBookingCalendar")]
        public async Task<ActionResult<ApiResponse<List<LocationBookingCalendarDayDto>>>> GetLocationBookingCalendar(int locationId , [FromQuery] DateTime? fromDate , [FromQuery] DateTime? toDate)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationBookingService.GetLocationBookingCalendarAsync(currentUserId.Value ,locationId, fromDate,toDate);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [Authorize(Roles = "Location Manager , Production Company")]
        [HttpPut("update-booking-request")]
        public async Task<ActionResult<ApiResponse<BookingRequestDto>>> UpdateBookingRequest(UpdateBookingRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();


            var response = await _locationBookingService.UpdateBookingRequestAsync(currentUserId.Value, dto);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }

        [Authorize(Roles ="Location Manager , Production Company")]
        [HttpDelete("cancel-booking-request/{requestId}")]
        public async Task<ActionResult<ApiResponse<bool>>> CancelBookingRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var response = await _locationBookingService.CancelBookingRequestAsync(requestId, currentUserId.Value);
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