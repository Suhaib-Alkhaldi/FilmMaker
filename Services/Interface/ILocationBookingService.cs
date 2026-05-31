using FilmMaker.Common;
using FilmMaker.DTO.LocationBooking;
using FilmMaker.DTO.LocationManager;

namespace FilmMaker.Services.Interface
{
    public interface ILocationBookingService
    {
        Task<ApiResponse<LocationOwnerBookingRequestResponseDto>> CreateBookingRequestAsync(int productionCompanyProfileId, CreateBookingRequestDto dto);
        Task<ApiResponse<List<LocationOwnerBookingRequestResponseDto>>> GetBookingRequestsAsync(int managerProfileId);
        Task<ApiResponse<LocationOwnerBookingRequestResponseDto>> GetBookingRequestByIdAsync(int requestId, int managerProfileId);
        Task<ApiResponse<LocationOwnerBookingRequestResponseDto>> UpdateBookingRequestAsync(int requestId, int managerProfileId, UpdateBookingRequestDto dto);
        Task<ApiResponse<bool>> CancelBookingRequestAsync(int requestId, int managerProfileId);
    }
}
