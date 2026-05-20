using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.Auth.Request;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(IAuthService authService, ITokenService tokenService)
        {
            _tokenService = tokenService;
            _authService = authService;
        }
        private string? GetCurrentUserName()
        {
            var userNameClaim = User.FindFirst("FirstName")?.Value;

            if (string.IsNullOrWhiteSpace(userNameClaim))
            {
                return null;
            }

            return userNameClaim;
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

        [HttpPost("register-location-owner")]
        public async Task<IActionResult> RegisterLocationOwner(RegisterLocationOwnerRequestDto request)
        {
            var result = await _authService.RegisterLocationOwner(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("register-location-manager")]
        public async Task<IActionResult> RegisterLocationManager(RegisterLocationManagerRequestDto request)
        {
            var result = await _authService.RegisterLocationManager(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("register-production-company")]
        public async Task<IActionResult> RegisterProductionCompany(RegisterProductionCompanyRequestDto request)
        {
            var result = await _authService.RegisterProductionCompany(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }
        [HttpPost("register-service-provider")]
        public async Task<IActionResult> RegisterServiceProvider(RegisterServiceProviderRequestDto request)
        {
            var result = await _authService.RegisterServiceProvider(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("Login")]
        public async Task<IActionResult> Login(LoginRequestDto request)
        {

            if (User.Identity?.IsAuthenticated == true)
            {
                var userName = GetCurrentUserName();

                ApiResponse<object> response = new ApiResponse<object>
                {
                    Success = false,
                    MessageEn = $"User  {userName} is already logged in.",
                    MessageAr = $" مسجل الدخول في النظام مسبقاً {userName} المستخدم",
                    Data = null
                };

                return Conflict(response);

            }

            var result = await _authService.Login(request);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [HttpPost("Logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshRequest request)
        {
            await _tokenService.RevokeRefreshTokenAsync(request.RefreshToken);

            return Ok(new ApiResponse<object>
            {
                Success = true,
                MessageEn = "Logged out successfully.",
                MessageAr = "تم تسجيل الخروج بنجاح.",
                Data = null
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequest request)
        {
            var result = await _authService.RefreshToken(request.RefreshToken);
            if (!result.Success)
                return BadRequest(result);
            return Ok(result);

        }

        [HttpPost("verify-email")]
        [Authorize]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyOtpRequest request)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    MessageEn = "Unauthorized.",
                    MessageAr = "غير مصرح.",
                    Data = null
                });

            var result = await _authService.VerifyEmail(request, GetCurrentUserId()!.Value);

            return Ok(result);
        }
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            var result = await _authService.ForgotPassword(request);

            return Ok(result);

        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            var result = await _authService.ResetPassword(request);
            return Ok(result);
        }

        [HttpPost("send-verification")]
        [Authorize]  
        public async Task<IActionResult> SendVerificationOtp()
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized(new ApiResponse<object>
                {
                    Success = false,
                    MessageEn = "Unauthorized.",
                    MessageAr = "غير مصرح.",
                    Data = null
                });
            var result = await _authService.SendVerificationOtp(currentUserId.Value);

            return Ok(result);
        }
    }
}
