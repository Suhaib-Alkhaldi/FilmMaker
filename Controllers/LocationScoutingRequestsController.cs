using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.LocationScouting.Request;
using FilmMaker.DTO.LocationScouting.Response;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LocationScoutingRequestsController : ControllerBase
    {
        private readonly ILocationScoutingRequestService _locationScoutingRequestService;

        public LocationScoutingRequestsController(
            ILocationScoutingRequestService locationScoutingRequestService)
        {
            _locationScoutingRequestService = locationScoutingRequestService;
        }

        [HttpPost("CreateLocationScoutingRequest")]
        [AuthorizeProductionCompany]
        public async Task<IActionResult> CreateLocationScoutingRequest([FromBody] CreateLocationScoutingRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.CreateLocationScoutingRequest(
                dto,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("UpdateLocationScoutingRequest")]
        [AuthorizeProductionCompany]
        public async Task<IActionResult> UpdateLocationScoutingRequest([FromBody] UpdateLocationScoutingRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.UpdateLocationScoutingRequest(
                dto,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("GetLocationScoutingRequestById")]
        [Authorize(Roles = "Production Company,Location Manager")]
        public async Task<IActionResult> GetLocationScoutingRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.GetLocationScoutingRequestById(
                requestId,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("my-sent")]
        [AuthorizeProductionCompany]
        public async Task<IActionResult> GetMySentLocationScoutingRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<List<LocationScoutingRequestResponseDto>>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.GetMySentLocationScoutingRequests(
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("GetMyReceivedLocationScoutingRequests")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> GetMyReceivedLocationScoutingRequests()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<List<LocationScoutingRequestResponseDto>>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.GetMyReceivedLocationScoutingRequests(
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("RespondToLocationScoutingRequest")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> RespondToLocationScoutingRequest([FromBody] RespondLocationScoutingRequestDto dto)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.RespondToLocationScoutingRequest(
                dto,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("CancelLocationScoutingRequest")]
        [AuthorizeProductionCompany]
        public async Task<IActionResult> CancelLocationScoutingRequest(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<bool>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.CancelLocationScoutingRequest(
                requestId,
                currentUserId.Value
            );

            return response.Success ? Ok(response) : BadRequest(response);
        }


        [HttpGet("GetManagerLocationScoutingRequestById")]
        [AuthorizeLocationManager]
        public async Task<IActionResult> GetManagerLocationScoutingRequestById(int requestId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
            {
                return Unauthorized(ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Invalid token.",
                    "رمز الدخول غير صالح."
                ));
            }

            var response = await _locationScoutingRequestService.GetManagerLocationScoutingRequestById(
                requestId,
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
