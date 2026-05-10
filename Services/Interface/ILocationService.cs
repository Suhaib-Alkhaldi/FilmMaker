using FilmMaker.Common;
using FilmMaker.DTO.Location;

namespace FilmMaker.Services.Interface
{
    public interface ILocationService
    {
        Task<ApiResponse<LocationDTO>> CreateLocation(LocationDTO location, int currentUserId);
        Task<ApiResponse<LocationDTO>> UpdateLocation(LocationDTO location, int currentUserId);
        Task<ApiResponse<GetLocationDTO>> GetLocationById(int locationId);
        Task<ApiResponse<List<GetLocationDTO>>> GetAllLocations();
        Task<ApiResponse<List<GetLocationDTO>>> GetAllUnArchivedLocations();
        Task<ApiResponse<List<GetLocationDTO>>> GetLocationsByOwnerId(int currentUserId);
        Task<ApiResponse<List<GetLocationDTO>>> GetOwnerArchivedLocations(int currentUserId);
        Task<ApiResponse<List<GetLocationDTO>>> GetOwnerActiveLocations(int currentUserId);


    }
}
