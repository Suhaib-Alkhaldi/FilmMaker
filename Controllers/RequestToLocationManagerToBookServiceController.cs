using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.RequestToLocationManagerToBookService;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class RequestToLocationManagerToBookServiceController : ControllerBase
    {
        private readonly IRequestToLocationManagerToBookServiceService _service;
        private readonly ILogger<RequestToLocationManagerToBookServiceController> _logger;

        public RequestToLocationManagerToBookServiceController(
            IRequestToLocationManagerToBookServiceService service,
            ILogger<RequestToLocationManagerToBookServiceController> logger)
        {
            _service = service;
            _logger = logger;
        }


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
        [AuthorizeProductionCompany]
        public async Task<IActionResult> CreateServiceRequestToLocationManager(
            [FromBody] CreateRequestToLocationManagerToBookServiceDTO request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.CreateServiceRequestToLocationManager(request, currentUserId);
            return Ok(result);
        }



        [HttpGet("{id:int}/is-deleted")]
        [AuthorizeProductionCompany]

        public async Task<IActionResult> ReadServiceRequestToLocationManagerByIdWhereDeleted(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.ReadServiceRequestToLocationManager(currentUserId, true);

            return Ok(result);
        }


        [HttpGet("{id:int}")]
        [AuthorizeProductionCompanyOrLocationManager]
        public async Task<IActionResult> ReadServiceRequestToLocationManagerById(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.ReadServiceRequestToLocationManagerById(currentUserId, false,id);

            return Ok(result);
        }

        [HttpGet]
        [AuthorizeProductionCompanyOrLocationManager]

        public async Task<IActionResult> ReadServiceRequestToLocationManager()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.ReadServiceRequestToLocationManager(currentUserId, false);

            return Ok(result);
        }

        [HttpGet("is-deleted")]
        [AuthorizeProductionCompany]
        public async Task<IActionResult> ReadServiceRequestToLocationManagerWhereDeleted()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.ReadServiceRequestToLocationManager(currentUserId, true);

            return Ok(result); 
        }

        [HttpGet("by-location-booking/{locationBookingId:int}")]
        [AuthorizeProductionCompanyOrLocationManager]

        public async Task<IActionResult> ReadServiceRequestToLocationManagerByLocationBookingId(
            int locationBookingId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.ReadServiceRequestToLocationManagerByLocationBookingId(
                currentUserId, locationBookingId, false);
            return Ok(result);
        }


        [HttpGet("by-location-booking/{locationBookingId:int}/is-deleted")]
        [AuthorizeProductionCompany]

        public async Task<IActionResult> ReadServiceRequestToLocationManagerByLocationBookingIdWhereDeleted(
            int locationBookingId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.ReadServiceRequestToLocationManagerByLocationBookingId(
                currentUserId, locationBookingId, true);
            return Ok(result);
        }


        [HttpPut]
        [AuthorizeProductionCompany]

        public async Task<IActionResult> UpdateServiceRequestToLocationManager(
            [FromBody] UpdateRequestToLocationManagerToBookServiceDTO request)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.UpdateServiceRequestToLocationManager(request, currentUserId);
            return Ok(result);
        }


        [HttpDelete("{id:int}")]
        [AuthorizeProductionCompany]

        public async Task<IActionResult> DeleteServiceRequestToLocationManager(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.DeleteServiceRequestToLocationManager(currentUserId, id);
            return Ok(result);
        }

        [HttpPatch("{id:int}/restore")]
        [AuthorizeProductionCompany]

        public async Task<IActionResult> RestoreDeletedServiceRequestToLocationManager(int id)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.RestoreDeletedServiceRequestToLocationManager(currentUserId, id);
            return Ok(result);
        }
    }
}