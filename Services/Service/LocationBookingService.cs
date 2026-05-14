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

        public async Task<ApiResponse<BookingRequestResponseDto>> CreateBookingRequestAsync(int productionCompanyProfileId, CreateBookingRequestDto dto)
        {
            try
            {
                if (dto.StartDateTime >= dto.EndDateTime)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Start time must be before end time.",
                        "وقت البداية يجب أن يكون قبل وقت النهاية."
                    );
                }

                if (dto.StartDateTime < DateTime.UtcNow)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Cannot book in the past.",
                        "لا يمكن الحجز في تاريخ ماضٍ."
                    );
                }

                var location = await _context.Locations
                    .FirstOrDefaultAsync(l =>
                        l.Id == dto.LocationId &&
                        l.IsActive &&
                        !l.IsDeleted);

                if (location == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Location not found or inactive.",
                        "الموقع غير موجود أو غير نشط."
                    );
                }

                var isFullDay = (dto.EndDateTime - dto.StartDateTime).TotalHours >= 4;
                var bookingDate = dto.StartDateTime.Date;

                var hasConflict = await _context.LocationBookingRequests
                    .Where(b =>
                        b.LocationId == dto.LocationId &&
                        b.IsActive &&
                        !b.IsDeleted &&
                        (b.BookingStatus.Name == "Pending" ||
                         b.BookingStatus.Name == "Accepted" ||
                         b.BookingStatus.Name == "Contract Created" ||
                         b.BookingStatus.Name == "Awaiting Contract Approval" ||
                         b.BookingStatus.Name == "Contract Signed" ||
                         b.BookingStatus.Name == "Payment Pending" ||
                         b.BookingStatus.Name == "Confirmed"))
                    .AnyAsync(b =>
                        (isFullDay && b.StartDateTime.Date == bookingDate) ||
                        (!isFullDay && b.StartDateTime.Date == bookingDate &&
                         (b.EndDateTime - b.StartDateTime).TotalHours >= 4) ||
                        (!isFullDay &&
                         b.StartDateTime.Date == bookingDate &&
                         b.StartDateTime < dto.EndDateTime &&
                         b.EndDateTime > dto.StartDateTime)
                    );

                if (hasConflict)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "This location is already booked for the selected time.",
                        "الموقع محجوز مسبقاً في هذا الوقت."
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
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "System error: status not found.",
                        "خطأ في النظام: الحالة غير موجودة."
                    );
                }

                var totalPrice = CalculateTotalPrice(location, dto.StartDateTime, dto.EndDateTime, isFullDay);

                var booking = new LocationBookingRequest
                {
                    LocationId = dto.LocationId,
                    StartDateTime = dto.StartDateTime,
                    EndDateTime = dto.EndDateTime,
                    Message = dto.Message,
                    ProductionCompanyId = productionCompanyProfileId,
                    LocationOwnerId = location.LocationOwnerId,
                    LocationManagerId = location.LocationManagerId,
                    BookingStatusId = pendingStatus.Id,
                    TotalPrice = totalPrice,
                    CreatedAt = DateTime.UtcNow
                };

                _context.LocationBookingRequests.Add(booking);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Booking request created. LocationId: {LocationId}, ProductionCompanyId: {ProductionCompanyId}",
                    dto.LocationId, productionCompanyProfileId
                );

                return ApiResponse<BookingRequestResponseDto>.SuccessResponse(
                    MapToDto(booking, location, isFullDay, pendingStatus.Name),
                    "Booking request sent successfully.",
                    "تم إرسال طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error creating booking request. LocationId: {LocationId}",
                    dto.LocationId
                );

                return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                    "An error occurred while creating the booking request.",
                    "حدث خطأ أثناء إنشاء طلب الحجز."
                );
            }
        }

        public async Task<ApiResponse<List<BookingRequestResponseDto>>> GetBookingRequestsAsync(int managerProfileId)
        {
            try
            {
                var requests = await _context.LocationBookingRequests
                    .Include(b => b.Location)
                    .Include(b => b.BookingStatus)
                    .Where(b =>
                        b.LocationManagerId == managerProfileId &&
                        b.IsActive &&
                        !b.IsDeleted)
                    .Select(b => new BookingRequestResponseDto
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
                        LocationManagerId = b.LocationManagerId,
                        CreatedAt = b.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<BookingRequestResponseDto>>.SuccessResponse(
                    requests,
                    "Booking requests retrieved successfully.",
                    "تم جلب طلبات الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error getting booking requests for manager {ManagerProfileId}",
                    managerProfileId
                );

                return ApiResponse<List<BookingRequestResponseDto>>.FailureResponse(
                    "An error occurred while retrieving booking requests.",
                    "حدث خطأ أثناء جلب طلبات الحجز."
                );
            }
        }

        public async Task<ApiResponse<BookingRequestResponseDto>> GetBookingRequestByIdAsync(int requestId, int managerProfileId)
        {
            try
            {
                var request = await _context.LocationBookingRequests
                    .Include(b => b.Location)
                    .Include(b => b.BookingStatus)
                    .FirstOrDefaultAsync(b =>
                        b.Id == requestId &&
                        b.LocationManagerId == managerProfileId &&
                        b.IsActive &&
                        !b.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Booking request not found.",
                        "طلب الحجز غير موجود."
                    );
                }

                return ApiResponse<BookingRequestResponseDto>.SuccessResponse(
                    MapToDto(request, request.Location),
                    "Booking request retrieved successfully.",
                    "تم جلب طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting booking request {RequestId}", requestId);

                return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                    "An error occurred while retrieving the booking request.",
                    "حدث خطأ أثناء جلب طلب الحجز."
                );
            }
        }

        public async Task<ApiResponse<BookingRequestResponseDto>> UpdateBookingRequestAsync(int requestId, int managerProfileId, UpdateBookingRequestDto dto)
        {
            try
            {
                var request = await _context.LocationBookingRequests
                    .Include(b => b.BookingStatus)
                    .Include(b => b.Location)
                    .FirstOrDefaultAsync(b =>
                        b.Id == requestId &&
                        b.LocationManagerId == managerProfileId &&
                        b.IsActive &&
                        !b.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Booking request not found.",
                        "طلب الحجز غير موجود."
                    );
                }

                if (request.BookingStatus.Name != "Pending")
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Only pending requests can be updated.",
                        "يمكن تعديل الطلبات المعلقة فقط."
                    );
                }

                var finalStart = dto.StartDateTime ?? request.StartDateTime;
                var finalEnd = dto.EndDateTime ?? request.EndDateTime;

                if (finalStart >= finalEnd)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Start time must be before end time.",
                        "وقت البداية يجب أن يكون قبل وقت النهاية."
                    );
                }

                if (finalStart < DateTime.UtcNow)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Cannot book in the past.",
                        "لا يمكن الحجز في تاريخ ماضٍ."
                    );
                }

                var isFullDay = (finalEnd - finalStart).TotalHours >= 4;
                var bookingDate = finalStart.Date;

                var hasConflict = await _context.LocationBookingRequests
                    .Where(b =>
                        b.LocationId == request.LocationId &&
                        b.Id != requestId &&
                        b.IsActive &&
                        !b.IsDeleted &&
                        (b.BookingStatus.Name == "Pending" ||
                         b.BookingStatus.Name == "Accepted" ||
                         b.BookingStatus.Name == "Contract Created" ||
                         b.BookingStatus.Name == "Awaiting Contract Approval" ||
                         b.BookingStatus.Name == "Contract Signed" ||
                         b.BookingStatus.Name == "Payment Pending" ||
                         b.BookingStatus.Name == "Confirmed"))
                    .AnyAsync(b =>
                        (isFullDay && b.StartDateTime.Date == bookingDate) ||
                        (!isFullDay && b.StartDateTime.Date == bookingDate &&
                         (b.EndDateTime - b.StartDateTime).TotalHours >= 4) ||
                        (!isFullDay &&
                         b.StartDateTime.Date == bookingDate &&
                         b.StartDateTime < finalEnd &&
                         b.EndDateTime > finalStart)
                    );

                if (hasConflict)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "This location is already booked for the selected time.",
                        "الموقع محجوز مسبقاً في هذا الوقت."
                    );
                }

                if (dto.StartDateTime.HasValue)
                    request.StartDateTime = dto.StartDateTime.Value;

                if (dto.EndDateTime.HasValue)
                    request.EndDateTime = dto.EndDateTime.Value;

                if (dto.Message is not null)
                    request.Message = dto.Message;

                if (dto.StartDateTime.HasValue || dto.EndDateTime.HasValue)
                    request.TotalPrice = CalculateTotalPrice(request.Location, finalStart, finalEnd, isFullDay);

                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking request {RequestId} updated", requestId);

                return ApiResponse<BookingRequestResponseDto>.SuccessResponse(
                    MapToDto(request, request.Location, isFullDay),
                    "Booking request updated successfully.",
                    "تم تعديل طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating booking request {RequestId}", requestId);

                return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                    "An error occurred while updating the booking request.",
                    "حدث خطأ أثناء تعديل طلب الحجز."
                );
            }
        }

        public async Task<ApiResponse<bool>> CancelBookingRequestAsync(int requestId, int managerProfileId)
        {
            try
            {
                var request = await _context.LocationBookingRequests
                    .Include(b => b.BookingStatus)
                    .FirstOrDefaultAsync(b =>
                        b.Id == requestId &&
                        b.LocationManagerId == managerProfileId &&
                        b.IsActive &&
                        !b.IsDeleted);

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

                request.IsDeleted = true;
                request.IsActive = false;
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation("Booking request {RequestId} cancelled", requestId);

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Booking request cancelled successfully.",
                    "تم إلغاء طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling booking request {RequestId}", requestId);

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the booking request.",
                    "حدث خطأ أثناء إلغاء طلب الحجز."
                );
            }
        }


        // Helper Methods
        private static decimal CalculateTotalPrice(Location location, DateTime start, DateTime end, bool isFullDay)
        {
            if (isFullDay)
                return location.DailyPrice;

            var hours = (decimal)(end - start).TotalHours;
            return hours * (location.HourlyPrice ?? 0m);
        }

        private static BookingRequestResponseDto MapToDto(
            LocationBookingRequest request,
            Location location,
            bool? isFullDay = null,
            string? statusName = null)
        {
            var fullDay = isFullDay ?? (request.EndDateTime - request.StartDateTime).TotalHours >= 4;

            return new BookingRequestResponseDto
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
                LocationManagerId = request.LocationManagerId,
                CreatedAt = request.CreatedAt
            };
        }
    }
}