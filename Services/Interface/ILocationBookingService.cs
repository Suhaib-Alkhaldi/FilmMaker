using FilmMaker.Common;
using FilmMaker.DTO.LocationBooking;
using FilmMaker.DTO.LocationManager;

namespace FilmMaker.Services.Interface
{
    public interface ILocationBookingService
    {
        Task<ApiResponse<BookingRequestDto>> CreateBookingRequestAsync(int productionCompanyProfileId, CreateBookingRequestDto dto);
        Task<ApiResponse<List<BookingRequestDto>>> GetMyBookingRequestsAsync(int currentUserId);
        Task<ApiResponse<BookingRequestDto>> GetBookingRequestByIdAsync(int requestId, int managerProfileId);
        Task<ApiResponse<BookingRequestDto>> UpdateBookingRequestAsync(int currentUserId, UpdateBookingRequestDto dto);
        Task<ApiResponse<List<LocationBookingCalendarDayDto>>> GetLocationBookingCalendarAsync(int locationId, DateTime fromDate, DateTime toDate);
        Task<ApiResponse<bool>> CancelBookingRequestAsync(int requestId, int managerProfileId);
    }
}
