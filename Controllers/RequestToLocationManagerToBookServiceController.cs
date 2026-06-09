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

        



        


        
        [HttpGet("GetMySentServiceRequestsToLocationManager")]
        [AuthorizeProductionCompanyOrLocationManager]

        public async Task<IActionResult> GetMySentServiceRequestsToLocationManager()
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.GetMySentServiceRequestsToLocationManager(currentUserId);

            return Ok(result);
        }

        

        [HttpGet("GetMyServiceRequestToLocationManagerById")]
        [AuthorizeProductionCompanyOrLocationManager]

        public async Task<IActionResult> GetMyServiceRequestToLocationManagerById(int requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.GetMyServiceRequestToLocationManagerById(requestId,currentUserId);
            return Ok(result);
        }

        [HttpGet("GetMyReceivedServiceRequests")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> GetMyReceivedServiceRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == 0)
            {
                return Unauthorized(ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _service.GetMyReceivedServiceRequestsToLocationManager(currentUserId);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("GetReceivedServiceRequestById")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> GetReceivedServiceRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == 0)
            {
                return Unauthorized(ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _service.GetReceivedServiceRequestToLocationManagerById(requestId,currentUserId);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("CreateServiceRequestToLocationManager")]
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

        [HttpPost("RespondToServiceRequest")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> RespondToServiceRequest(RespondRequestToLocationManagerToBookServiceDTO request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == 0)
            {
                return Unauthorized(ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _service.RespondToServiceRequestToLocationManager(request,currentUserId);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("UpdateServiceRequestToLocationManager")]
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

        [HttpPut("CancelServiceRequestToLocationManager")]
        [AuthorizeProductionCompany]

        public async Task<IActionResult> CancelServiceRequestToLocationManager(int requestId)
        {
            var currentUserId = GetCurrentUserId();
            if (currentUserId == 0)
                return Unauthorized(new ApiResponse<bool> { MessageEn = "Invalid token", MessageAr = "رمز غير صالح" });

            var result = await _service.CancelServiceRequestToLocationManager(requestId, currentUserId);

            return Ok(result);
        }



    }
}