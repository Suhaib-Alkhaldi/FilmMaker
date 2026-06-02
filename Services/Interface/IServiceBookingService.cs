using FilmMaker.Common;
using FilmMaker.DTO.ServiceBooking;

namespace FilmMaker.Services.Interface
{
    public interface IServiceBookingService
    {
        Task<ApiResponse<CreateServiceBookingDTO>> CreateBookingRequest(
          CreateServiceBookingDTO dto,
          int currentUserId, bool isLocationManager);

        
        Task<ApiResponse<GetServiceBookingDTO>> GetBookingById(
            int bookingId);

        Task<ApiResponse<List<GetServiceBookingDTO>>> GetProviderBookings(
            int currentUserId);
        Task<ApiResponse<List<GetServiceBookingDTO>>> GetProviderBookingsByDate(
        DateTime startDate,
        DateTime endDate,
        int currentUserId);

        Task<ApiResponse<List<GetServiceBookingDTO>>> GetReceivedBookings(
            int currentUserId);

        Task<ApiResponse<List<GetServiceBookingDTO>>> GetServicesIBooked(
            int currentUserId);

        // Accept booking request
        Task<ApiResponse<bool>> AcceptBooking(
            int bookingId,
            int currentUserId);

        // Reject booking request
        Task<ApiResponse<bool>> RejectBooking(
            int bookingId,
            string? rejectionReason,
            int currentUserId);

        // Cancel booking
        Task<ApiResponse<bool>> CancelBooking(
            int bookingId,
            int currentUserId);


        //extend booking
        Task<ApiResponse<bool>> ExtendBooking(
        int bookingId,
        int currentUserId,int days);

        // Change booking status manually
        Task<ApiResponse<bool>> UpdateBookingStatus(
            int bookingId,
            int bookingStatusId,
            int currentUserId);
        Task<ApiResponse<bool>> CompleteBooking(
          int bookingId,
          int currentUserId);
        
        Task<ApiResponse<List<GetServiceBookingDTO>>> GetBookingsByProject(
            int projectId);

   
        // Get bookings by location
        Task<ApiResponse<List<GetServiceBookingDTO>>> GetBookingsByLocation(
            int locationId);
    }
}
