using FilmMaker.Common;
using FilmMaker.DTO.Visit;

namespace FilmMaker.Services.Interface
{
    public interface ILocationOwnerVisitRequestService
    {
        Task<ApiResponse<List<LocationVisitRequestResponseDto>>> GetReceivedVisitRequests(int currentUserId);
        Task<ApiResponse<LocationVisitRequestResponseDto>> GetReceivedVisitRequestById(int visitRequestId,int currentUserId);
        Task<ApiResponse<LocationVisitRequestResponseDto>> RespondVisitRequest(RespondLocationVisitRequestDto request,int currentUserId);
    }
}
