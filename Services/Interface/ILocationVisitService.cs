using FilmMaker.Common;
using FilmMaker.DTO.LocationVisit;

namespace FilmMaker.Services.Interface
{
    public interface ILocationVisitService
    {
        Task<ApiResponse<VisitRequestResponseDto>> CreateVisitRequestAsync(int managerProfileId, CreateVisitRequestDto dto);
        Task<ApiResponse<List<VisitRequestResponseDto>>> GetVisitRequestsAsync(int managerProfileId);
        Task<ApiResponse<VisitRequestResponseDto>> GetVisitRequestByIdAsync(int requestId, int managerProfileId);
        Task<ApiResponse<VisitRequestResponseDto>> UpdateVisitRequestAsync(int managerProfileId, UpdateVisitRequestDto dto);
        Task<ApiResponse<bool>> CancelVisitRequestAsync(int requestId, int managerProfileId);
    }
}
