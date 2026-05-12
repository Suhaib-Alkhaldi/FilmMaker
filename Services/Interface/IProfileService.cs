using FilmMaker.Common;
using FilmMaker.DTO.Profile.Request;
using FilmMaker.DTO.Profile.Response;

namespace FilmMaker.Services.Interface
{
    public interface IProfileService
    {
        Task<ApiResponse<LocationManagerProfileResponseDto>> GetMyLocationManagerProfile(int currentUserId);

        Task<ApiResponse<LocationManagerProfileResponseDto>> UpdateLocationManagerProfile(
            UpdateLocationManagerProfileRequestDto request,int currentUserId);

        Task<ApiResponse<LocationManagerProfileResponseDto>> CompleteLocationManagerProfile(
           CompleteLocationManagerProfileRequestDto request,int currentUserId);

        Task<ApiResponse<ProductionCompanyProfileResponseDto>> GetMyProductionCompanyProfile(
            int currentUserId
        );

        Task<ApiResponse<ProductionCompanyProfileResponseDto>> UpdateProductionCompanyProfile(
            UpdateProductionCompanyProfileRequestDto request,
            int currentUserId
        );

        Task<ApiResponse<ServiceProviderProfileResponseDto>> GetMyServiceProviderProfile(int currentUserId);

        Task<ApiResponse<ServiceProviderProfileResponseDto>> UpdateServiceProviderProfile(UpdateServiceProviderProfileRequestDto request,int currentUserId);

        Task<ApiResponse<LocationOwnerProfileResponseDto>> GetMyLocationOwnerProfile(int currentUserId);

        Task<ApiResponse<LocationOwnerProfileResponseDto>> UpdateLocationOwnerProfile(
            UpdateLocationOwnerProfileRequestDto request,int currentUserId);

        Task<ApiResponse<bool>> ChangePassword(ChangePasswordRequestDto request,int currentUserId);


    }
}
