using FilmMaker.Common;
using FilmMaker.DTO.LocationScouting.Request;
using FilmMaker.DTO.LocationScouting.Response;
namespace FilmMaker.Services.Interface
{
    public interface ILocationScoutingRequestService
    {
        Task<ApiResponse<LocationScoutingRequestResponseDto>> CreateLocationScoutingRequest(CreateLocationScoutingRequestDto dto,int currentUserId);
        Task<ApiResponse<LocationScoutingRequestResponseDto>> UpdateLocationScoutingRequest(UpdateLocationScoutingRequestDto dto,int currentUserId);

        Task<ApiResponse<LocationScoutingRequestResponseDto>> GetLocationScoutingRequestById(int requestId,int currentUserId);

        Task<ApiResponse<List<LocationScoutingRequestResponseDto>>> GetMySentLocationScoutingRequests(int currentUserId);

        Task<ApiResponse<List<LocationScoutingRequestResponseDto>>> GetMyReceivedLocationScoutingRequests(int currentUserId);

        Task<ApiResponse<LocationScoutingRequestResponseDto>> RespondToLocationScoutingRequest(RespondLocationScoutingRequestDto dto,int currentUserId);

        Task<ApiResponse<bool>> CancelLocationScoutingRequest(int requestId,int currentUserId);

        Task<ApiResponse<LocationScoutingRequestResponseDto>> GetManagerLocationScoutingRequestById(int requestId,int currentUserId);
    }
}
