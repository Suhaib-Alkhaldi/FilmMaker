using FilmMaker.Common;
using FilmMaker.DTO.LocationBooking;
using FilmMaker.DTO.LocationManager;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationBookingService : ILocationBookingService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<LocationBookingService> _logger;

        public LocationBookingService(
            FilmMakerDbContext context,
            ILogger<LocationBookingService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<BookingRequestDto>> CreateBookingRequestAsync(int currentUserId,CreateBookingRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                if (dto.StartDateTime >= dto.EndDateTime)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Start time must be before end time.",
                        "وقت البداية يجب أن يكون قبل وقت النهاية."
                    );
                }

                if (dto.StartDateTime < DateTime.UtcNow)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Cannot book in the past.",
                        "لا يمكن الحجز في تاريخ ماضٍ."
                    );
                }

                var productionCompanyProfileId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var locationManagerProfileId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isProductionCompany = productionCompanyProfileId.HasValue;
                var isLocationManager = locationManagerProfileId.HasValue;

                if (isProductionCompany == isLocationManager)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "User must be either a production company or a location manager to create a booking request.",
                        "يجب أن يكون المستخدم شركة إنتاج أو مدير موقع لإنشاء طلب حجز."
                    );
                }

                var location = await _context.Locations
                    .Where(l =>
                        l.Id == dto.LocationId &&
                        l.IsActive &&
                        !l.IsDeleted).FirstOrDefaultAsync();

                if (location == null)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Location not found or inactive.",
                        "الموقع غير موجود أو غير نشط."
                    );
                }

                var isFullDay = (dto.EndDateTime - dto.StartDateTime).TotalHours >= 4;

                var availability = await CheckBookingAvailabilityForCreate(
                    location.Id,
                    dto.StartDateTime,
                    dto.EndDateTime
                );

                if (!availability.CanCreateRequest)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        availability.MessageEn,
                        availability.MessageAr
                    );
                }

                var pendingStatus = await _context.LookupItems
                    .Include(l => l.LookupCategory)
                    .FirstOrDefaultAsync(l =>
                        l.Name == "Pending" &&
                        l.LookupCategory.Name == "BookingStatus" &&
                        l.IsActive &&
                        !l.IsDeleted);

                if (pendingStatus == null)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "System error: pending booking status was not found.",
                        "خطأ في النظام: حالة الحجز قيد الانتظار غير موجودة."
                    );
                }

                var totalPrice = CalculateTotalPrice(
                    location,
                    dto.StartDateTime,
                    dto.EndDateTime,
                    isFullDay
                );

                var booking = new LocationBookingRequest
                {
                    LocationId = location.Id,

                    StartDateTime = dto.StartDateTime,
                    EndDateTime = dto.EndDateTime,

                    Message = string.IsNullOrWhiteSpace(dto.Message)
                        ? null
                        : dto.Message.Trim(),

                    LocationOwnerId = location.LocationOwnerId,

                    ProductionCompanyId = isProductionCompany
                        ? productionCompanyProfileId.Value
                        : null,

                    LocationManagerId = isLocationManager
                        ? locationManagerProfileId.Value
                        : null,

                    BookingStatusId = pendingStatus.Id,
                    TotalPrice = totalPrice,

                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false
                };

                _context.LocationBookingRequests.Add(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Booking request created. LocationId: {LocationId}, UserId: {UserId}, ProductionCompanyId: {ProductionCompanyId}, LocationManagerId: {LocationManagerId}, CalendarColorBeforeCreate: {CalendarColor}",
                    location.Id,
                    currentUserId,
                    booking.ProductionCompanyId,
                    booking.LocationManagerId,
                    availability.CalendarColor
                );

                var createdBooking = await _context.LocationBookingRequests
    .Include(b => b.Location)
        .ThenInclude(l => l.LocationOwner)
            .ThenInclude(o => o.User)
    .Include(b => b.BookingStatus)
    .Include(b => b.LocationManager)
        .ThenInclude(m => m.User)
    .Include(b => b.ProductionCompany)
        .ThenInclude(p => p.User)
    .FirstOrDefaultAsync(b =>
        b.Id == booking.Id &&
        b.IsActive &&
        !b.IsDeleted);

                if (createdBooking == null)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Booking request was created, but response could not be loaded.",
                        "تم إنشاء طلب الحجز، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<BookingRequestDto>.SuccessResponse(
                    MapToDto(
                        createdBooking,
                        createdBooking.Location,
                        isFullDay,
                        createdBooking.BookingStatus.Name
                    ),
                    availability.HasPendingRequest
                        ? "Booking request sent successfully. This date already has pending requests."
                        : "Booking request sent successfully.",
                    availability.HasPendingRequest
                        ? "تم إرسال طلب الحجز بنجاح. يوجد طلبات أخرى قيد الانتظار على هذا التاريخ."
                        : "تم إرسال طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating booking request. LocationId: {LocationId}, UserId: {UserId}",
                    dto?.LocationId,
                    currentUserId
                );

                return ApiResponse<BookingRequestDto>.FailureResponse(
                    "An error occurred while creating the booking request.",
                    "حدث خطأ أثناء إنشاء طلب الحجز."
                );
            }
        }

        public async Task<ApiResponse<List<BookingRequestDto>>> GetMyBookingRequestsAsync(int currentUserId)
        {
            try
            {
                var productionCompanyProfileId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var locationManagerProfileId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isProductionCompany = productionCompanyProfileId.HasValue;
                var isLocationManager = locationManagerProfileId.HasValue;

                if (isProductionCompany == isLocationManager)
                {
                    return ApiResponse<List<BookingRequestDto>>.FailureResponse(
                        "User must be either a production company or a location manager.",
                        "يجب أن يكون المستخدم إما شركة إنتاج أو مدير موقع."
                    );
                }

                var query = _context.LocationBookingRequests
                    .Where(b =>
                        b.IsActive &&
                        !b.IsDeleted);

                if (isProductionCompany)
                {
                    query = query.Where(b =>
                        b.ProductionCompanyId == productionCompanyProfileId.Value);
                }
                else
                {
                    query = query.Where(b =>
                        b.LocationManagerId == locationManagerProfileId.Value);
                }

                var requests = await query
                    .OrderByDescending(b => b.CreatedAt)
                    .Select(b => new BookingRequestDto
                    {
                        Id = b.Id,

                        LocationId = b.LocationId,
                        LocationName = b.Location.LocationName,
                        City = b.Location.City,

                        StartDateTime = b.StartDateTime,
                        EndDateTime = b.EndDateTime,
                        IsFullDay = (b.EndDateTime - b.StartDateTime).TotalHours >= 4,

                        Status = b.BookingStatus.Name,
                        Message = b.Message,
                        TotalPrice = b.TotalPrice,

                        LocationOwnerId = b.LocationOwnerId,
                        LocationOwnerName = b.LocationOwner.User.Name,
                        LocationManagerId = b.LocationManagerId,
                        LocationManagerName = b.LocationManager.User.Name,
                        ProductionCompanyId = b.ProductionCompanyId,
                        ProductionCompanyName = b.ProductionCompany.User.Name,

                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();

                if (!requests.Any())
                { 
                    return ApiResponse<List<BookingRequestDto>>.SuccessResponse(
                        requests,
                        "No booking requests found.",
                        "لا توجد طلبات حجز."
                    );
                }

                return ApiResponse<List<BookingRequestDto>>.SuccessResponse(
                    requests,
                    "Booking requests retrieved successfully.",
                    "تم جلب طلبات الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting booking requests for user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<BookingRequestDto>>.FailureResponse(
                    "An error occurred while retrieving booking requests.",
                    "حدث خطأ أثناء جلب طلبات الحجز."
                );
            }
        }

        public async Task<ApiResponse<BookingRequestDto>> GetBookingRequestByIdAsync(int requestId,int currentUserId)
        {
            try
            {
                var productionCompanyProfileId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var locationManagerProfileId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isProductionCompany = productionCompanyProfileId.HasValue;
                var isLocationManager = locationManagerProfileId.HasValue;

                if (isProductionCompany == isLocationManager)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "User must be either a production company or a location manager.",
                        "يجب أن يكون المستخدم إما شركة إنتاج أو مدير موقع."
                    );
                }

                var query = _context.LocationBookingRequests
                    .Where(b =>
                        b.Id == requestId &&
                        b.IsActive &&
                        !b.IsDeleted);

                if (isProductionCompany)
                {
                    query = query.Where(b =>
                        b.ProductionCompanyId == productionCompanyProfileId.Value);
                }
                else
                {
                    query = query.Where(b =>
                        b.LocationManagerId == locationManagerProfileId.Value);
                }

                var response = await query
                    .Select(b => new BookingRequestDto
                    {
                        Id = b.Id,

                        LocationId = b.LocationId,
                        LocationName = b.Location.LocationName,
                        City = b.Location.City,

                        StartDateTime = b.StartDateTime,
                        EndDateTime = b.EndDateTime,
                        IsFullDay = (b.EndDateTime - b.StartDateTime).TotalHours >= 4,

                        Status = b.BookingStatus.Name,
                        Message = b.Message,
                        TotalPrice = b.TotalPrice,

                        LocationOwnerId = b.LocationOwnerId,
                        LocationOwnerName = b.LocationOwner.User.Name,
                        LocationManagerId = b.LocationManagerId,
                        LocationManagerName = b.LocationManager.User.Name,
                        ProductionCompanyId = b.ProductionCompanyId,
                        ProductionCompanyName = b.ProductionCompany.User.Name,

                        CreatedAt = b.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (response == null)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Booking request not found.",
                        "طلب الحجز غير موجود."
                    );
                }

                return ApiResponse<BookingRequestDto>.SuccessResponse(
                    response,
                    "Booking request retrieved successfully.",
                    "تم جلب طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting booking request {RequestId} for user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<BookingRequestDto>.FailureResponse(
                    "An error occurred while retrieving the booking request.",
                    "حدث خطأ أثناء جلب طلب الحجز."
                );
            }
        }

        public async Task<ApiResponse<BookingRequestDto>> UpdateBookingRequestAsync(int currentUserId,UpdateBookingRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                var productionCompanyProfileId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var locationManagerProfileId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isProductionCompany = productionCompanyProfileId.HasValue;
                var isLocationManager = locationManagerProfileId.HasValue;

                if (isProductionCompany == isLocationManager)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "User must be either a production company or a location manager.",
                        "يجب أن يكون المستخدم إما شركة إنتاج أو مدير موقع."
                    );
                }

                var query = _context.LocationBookingRequests
                    .Include(b => b.BookingStatus)
                    .Include(b => b.Location)
                    .ThenInclude(l => l.LocationOwner)
                    .ThenInclude(o => o.User)
                    .Include(b => b.LocationManager)
                    .ThenInclude(m => m.User)
                    .Include(b => b.ProductionCompany)
                    .ThenInclude(p => p.User)
                    .Where(b =>
                        b.Id == dto.requestId &&
                        b.IsActive &&
                        !b.IsDeleted);

                if (isProductionCompany)
                {
                    query = query.Where(b =>
                        b.ProductionCompanyId == productionCompanyProfileId.Value);
                }
                else
                {
                    query = query.Where(b =>
                        b.LocationManagerId == locationManagerProfileId.Value);
                }

                var request = await query.FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Booking request not found.",
                        "طلب الحجز غير موجود."
                    );
                }

                if (request.BookingStatus.Name != "Pending")
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Only pending requests can be updated.",
                        "يمكن تعديل الطلبات المعلقة فقط."
                    );
                }

                var finalStart = dto.StartDateTime ?? request.StartDateTime;
                var finalEnd = dto.EndDateTime ?? request.EndDateTime;

                if (finalStart >= finalEnd)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Start time must be before end time.",
                        "وقت البداية يجب أن يكون قبل وقت النهاية."
                    );
                }

                if (finalStart < DateTime.UtcNow)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        "Cannot book in the past.",
                        "لا يمكن الحجز في تاريخ ماضٍ."
                    );
                }

                var isFullDay = (finalEnd - finalStart).TotalHours >= 4;

                var availability = await CheckBookingAvailabilityForUpdate(
                    request.LocationId,
                    request.Id,
                    finalStart,
                    finalEnd
                );

                if (!availability.CanCreateRequest)
                {
                    return ApiResponse<BookingRequestDto>.FailureResponse(
                        availability.MessageEn,
                        availability.MessageAr
                    );
                }

                request.StartDateTime = finalStart;
                request.EndDateTime = finalEnd;

                if (dto.Message is not null)
                {
                    request.Message = string.IsNullOrWhiteSpace(dto.Message)
                        ? null
                        : dto.Message.Trim();
                }

                request.TotalPrice = CalculateTotalPrice(
                    request.Location,
                    finalStart,
                    finalEnd,
                    isFullDay
                );

                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Booking request {RequestId} updated by UserId {UserId}",
                    dto.requestId,
                    currentUserId
                );

                return ApiResponse<BookingRequestDto>.SuccessResponse(
                    MapToDto(request, request.Location, isFullDay, request.BookingStatus.Name),
                    availability.HasPendingRequest
                        ? "Booking request updated successfully. This date already has pending requests."
                        : "Booking request updated successfully.",
                    availability.HasPendingRequest
                        ? "تم تعديل طلب الحجز بنجاح. يوجد طلبات أخرى قيد الانتظار على هذا التاريخ."
                        : "تم تعديل طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating booking request {RequestId} for UserId {UserId}",
                    dto.requestId,
                    currentUserId
                );

                return ApiResponse<BookingRequestDto>.FailureResponse(
                    "An error occurred while updating the booking request.",
                    "حدث خطأ أثناء تعديل طلب الحجز."
                );
            }
        }

        public async Task<ApiResponse<bool>> CancelBookingRequestAsync(int requestId,int currentUserId)
        {
            try
            {
                var productionCompanyProfileId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var locationManagerProfileId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isProductionCompany = productionCompanyProfileId.HasValue;
                var isLocationManager = locationManagerProfileId.HasValue;

                if (isProductionCompany == isLocationManager)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "User must be either a production company or a location manager.",
                        "يجب أن يكون المستخدم إما شركة إنتاج أو مدير موقع."
                    );
                }

                var query = _context.LocationBookingRequests
                    .Include(b => b.BookingStatus)
                    .Where(b =>
                        b.Id == requestId &&
                        b.IsActive &&
                        !b.IsDeleted);

                if (isProductionCompany)
                {
                    query = query.Where(b =>
                        b.ProductionCompanyId == productionCompanyProfileId.Value);
                }
                else
                {
                    query = query.Where(b =>
                        b.LocationManagerId == locationManagerProfileId.Value);
                }

                var request = await query.FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Booking request not found.",
                        "طلب الحجز غير موجود."
                    );
                }

                if (request.BookingStatus.Name != "Pending")
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending requests can be cancelled.",
                        "يمكن إلغاء الطلبات المعلقة فقط."
                    );
                }

                var cancelledStatus = await _context.LookupItems
                    .Include(x => x.LookupCategory)
                    .FirstOrDefaultAsync(x =>
                        x.Name == "Cancelled" &&
                        x.LookupCategory.Name == "BookingStatus" &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (cancelledStatus == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Cancelled booking status was not found in lookup data.",
                        "حالة إلغاء الحجز غير موجودة في بيانات النظام."
                    );
                }

                request.BookingStatusId = cancelledStatus.Id;
                request.IsActive = false;
                request.IsDeleted = true;
                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Booking request {RequestId} cancelled by UserId {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Booking request cancelled successfully.",
                    "تم إلغاء طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cancelling booking request {RequestId} for UserId {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the booking request.",
                    "حدث خطأ أثناء إلغاء طلب الحجز."
                );
            }
        }


        public async Task<ApiResponse<List<LocationBookingCalendarDayDto>>> GetLocationBookingCalendarAsync(int locationId,DateTime fromDate,DateTime toDate)
        {
            try
            {
                if (fromDate.Date > toDate.Date)
                {
                    return ApiResponse<List<LocationBookingCalendarDayDto>>.FailureResponse(
                        "From date must be before or equal to to date.",
                        "تاريخ البداية يجب أن يكون قبل أو يساوي تاريخ النهاية."
                    );
                }

                var locationExists = await _context.Locations
                    .AnyAsync(x =>
                        x.Id == locationId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!locationExists)
                {
                    return ApiResponse<List<LocationBookingCalendarDayDto>>.FailureResponse(
                        "Location not found or inactive.",
                        "الموقع غير موجود أو غير نشط."
                    );
                }

                var from = fromDate.Date;
                var to = toDate.Date;

                var bookedStatuses = new[]
                {
                    "Accepted",
                    "Confirmed",
                    "Contract Created",
                    "Awaiting Contract Approval",
                    "Contract Signed",
                    "Payment Pending"
                };

                var bookings = await _context.LocationBookingRequests
                    .Where(x =>
                        x.LocationId == locationId &&
                        x.IsActive &&
                        !x.IsDeleted &&
                        x.StartDateTime.Date <= to &&
                        x.EndDateTime.Date >= from &&
                        (
                            x.BookingStatus.Name == "Pending" ||
                            bookedStatuses.Contains(x.BookingStatus.Name)
                        ))
                    .Select(x => new
                    {
                        x.Id,
                        x.StartDateTime,
                        x.EndDateTime,
                        StatusName = x.BookingStatus.Name
                    })
                    .ToListAsync();

                var result = new List<LocationBookingCalendarDayDto>();

                for (var day = from; day <= to; day = day.AddDays(1))
                {
                    var dayBookings = bookings
                        .Where(x =>
                            x.StartDateTime.Date <= day &&
                            x.EndDateTime.Date >= day)
                        .ToList();

                    var bookedCount = dayBookings.Count(x =>
                        bookedStatuses.Contains(x.StatusName));

                    var pendingCount = dayBookings.Count(x =>
                        x.StatusName == "Pending");

                    var isBooked = bookedCount > 0;
                    var hasPendingRequest = pendingCount > 0;
                    var isAvailable = !isBooked && !hasPendingRequest;

                    var status = isBooked
                        ? "Booked"
                        : hasPendingRequest
                            ? "Pending"
                            : "Available";

                    result.Add(new LocationBookingCalendarDayDto
                    {
                        Date = day,

                        IsBooked = isBooked,
                        HasPendingRequest = hasPendingRequest,
                        IsAvailable = isAvailable,

                        CanRequest = !isBooked,

                        PendingCount = pendingCount,
                        BookedCount = bookedCount,

                        Status = status
                    });
                }

                return ApiResponse<List<LocationBookingCalendarDayDto>>.SuccessResponse(
                    result,
                    "Location booking calendar fetched successfully.",
                    "تم جلب تقويم حجوزات الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching location booking calendar. LocationId: {LocationId}, FromDate: {FromDate}, ToDate: {ToDate}",
                    locationId,
                    fromDate,
                    toDate
                );

                return ApiResponse<List<LocationBookingCalendarDayDto>>.FailureResponse(
                    "An unexpected error occurred while fetching booking calendar.",
                    "حدث خطأ غير متوقع أثناء جلب تقويم الحجوزات."
                );
            }
        }




        #region Helper Methods


        private static decimal CalculateTotalPrice(Location location,DateTime start,DateTime end,bool isFullDay)
        {
            if (isFullDay)
                return location.DailyPrice;

            var hours = (decimal)(end - start).TotalHours;

            if (hours <= 0)
                return 0;

            if (location.HourlyPrice == null || location.HourlyPrice <= 0)
                return location.DailyPrice;

            return hours * location.HourlyPrice.Value;
        }

        private static BookingRequestDto MapToDto(LocationBookingRequest request,Location location,bool? isFullDay = null,string? statusName = null)
        {
            var fullDay = isFullDay ?? (request.EndDateTime - request.StartDateTime).TotalHours >= 4;

            return new BookingRequestDto
            {
                Id = request.Id,
                LocationId = request.LocationId,
                LocationName = location.LocationName,
                City = location.City,
                StartDateTime = request.StartDateTime,
                EndDateTime = request.EndDateTime,
                IsFullDay = fullDay,
                Status = statusName ?? request.BookingStatus?.Name ?? string.Empty,
                Message = request.Message,
                TotalPrice = request.TotalPrice,
                LocationOwnerId = request.LocationOwnerId,
                LocationOwnerName = location.LocationOwner?.User?.Name ?? string.Empty,

                LocationManagerId = request.LocationManagerId,
                LocationManagerName = request.LocationManager?.User?.Name,

                ProductionCompanyId = request.ProductionCompanyId,
                ProductionCompanyName = request.ProductionCompany?.User?.Name,

                CreatedAt = request.CreatedAt
            };
        }


        private class BookingAvailabilityCheckResult
        {
            public bool CanCreateRequest { get; set; }

            public bool HasBookedConflict { get; set; }

            public bool HasPendingRequest { get; set; }

            public string CalendarColor { get; set; } = "Green";

            public string MessageEn { get; set; } = string.Empty;

            public string MessageAr { get; set; } = string.Empty;
        }

        private async Task<BookingAvailabilityCheckResult> CheckBookingAvailabilityForCreate(int locationId,DateTime startDateTime,DateTime endDateTime)
        {
            var startDate = startDateTime.Date;
            var endDate = endDateTime.Date;

            var redStatuses = new[]
            {
                "Accepted",
                "Confirmed",
                "Contract Created",
                "Awaiting Contract Approval",
                "Contract Signed",
                "Payment Pending"
            };

            var hasBookedConflict = await _context.LocationBookingRequests
                .AnyAsync(b =>
                    b.LocationId == locationId &&
                    b.IsActive &&
                    !b.IsDeleted &&
                    redStatuses.Contains(b.BookingStatus.Name) &&
                    b.StartDateTime.Date <= endDate &&
                    b.EndDateTime.Date >= startDate);

            if (hasBookedConflict)
            {
                return new BookingAvailabilityCheckResult
                {
                    CanCreateRequest = false,
                    HasBookedConflict = true,
                    HasPendingRequest = false,
                    CalendarColor = "Red",
                    MessageEn = "This location is already booked for the selected date.",
                    MessageAr = "هذا الموقع محجوز في التاريخ المحدد."
                };
            }

            var hasPendingRequest = await _context.LocationBookingRequests
                .AnyAsync(b =>
                    b.LocationId == locationId &&
                    b.IsActive &&
                    !b.IsDeleted &&
                    b.BookingStatus.Name == "Pending" &&
                    b.StartDateTime.Date <= endDate &&
                    b.EndDateTime.Date >= startDate);

            if (hasPendingRequest)
            {
                return new BookingAvailabilityCheckResult
                {
                    CanCreateRequest = true,
                    HasBookedConflict = false,
                    HasPendingRequest = true,
                    CalendarColor = "Orange",
                    MessageEn = "This location has pending booking requests, but new requests are allowed.",
                    MessageAr = "يوجد طلبات حجز قيد الانتظار على هذا الموقع، لكن يمكن إرسال طلب جديد."
                };
            }

            return new BookingAvailabilityCheckResult
            {
                CanCreateRequest = true,
                HasBookedConflict = false,
                HasPendingRequest = false,
                CalendarColor = "Green",
                MessageEn = "This location is available.",
                MessageAr = "هذا الموقع متاح."
            };
        }


        private async Task<BookingAvailabilityCheckResult> CheckBookingAvailabilityForUpdate(int locationId,int currentBookingRequestId,DateTime startDateTime,DateTime endDateTime)
        {
            var startDate = startDateTime.Date;
            var endDate = endDateTime.Date;

            var redStatuses = new[]
            {
                "Accepted",
                "Confirmed",
                "Contract Created",
                "Awaiting Contract Approval",
                "Contract Signed",
                "Payment Pending"
            };

            var hasBookedConflict = await _context.LocationBookingRequests
                .AnyAsync(b =>
                    b.Id != currentBookingRequestId &&
                    b.LocationId == locationId &&
                    b.IsActive &&
                    !b.IsDeleted &&
                    redStatuses.Contains(b.BookingStatus.Name) &&
                    b.StartDateTime.Date <= endDate &&
                    b.EndDateTime.Date >= startDate);

            if (hasBookedConflict)
            {
                return new BookingAvailabilityCheckResult
                {
                    CanCreateRequest = false,
                    HasBookedConflict = true,
                    HasPendingRequest = false,
                    CalendarColor = "Red",
                    MessageEn = "This location is already booked for the selected date.",
                    MessageAr = "هذا الموقع محجوز في التاريخ المحدد."
                };
            }

            var hasPendingRequest = await _context.LocationBookingRequests
                .AnyAsync(b =>
                    b.Id != currentBookingRequestId &&
                    b.LocationId == locationId &&
                    b.IsActive &&
                    !b.IsDeleted &&
                    b.BookingStatus.Name == "Pending" &&
                    b.StartDateTime.Date <= endDate &&
                    b.EndDateTime.Date >= startDate);

            if (hasPendingRequest)
            {
                return new BookingAvailabilityCheckResult
                {
                    CanCreateRequest = true,
                    HasBookedConflict = false,
                    HasPendingRequest = true,
                    CalendarColor = "Orange",
                    MessageEn = "This location has pending booking requests, but updates are allowed.",
                    MessageAr = "يوجد طلبات حجز قيد الانتظار على هذا الموقع، لكن يمكن تعديل الطلب."
                };
            }

            return new BookingAvailabilityCheckResult
            {
                CanCreateRequest = true,
                HasBookedConflict = false,
                HasPendingRequest = false,
                CalendarColor = "Green",
                MessageEn = "This location is available.",
                MessageAr = "هذا الموقع متاح."
            };
        }
        #endregion
    }
}