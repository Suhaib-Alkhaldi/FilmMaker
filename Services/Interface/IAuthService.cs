using FilmMaker.Common;
using FilmMaker.DTO.Auth.Request;
using FilmMaker.DTO.Auth.Response;

namespace FilmMaker.Services.Interface
{
    public interface IAuthService
    {
        Task<ApiResponse<RegisterResponseDto>> RegisterLocationOwner(RegisterLocationOwnerRequestDto request);

        Task<ApiResponse<RegisterResponseDto>> RegisterLocationManager(RegisterLocationManagerRequestDto request);
        Task<ApiResponse<RegisterResponseDto>> RegisterProductionCompany(RegisterProductionCompanyRequestDto request);
        Task<ApiResponse<RegisterResponseDto>> RegisterServiceProvider(RegisterServiceProviderRequestDto request);
        Task<ApiResponse<LoginResponseDTO>> Login(LoginRequestDto request );

        Task<ApiResponse<object>> RefreshToken(string refreshToken);

        Task<ApiResponse<object>> VerifyEmail(VerifyOtpRequest request, int currentUserId);

        Task<ApiResponse<object>> SendVerificationOtp(int currentUserId);

        Task<ApiResponse<object>> ForgotPassword(ForgotPasswordRequest request);

        Task<ApiResponse<object>> ResetPassword(ResetPasswordRequest request);

    }
}
