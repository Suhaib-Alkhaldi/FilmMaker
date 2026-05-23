using FilmMaker.Attribute;
using FilmMaker.DTO.ServiceProvider;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ServicesProvidedController : ControllerBase
    {
        private readonly IServicesProvidedService _servicesProvidedService;
        private readonly ILogger<ServicesProvidedController> _logger;

        public ServicesProvidedController(IServicesProvidedService servicesProvidedService, ILogger<ServicesProvidedController> logger)
        {
            _servicesProvidedService = servicesProvidedService;
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
        [AuthorizeServiceProvider]
        public async Task<IActionResult> AddService([FromBody] CreateServiceDTO serviceDto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.AddService(serviceDto, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        
        [HttpPut]
        [AuthorizeServiceProvider]

        public async Task<IActionResult> UpdateService([FromBody] UpdateServiceDTO serviceDto)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.UpdateService(serviceDto, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

      
        [HttpDelete("{serviceId:int}")]
        [AuthorizeServiceProvider]

        public async Task<IActionResult> DeleteService(int serviceId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.DeleteService(serviceId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("restore/{serviceId:int}")]
        [AuthorizeServiceProvider]

        public async Task<IActionResult> RestoreDeleteService(int serviceId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.RestoreDeletedService(serviceId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpPatch("activate")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> SetServiceActive(int ServiceId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.SetServiceActive(ServiceId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpPatch("deactivate")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> SetServiceInactive(int ServiceId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.SetServiceInactive(ServiceId, currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        // ── Read Endpoints ────────────────────────────────────────────────────────

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServices()
        {
            var result = await _servicesProvidedService.GetAllServices();
            return result.Success ? Ok(result) : BadRequest(result);
        }


        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServicesByName([FromQuery] string searchTerm)
        {
            var result = await _servicesProvidedService.SearchServices(searchTerm );
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("{serviceId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceById(int serviceId)
        {
            var result = await _servicesProvidedService.GetServiceById(serviceId);
            if (!result.Success) return NotFound(result);
            return Ok(result);
        }

        [HttpGet("my-services")]
        [AuthorizeServiceProvider]

        public async Task<IActionResult> GetMyServices()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.GetMyServicesByProvider(currentUserId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("my-services/deleted")]
        [AuthorizeServiceProvider]

        public async Task<IActionResult> GetMyDeletedServices()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _servicesProvidedService.GetMyServicesByProvider(currentUserId, includeDeleted: true);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-provider/{providerId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicesByProvider(int providerId)
        {
            var result = await _servicesProvidedService.GetServicesByProvider(providerId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-type/{serviceTypeId:int}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicesByServiceType(int serviceTypeId)
        {
            var result = await _servicesProvidedService.GetServicesByServiceType(serviceTypeId);
            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("by-type/{serviceTypeId:int}/search")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicesByServiceTypeAndByName(int serviceTypeId, [FromQuery] string searchTerm)
        {
            var result = await _servicesProvidedService.SearchServicesByServiceType(serviceTypeId, searchTerm);
            return result.Success ? Ok(result) : BadRequest(result);
        }
    }
}