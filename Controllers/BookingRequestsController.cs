using FilmMaker.Common;
using FilmMaker.DTO.Booking;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingRequestsController : ControllerBase
    {
        private readonly ILocationOwnerBookingRequestService _locationOwnerBookingRequestService;
        public BookingRequestsController(ILocationOwnerBookingRequestService locationOwnerBookingRequestService)
        {
            _locationOwnerBookingRequestService = locationOwnerBookingRequestService;
        }

        [HttpGet("GetReceivedBookingRequests")]
        public async Task<ActionResult<ApiResponse<List<BookingRequestResponseDto>>>> GetReceivedBookingRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationOwnerBookingRequestService.GetReceivedBookingRequests(currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpGet("GetReceivedBookingRequestById")]
        public async Task<ActionResult<ApiResponse<BookingRequestResponseDto>>> GetReceivedBookingRequestById(int bookingRequestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationOwnerBookingRequestService.GetReceivedBookingRequestById(bookingRequestId,currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPut("RespondBookingRequest")]
        public async Task<ActionResult<ApiResponse<BookingRequestResponseDto>>> RespondBookingRequest([FromBody] RespondBookingRequestDto request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _locationOwnerBookingRequestService.RespondBookingRequest(request,currentUserId.Value);

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
