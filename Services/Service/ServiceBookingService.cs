using FilmMaker.Common;
using FilmMaker.DTO.ServiceBooking;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FilmMaker.Services.Service
{
    public class ServiceBookingService : IServiceBookingService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<ServiceBookingService> _logger;


        private  int StatusPending()
        {
          return _context.LookupItems
                .Where(l => l.LookupCategoryId == 3 && l.Name == "Pending" && !l.IsDeleted)
                .Select(l => l.Id)
                .FirstOrDefault();
        } 
        private  int StatusAccepted()
        {
            return _context.LookupItems
                .Where(l => l.LookupCategoryId == 3 && l.Name == "Accepted" && !l.IsDeleted)
                .Select(l => l.Id)
                .FirstOrDefault();
        }
        private  int StatusRejected ()
        {
            return _context.LookupItems
                .Where(l => l.LookupCategoryId == 3 && l.Name == "Rejected" && !l.IsDeleted)
                .Select(l => l.Id)
                .FirstOrDefault();
        }
        private  int StatusCancelled()
        {
            return _context.LookupItems
                .Where(l => l.LookupCategoryId == 3 && l.Name == "Cancelled" && !l.IsDeleted)
                .Select(l => l.Id)
                .FirstOrDefault();  
        }
        private  int StatusCompleted()
        {
            return _context.LookupItems
          .Where(l => l.LookupCategoryId == 3 && l.Name == "Completed" && !l.IsDeleted)
          .Select(l => l.Id)
          .FirstOrDefault();
        }

        public ServiceBookingService(FilmMakerDbContext context, ILogger<ServiceBookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        private async Task<int?> GetServiceProviderIdAsync(int userId)
        {
            var profile = await _context.ServiceProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            return profile?.Id;
        }

        private static GetServiceBookingDTO MapToDTO(ServiceBooking booking) => new()
        {
            Id = booking.Id,
            Notes = booking.Notes,
            BookingStartDate = booking.bookingStartDate,
            BookingEndDate = booking.bookingEndDate,
            TotalDays = (booking.bookingEndDate - booking.bookingStartDate).Days,
            TotalPrice = (booking.bookingEndDate - booking.bookingStartDate).Days
                                    * (booking.Service?.DailyPrice ?? 0),
            CreatedAt = booking.CreatedAt,

            StatusId = booking.StatusId,
            StatusName = booking.Status?.Name ?? string.Empty,

            ServiceId = booking.ServiceId,
            ServiceName = booking.Service?.ServiceName ?? string.Empty,
            ServiceDailyPrice = booking.Service?.DailyPrice ?? 0,
            ServiceTypeId = booking.Service?.ServiceTypeId ?? 0,
            ServiceTypeName = booking.Service?.ServiceType?.Name ?? string.Empty,

            ServiceProviderId = booking.Service?.ServiceProviderId ?? 0,
            ServiceProviderName = booking.Service?.ServiceProvider?.User?.Name ?? string.Empty,

            RequesterId = booking.RequesterId,
            RequesterName = booking.Requester?.Name ?? string.Empty,

            LocationId = booking.LocationId,
            LocationName = booking.Location?.LocationName,
            LocationCity = booking.Location?.City,
            LocationAddress = booking.Location?.Address,

            Latitude = booking.LocationId == null
                                                  ? booking.Latitude
                                                  : booking.Location?.Latitude,

            Longitude = booking.LocationId == null
                                                    ? booking.Longitude
                                                    : booking.Location?.Longitude,
            LocationOnGoogleMaps = booking.Location?.LocationOnGoogleMaps == null ? booking.LocationOnGoogleMaps : booking.Location.LocationOnGoogleMaps
        };

        private IQueryable<ServiceBooking> BookingsWithIncludes() =>
            _context.ServiceBookings
                .Include(b => b.Status)
                .Include(b => b.Requester)
                .Include(b => b.Location)
                    .ThenInclude(l => l.City)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceType)
                .Include(b => b.Service)
                    .ThenInclude(s => s.ServiceProvider)
                        .ThenInclude(sp => sp.User);


        // the actual code
        public async Task<ApiResponse<CreateServiceBookingDTO>> CreateBookingRequest(
            CreateServiceBookingDTO dto, int currentUserId)
        {
            try
            {

                if (dto.BookingStartDate < DateTime.UtcNow || dto.BookingEndDate < DateTime.UtcNow)
                    return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                        "date must be in the future",
                        "يجب أن يكون تاريخ  في المستقبل"
                    );

                if (dto.BookingStartDate >= dto.BookingEndDate)
                    return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                        "End date must be after start date",
                        "يجب أن يكون تاريخ الانتهاء بعد تاريخ البداية"
                    );

                var service = await _context.ServicesProvided
                    .FirstOrDefaultAsync(s => s.Id == dto.ServiceId && s.IsActive && !s.IsDeleted);

                if (service == null)
                    return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                        "Service not found or unavailable",
                        "الخدمة غير موجودة أو غير متاحة"
                    );

                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                if (serviceProviderId == service.ServiceProviderId)
                    return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                        "You cannot book your own service",
                        "لا يمكنك حجز خدمتك الخاصة"
                    );

                
                var hasOverlap = await _context.ServiceBookings.AnyAsync(b =>
                    b.ServiceId == dto.ServiceId &&
                    !b.IsDeleted &&
                    b.StatusId != StatusRejected() &&
                    b.StatusId != StatusCancelled() &&
                    b.bookingStartDate < dto.BookingEndDate &&
                    b.bookingEndDate > dto.BookingStartDate);

                if (hasOverlap)
                    return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                        "This service is already booked for the selected dates",
                        "هذه الخدمة محجوزة بالفعل في التواريخ المحددة"
                    );

                var booking = new ServiceBooking
                {
                    ServiceId = dto.ServiceId,
                    RequesterId = currentUserId,
                    LocationId = dto.LocationId,
                    Notes = dto.Notes,
                    StatusId = StatusPending(),
                    bookingStartDate = dto.BookingStartDate,
                    bookingEndDate = dto.BookingEndDate,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false
                    
                };
                if(!dto.LocationId.HasValue)
                {

                    if(dto.Latitude != 0 && dto.Longitude != 0)
                    {
                        booking.Latitude = dto.Latitude;
                        booking.Longitude = dto.Longitude;
                        if (dto.LocationOnGoogleMaps.IsNullOrEmpty())
                        {
                            booking.LocationOnGoogleMaps = $"https://www.google.com/maps/search/?api=1&query={dto.Latitude},{dto.Longitude}";
                        }
                        else
                        {
                            booking.LocationOnGoogleMaps = dto.LocationOnGoogleMaps;
                        }
                    }
                    else if (!dto.LocationOnGoogleMaps.IsNullOrEmpty())
                    {
                        booking.LocationOnGoogleMaps = dto.LocationOnGoogleMaps;
                    }
                    else
                    {
                        return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                       "location or latitude and longitude is required",
                       "الموقع أو خط العرض وخط الطول مطلوبان"
                   );
                    }
                }else if (!await _context.Locations
                                        .AnyAsync(l => l.Id == dto.LocationId))
                                    {
                                        return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                                            "Location not found",
                                            "الموقع غير موجود"
                                        );
}

                await _context.ServiceBookings.AddAsync(booking);
                await _context.SaveChangesAsync();

                return ApiResponse<CreateServiceBookingDTO>.SuccessResponse(
                    dto,
                    "Booking request created successfully",
                    "تم إنشاء طلب الحجز بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking request for user {UserId}", currentUserId);
                return ApiResponse<CreateServiceBookingDTO>.FailureResponse(
                    "An error occurred while creating the booking request",
                    "حدث خطأ أثناء إنشاء طلب الحجز"
                );
            }
        }


        public async Task<ApiResponse<bool>> AcceptBooking(int bookingId, int currentUserId)
        {
            try
            {
                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);

                if (serviceProviderId == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service provider profile not found",
                        "لم يتم العثور على ملف مزود الخدمة"
                    );

                var booking = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Booking not found",
                        "الحجز غير موجود"
                    );

                if (booking.Service.ServiceProviderId != serviceProviderId.Value)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to accept this booking",
                        "غير مصرح لك بقبول هذا الحجز"
                    );

                if (booking.StatusId != StatusPending())
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending bookings can be accepted",
                        "يمكن قبول الحجوزات المعلقة فقط"
                    );

                booking.StatusId = StatusAccepted();
                booking.UpdatedAt = DateTime.UtcNow;
                booking.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Booking accepted successfully",
                    "تم قبول الحجز بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error accepting booking {BookingId}", bookingId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while accepting the booking",
                    "حدث خطأ أثناء قبول الحجز"
                );
            }
        }

        public async Task<ApiResponse<bool>> RejectBooking(
            int bookingId, string? rejectionReason, int currentUserId)
        {
            try
            {
                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                if (serviceProviderId == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service provider profile not found",
                        "لم يتم العثور على ملف مزود الخدمة"
                    );

                var booking = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Booking not found",
                        "الحجز غير موجود"
                    );

                if (booking.Service.ServiceProviderId != serviceProviderId.Value)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to reject this booking",
                        "غير مصرح لك برفض هذا الحجز"
                    );

                if (booking.StatusId != StatusPending())
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending bookings can be rejected",
                        "يمكن رفض الحجوزات المعلقة فقط"
                    );

                booking.StatusId = StatusRejected();
                //booking.RejectionReason = rejectionReason;   // Add this field to ServiceBooking entity
                booking.UpdatedAt = DateTime.UtcNow;
                booking.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Booking rejected successfully",
                    "تم رفض الحجز بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error rejecting booking {BookingId}", bookingId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while rejecting the booking",
                    "حدث خطأ أثناء رفض الحجز"
                );
            }
        }

        public async Task<ApiResponse<bool>> CancelBooking(int bookingId, int currentUserId)
        {
            try
            {
                var booking = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Booking not found",
                        "الحجز غير موجود"
                    );

                
                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                bool isRequester = booking.RequesterId == currentUserId;
                bool isProvider = serviceProviderId.HasValue &&
                                   booking.Service.ServiceProviderId == serviceProviderId.Value;

                if (!isRequester && !isProvider)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to cancel this booking",
                        "غير مصرح لك بإلغاء هذا الحجز"
                    );

                if (booking.StatusId == StatusCompleted() || booking.StatusId == StatusCancelled())
                    return ApiResponse<bool>.FailureResponse(
                        "This booking cannot be cancelled",
                        "لا يمكن إلغاء هذا الحجز"
                    );

                booking.StatusId = StatusCancelled();
                booking.UpdatedAt = DateTime.UtcNow;
                booking.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Booking cancelled successfully",
                    "تم إلغاء الحجز بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking {BookingId}", bookingId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the booking",
                    "حدث خطأ أثناء إلغاء الحجز"
                );
            }
        }

        public async Task<ApiResponse<bool>> CompleteBooking(int bookingId, int currentUserId)
        {
            try
            {
                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                if (serviceProviderId == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service provider profile not found",
                        "لم يتم العثور على ملف مزود الخدمة"
                    );

                var booking = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Booking not found",
                        "الحجز غير موجود"
                    );

                if (booking.Service.ServiceProviderId != serviceProviderId.Value)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to complete this booking",
                        "غير مصرح لك بإتمام هذا الحجز"
                    );

                if (booking.StatusId != StatusAccepted())
                    return ApiResponse<bool>.FailureResponse(
                        "Only accepted bookings can be marked as completed",
                        "يمكن إتمام الحجوزات المقبولة فقط"
                    );

                booking.StatusId = StatusCompleted();
                booking.UpdatedAt = DateTime.UtcNow;
                booking.UpdatedBy = currentUserId.ToString();


                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Booking completed successfully",
                    "تم إتمام الحجز بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error completing booking {BookingId}", bookingId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while completing the booking",
                    "حدث خطأ أثناء إتمام الحجز"
                );
            }
        }

        public async Task<ApiResponse<bool>> ExtendBooking(
            int bookingId, int currentUserId, int days)
        {
            try
            {
                if (days <= 0)
                    return ApiResponse<bool>.FailureResponse(
                        "Extension days must be greater than zero",
                        "يجب أن تكون أيام التمديد أكبر من الصفر"
                    );

                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);

                var booking = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Booking not found",
                        "الحجز غير موجود"
                    );

                bool isRequester = booking.RequesterId == currentUserId;
                bool isProvider = serviceProviderId.HasValue &&
                                   booking.Service.ServiceProviderId == serviceProviderId.Value;

                if (!isRequester && !isProvider)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to extend this booking",
                        "غير مصرح لك بتمديد هذا الحجز"
                    );

                if (booking.StatusId != StatusAccepted())
                    return ApiResponse<bool>.FailureResponse(
                        "Only accepted bookings can be extended",
                        "يمكن تمديد الحجوزات المقبولة فقط"
                    );

                var newEndDate = booking.bookingEndDate.AddDays(days);

                // Check no other booking overlaps the extended period
                var hasOverlap = await _context.ServiceBookings.AnyAsync(b =>
                    b.ServiceId == booking.ServiceId &&
                    b.Id != bookingId &&
                    !b.IsDeleted &&
                    b.StatusId != StatusRejected() &&
                    b.StatusId != StatusCancelled() &&
                    b.bookingStartDate < newEndDate &&
                    b.bookingEndDate > booking.bookingEndDate);

                if (hasOverlap)
                    return ApiResponse<bool>.FailureResponse(
                        "Cannot extend: the service is already booked in that period",
                        "لا يمكن التمديد: الخدمة محجوزة بالفعل في تلك الفترة"
                    );

                booking.bookingEndDate = newEndDate;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    $"Booking extended by {days} day(s) successfully",
                    $"تم تمديد الحجز بمقدار {days} يوم بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error extending booking {BookingId}", bookingId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while extending the booking",
                    "حدث خطأ أثناء تمديد الحجز"
                );
            }
        }

        public async Task<ApiResponse<bool>> UpdateBookingStatus(
            int bookingId, int bookingStatusId, int currentUserId)
        {
            try
            {
                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                if (serviceProviderId == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service provider profile not found",
                        "لم يتم العثور على ملف مزود الخدمة"
                    );

                var statusExists = await _context.LookupItems.AnyAsync(
                    l => l.Id == bookingStatusId && l.LookupCategoryId == 3 && !l.IsDeleted);

                if (!statusExists)
                    return ApiResponse<bool>.FailureResponse(
                        "Invalid status",
                        "الحالة غير صالحة"
                    );

                var booking = await _context.ServiceBookings
                    .Include(b => b.Service)
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Booking not found",
                        "الحجز غير موجود"
                    );

                if (booking.Service.ServiceProviderId != serviceProviderId.Value)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to update this booking",
                        "غير مصرح لك بتحديث هذا الحجز"
                    );

                booking.StatusId = bookingStatusId;
                booking.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Booking status updated successfully",
                    "تم تحديث حالة الحجز بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating status for booking {BookingId}", bookingId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while updating the booking status",
                    "حدث خطأ أثناء تحديث حالة الحجز"
                );
            }
        }


        public async Task<ApiResponse<GetServiceBookingDTO>> GetBookingById(int bookingId)
        {
            try
            {
                var booking = await BookingsWithIncludes()
                    .FirstOrDefaultAsync(b => b.Id == bookingId && !b.IsDeleted);

                if (booking == null)
                    return ApiResponse<GetServiceBookingDTO>.FailureResponse(
                        "Booking not found",
                        "الحجز غير موجود"
                    );

                return ApiResponse<GetServiceBookingDTO>.SuccessResponse(
                    MapToDTO(booking),
                    "Booking retrieved successfully",
                    "تم استرجاع الحجز بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking {BookingId}", bookingId);
                return ApiResponse<GetServiceBookingDTO>.FailureResponse(
                    "An error occurred while retrieving the booking",
                    "حدث خطأ أثناء استرجاع الحجز"
                );
            }
        }

        public async Task<ApiResponse<List<GetServiceBookingDTO>>> GetProviderBookings(int currentUserId)
        {
            try
            {
                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                if (serviceProviderId == null)
                    return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                        "Service provider profile not found",
                        "لم يتم العثور على ملف مزود الخدمة"
                    );

                var bookings = await BookingsWithIncludes()
                    .Where(b => b.Service.ServiceProviderId == serviceProviderId.Value && !b.IsDeleted)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                return ApiResponse<List<GetServiceBookingDTO>>.SuccessResponse(
                    bookings,
                    "Provider bookings retrieved successfully",
                    "تم استرجاع حجوزات المزود بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving provider bookings for user {UserId}", currentUserId);
                return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                    "An error occurred while retrieving bookings",
                    "حدث خطأ أثناء استرجاع الحجوزات"
                );
            }
        }

        public async Task<ApiResponse<List<GetServiceBookingDTO>>> GetReceivedBookings(int currentUserId)
        {
            try
            {
                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                if (serviceProviderId == null)
                    return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                        "Service provider profile not found",
                        "لم يتم العثور على ملف مزود الخدمة"
                    );

                var bookings = await BookingsWithIncludes()
                    .Where(b =>
                        b.Service.ServiceProviderId == serviceProviderId.Value &&
                        b.StatusId == StatusPending() &&
                        !b.IsDeleted)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                return ApiResponse<List<GetServiceBookingDTO>>.SuccessResponse(
                    bookings,
                    "Received booking requests retrieved successfully",
                    "تم استرجاع طلبات الحجز الواردة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving received bookings for user {UserId}", currentUserId);
                return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                    "An error occurred while retrieving received bookings",
                    "حدث خطأ أثناء استرجاع طلبات الحجز الواردة"
                );
            }
        }

        public async Task<ApiResponse<List<GetServiceBookingDTO>>> GetServicesIBooked(int currentUserId)
        {
            try
            {
                var bookings = await BookingsWithIncludes()
                    .Where(b => b.RequesterId == currentUserId && !b.IsDeleted)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                return ApiResponse<List<GetServiceBookingDTO>>.SuccessResponse(
                    bookings,
                    "Your bookings retrieved successfully",
                    "تم استرجاع حجوزاتك بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for requester {UserId}", currentUserId);
                return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                    "An error occurred while retrieving your bookings",
                    "حدث خطأ أثناء استرجاع حجوزاتك"
                );
            }
        }

        public async Task<ApiResponse<List<GetServiceBookingDTO>>> GetBookingsByLocation(int locationId)
        {
            try
            {
                var bookings = await BookingsWithIncludes()
                    .Where(b => b.LocationId == locationId && !b.IsDeleted)
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                return ApiResponse<List<GetServiceBookingDTO>>.SuccessResponse(
                    bookings,
                    "Bookings retrieved successfully",
                    "تم استرجاع الحجوزات بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for location {LocationId}", locationId);
                return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                    "An error occurred while retrieving bookings by location",
                    "حدث خطأ أثناء استرجاع الحجوزات حسب الموقع"
                );
            }
        }

        public Task<ApiResponse<List<GetServiceBookingDTO>>> GetBookingsByProject(int projectId)
        {
            // TODO: Add ProjectId to ServiceBooking entity and re-implement this method
            _logger.LogWarning("GetBookingsByProject called but ProjectId is not mapped on ServiceBooking entity");
            return Task.FromResult(ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                "This feature is not yet available",
                "هذه الميزة غير متاحة بعد"
            ));
        }

        public async Task<ApiResponse<List<GetServiceBookingDTO>>> GetProviderBookingsByDate(
            DateTime startDate, DateTime endDate, int currentUserId)
        {
            try
            {
                if (startDate >= endDate)
                    return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                        "End date must be after start date",
                        "يجب أن يكون تاريخ الانتهاء بعد تاريخ البداية"
                    );

                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);
                if (serviceProviderId == null)
                    return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                        "Service provider profile not found",
                        "لم يتم العثور على ملف مزود الخدمة"
                    );

                var bookings = await BookingsWithIncludes()
                    .Where(b =>
                        b.Service.ServiceProviderId == serviceProviderId.Value &&
                        !b.IsDeleted &&
                        b.bookingStartDate >= startDate &&
                        b.bookingEndDate <= endDate)
                    .OrderBy(b => b.bookingStartDate)
                    .Select(b => MapToDTO(b))
                    .ToListAsync();

                return ApiResponse<List<GetServiceBookingDTO>>.SuccessResponse(
                    bookings,
                    "Bookings retrieved successfully",
                    "تم استرجاع الحجوزات بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings by date for user {UserId}", currentUserId);
                return ApiResponse<List<GetServiceBookingDTO>>.FailureResponse(
                    "An error occurred while retrieving bookings",
                    "حدث خطأ أثناء استرجاع الحجوزات"
                );
            }
        }
    }
}