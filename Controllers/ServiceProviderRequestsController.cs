using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.ServiceProviderBooking;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceProviderRequestsController : ControllerBase
    {
        private readonly IServiceProviderRequestService _serviceProviderRequestService;

        public ServiceProviderRequestsController(
            IServiceProviderRequestService serviceProviderRequestService)
        {
            _serviceProviderRequestService = serviceProviderRequestService;
        }


        [HttpGet("GetMySentServiceProviderRequests")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> GetMySentServiceProviderRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<List<GetServiceProviderRequestDTO>>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService.GetMySentServiceProviderRequests(
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("GetMySentServiceProviderRequestById")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> GetMySentServiceProviderRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService.GetMySentServiceProviderRequestById(
                requestId,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }



        [HttpGet("GetMyReceivedServiceProviderRequests")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> GetMyReceivedServiceProviderRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<List<GetServiceProviderRequestDTO>>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService
                .GetMyReceivedServiceProviderRequests(currentUserId.Value);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("GetMyReceivedServiceProviderRequestById")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> GetMyReceivedServiceProviderRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService
                .GetMyReceivedServiceProviderRequestById(
                    requestId,
                    currentUserId.Value
                );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPost("SendServiceRequestToProvider")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> SendServiceRequestToProvider([FromBody] SendServiceRequestToProviderDTO request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService.SendServiceRequestToProvider(
                request,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("UpdateServiceProviderRequest")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> UpdateServiceProviderRequest([FromBody] UpdateServiceProviderRequestDTO request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService.UpdateServiceProviderRequest(
                request,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        

        [HttpPut("CancelServiceProviderRequest")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> CancelServiceProviderRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<bool>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService.CancelServiceProviderRequest(
                requestId,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }
        
        [HttpPut("RespondToServiceProviderRequest")]
        [AuthorizeServiceProvider]
        public async Task<IActionResult> RespondToServiceProviderRequest([FromBody] RespondServiceProviderRequestDTO request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _serviceProviderRequestService
                .RespondToServiceProviderRequest(
                    request,
                    currentUserId.Value
                );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        private int? GetCurrentUserId()
        {
            var userIdValue =
                User.FindFirst("UserId")?.Value ??
                User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            return int.TryParse(userIdValue, out var userId)
                ? userId
                : null;
        }
    }
}
