using FilmMaker.Common;
using FilmMaker.DTO.Booking;

namespace FilmMaker.Services.Interface
{
    public interface ILocationOwnerBookingRequestService
    {
        Task<ApiResponse<BookingRequestResponseDto>> RespondBookingRequest(RespondBookingRequestDto request,int currentUserId);
        Task<ApiResponse<List<BookingRequestResponseDto>>> GetReceivedBookingRequests(int currentUserId);
        Task<ApiResponse<BookingRequestResponseDto>> GetReceivedBookingRequestById(int bookingRequestId,int currentUserId);
    }
}
