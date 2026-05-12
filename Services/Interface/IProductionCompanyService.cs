using FilmMaker.Common;
using FilmMaker.DTOs.ProductionCompany;

namespace FilmMaker.Services.Interfaces
{
    public interface IProductionCompanyService
    {
        Task<ApiResponse<RegisterProductionCompanyResponse>> RegisterAsync(RegisterProductionCompanyRequest request);
        Task<ApiResponse<LoginResponse>>LoginAsync(LoginRequest request);
        Task<ApiResponse<ProductionCompanyProfileResponse>>GetProfileAsync(int userId);
        Task<ApiResponse<UpdateProductionCompanyProfileResponse>> UpdateProfileAsync(int userId, UpdateProductionCompanyProfileRequest request);
        Task<ApiResponse<ChangePasswordResponse>>ChangePasswordAsync(int userId, ChangePasswordRequest request);
    }
}