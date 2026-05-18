using FilmMaker.Common;
using FilmMaker.DTO.LocationBooking;
using FilmMaker.DTO.LocationManager;

namespace FilmMaker.Services.Interface
{
    public interface ILocationBookingService
    {
        Task<ApiResponse<BookingRequestResponseDto>> CreateBookingRequestAsync(int productionCompanyProfileId, CreateBookingRequestDto dto);
        Task<ApiResponse<List<BookingRequestResponseDto>>> GetBookingRequestsAsync(int managerProfileId);
        Task<ApiResponse<BookingRequestResponseDto>> GetBookingRequestByIdAsync(int requestId, int managerProfileId);
        Task<ApiResponse<BookingRequestResponseDto>> UpdateBookingRequestAsync(int requestId, int managerProfileId, UpdateBookingRequestDto dto);
        Task<ApiResponse<bool>> CancelBookingRequestAsync(int requestId, int managerProfileId);
    }
}
