using FilmMaker.Common;
using FilmMaker.DTO.LocationManager;

namespace FilmMaker.Services.Interface
{
    public interface ILocationManagementServicee
    {
        Task<ApiResponse<List<LocationSummaryDto>>> GetLocationsAsync(LocationFilterDto dto);
        Task<ApiResponse<LocationSummaryDto>> GetLocationByIdAsync(int locationId);
        Task<ApiResponse<List<LocationSummaryDto>>> GetManagedLocationsAsync(int managerId);
        Task<ApiResponse<List<ManagementRequestResponseDto>>> GetMyRequestsAsync(int userId);
        Task<ApiResponse<ManagementRequestResponseDto>> SendManageRequestAsync(int managerProfileId, ManagementRequestDto dto);
    }
}
