using FilmMaker.Attribute;
using FilmMaker.DTO.ServiceBooking;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServiceBookingController : ControllerBase
    {
        private readonly IServiceBookingService _serviceBookingService;
        private readonly ILogger<ServiceBookingController> _logger;

        public ServiceBookingController(
            IServiceBookingService serviceBookingService,
            ILogger<ServiceBookingController> logger)
        {
            _serviceBookingService = serviceBookingService;
            _logger = logger;
        }

        // ── Helper ────────────────────────────────────────────────────────────────

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return 0;
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return 0;
            }

            return userId;
        }


        [HttpPost]
        [Authorize(Roles = "Location Manager,Production Company")]
        public async Task<IActionResult> CreateBookingRequest([FromBody] CreateServiceBookingDTO dto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" ,Success = false});

            var result = await _serviceBookingService.CreateBookingRequest(dto, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{bookingId:int}/accept")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> AcceptBooking(int bookingId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح", Success = false });

            var result = await _serviceBookingService.AcceptBooking(bookingId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

       
        [HttpPatch("{bookingId:int}/reject")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> RejectBooking(
            int bookingId
            )
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح", Success = false });

            var result = await _serviceBookingService.RejectBooking(
                bookingId, "",currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{bookingId:int}/cancel")]
        [Authorize(Roles = "Location Manager,Production Company,Service Provider")]
        public async Task<IActionResult> CancelBooking(int bookingId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح", Success = false });

            var result = await _serviceBookingService.CancelBooking(bookingId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{bookingId:int}/complete")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> CompleteBooking(int bookingId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح", Success = false });

            var result = await _serviceBookingService.CompleteBooking(bookingId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{bookingId:int}/extend")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> ExtendBooking(
            int bookingId,
            int days)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح", Success = false });

            var result = await _serviceBookingService.ExtendBooking(bookingId, currentUserId, days);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("{bookingId:int}/status")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> UpdateBookingStatus(
            int bookingId,
            int statusId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح", Success = false });

            var result = await _serviceBookingService.UpdateBookingStatus(
                bookingId, statusId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }


       
        [HttpGet("{bookingId:int}")]
        public async Task<IActionResult> GetBookingById(int bookingId)
        {
            var result = await _serviceBookingService.GetBookingById(bookingId);
            return result.Success ? Ok(result) : NotFound(result);
        }

        [HttpGet("provider/all")]
        public async Task<IActionResult> GetProviderBookings()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _serviceBookingService.GetProviderBookings(currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("provider/all/by-date")]
        public async Task<IActionResult> GetProviderBookingsByDate(DateTime start,DateTime end)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _serviceBookingService.GetProviderBookingsByDate(start, end,currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet("provider/received")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> GetReceivedBookings()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _serviceBookingService.GetReceivedBookings(currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my-bookings")]
        public async Task<IActionResult> GetServicesIBooked()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _serviceBookingService.GetServicesIBooked(currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-location/{locationId:int}")]
        public async Task<IActionResult> GetBookingsByLocation(int locationId)
        {
            var result = await _serviceBookingService.GetBookingsByLocation(locationId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        //[HttpGet("by-project/{projectId:int}")]
        //public async Task<IActionResult> GetBookingsByProject(int projectId)
        //{
        //    var result = await _serviceBookingService.GetBookingsByProject(projectId);
        //    return result.Success ? Ok(result) : BadRequest(result);
        //}
    }
}