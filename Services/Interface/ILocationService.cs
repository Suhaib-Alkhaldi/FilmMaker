using FilmMaker.Common;
using FilmMaker.DTO.Location.Request;
using FilmMaker.DTO.Location.Response;

namespace FilmMaker.Services.Interface
{
    public interface ILocationService
    {
        Task<ApiResponse<LocationResponseDto>> CreateLocation(CreateLocationRequestDto request,int currentUserId);
        Task<ApiResponse<LocationResponseDto>> UpdateLocation(UpdateLocationRequestDto request,int currentUserId);
        Task<ApiResponse<List<LocationResponseDto>>> GetMyActiveLocations(int currentUserId);
        Task<ApiResponse<List<ArchivedLocationResponseDto>>> GetMyArchivedLocations(int currentUserId);
        Task<ApiResponse<LocationResponseDto>> GetMyLocationById(int locationId,int currentUserId);
        Task<ApiResponse<bool>> ArchiveLocation(ArchiveLocationRequestDto request, int currentUserId);
        Task<ApiResponse<bool>> RestoreArchivedLocation(int locationId,int currentUserId);
        Task<ApiResponse<List<LocationResponseDto>>> GetAllActiveLocations();
        Task<ApiResponse<List<ArchivedLocationResponseDto>>> GetAllArchivedLocations();
    }
}
