using FilmMaker.Common;
using FilmMaker.DTO.Booking;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationOwnerBookingRequestService : ILocationOwnerBookingRequestService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<LocationOwnerBookingRequestService> _logger;

        public LocationOwnerBookingRequestService(FilmMakerDbContext context,ILogger<LocationOwnerBookingRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<BookingRequestResponseDto>>> GetReceivedBookingRequests(int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<List<BookingRequestResponseDto>>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var requests = await _context.LocationBookingRequests
                    .Where(x =>
                        x.LocationOwnerId == locationOwnerId.Value &&
                        !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new BookingRequestResponseDto
                    {
                        BookingRequestId = x.Id,

                        LocationId = x.LocationId,
                        LocationName = x.Location.LocationName,

                        LocationOwnerId = x.LocationOwnerId,
                        LocationOwnerName = x.LocationOwner.User.Name,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany != null
                            ? x.ProductionCompany.User.Name
                            : null,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : null,

                        RequestedByType = x.ProductionCompanyId != null
                            ? "ProductionCompany"
                            : "LocationManager",

                        RequestedByName = x.ProductionCompanyId != null
                            ? x.ProductionCompany!.User.Name
                            : x.LocationManager != null
                                ? x.LocationManager.User.Name
                                : string.Empty,

                        BookingStatusId = x.BookingStatusId,
                        BookingStatus = x.BookingStatus.Name,

                        Message = x.Message,
                        TotalPrice = x.TotalPrice,
                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<BookingRequestResponseDto>>.SuccessResponse(
                        requests,
                        "No booking requests found.",
                        "لا توجد طلبات حجز."
                    );
                }

                return ApiResponse<List<BookingRequestResponseDto>>.SuccessResponse(
                    requests,
                    "Booking requests fetched successfully.",
                    "تم جلب طلبات الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching received booking requests. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<List<BookingRequestResponseDto>>.FailureResponse(
                    "An unexpected error occurred while fetching booking requests.",
                    "حدث خطأ غير متوقع أثناء جلب طلبات الحجز."
                );
            }
        }

        public async Task<ApiResponse<BookingRequestResponseDto>> GetReceivedBookingRequestById(int bookingRequestId,int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var bookingRequest = await _context.LocationBookingRequests
                    .Where(x =>
                        x.Id == bookingRequestId &&
                        x.LocationOwnerId == locationOwnerId.Value &&
                        !x.IsDeleted)
                    .Select(x => new BookingRequestResponseDto
                    {
                        BookingRequestId = x.Id,

                        LocationId = x.LocationId,
                        LocationName = x.Location.LocationName,

                        LocationOwnerId = x.LocationOwnerId,
                        LocationOwnerName = x.LocationOwner.User.Name,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany != null
                            ? x.ProductionCompany.User.Name
                            : null,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : null,

                        RequestedByType = x.ProductionCompanyId != null
                            ? "ProductionCompany"
                            : "LocationManager",

                        RequestedByName = x.ProductionCompanyId != null
                            ? x.ProductionCompany!.User.Name
                            : x.LocationManager != null
                                ? x.LocationManager.User.Name
                                : string.Empty,

                        BookingStatusId = x.BookingStatusId,
                        BookingStatus = x.BookingStatus.Name,

                        StartDateTime = x.StartDateTime,
                        EndDateTime = x.EndDateTime,
                        IsFullDay = (x.EndDateTime - x.StartDateTime).TotalHours >= 4,
                        TotalPrice = x.TotalPrice,
                        CreatedAt = x.CreatedAt
                    }).FirstOrDefaultAsync();

                if (bookingRequest == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Booking request was not found.",
                        "لم يتم العثور على طلب الحجز."
                    );
                }

                return ApiResponse<BookingRequestResponseDto>.SuccessResponse(
                    bookingRequest,
                    "Booking request fetched successfully.",
                    "تم جلب طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching booking request by id. BookingRequestId: {BookingRequestId}, UserId: {UserId}",
                    bookingRequestId,
                    currentUserId
                );

                return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                    "An unexpected error occurred while fetching the booking request.",
                    "حدث خطأ غير متوقع أثناء جلب طلب الحجز."
                );
            }
        }

        public async Task<ApiResponse<BookingRequestResponseDto>> RespondBookingRequest(RespondBookingRequestDto request,int currentUserId)
        {
            if (request == null)
            {
                return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var pendingStatusId = await GetStatusId("BookingStatus", "Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Pending booking status was not found in lookup data.",
                        "حالة الحجز قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                var targetStatusName = request.IsAccepted ? "Accepted" : "Rejected";

                var targetStatusId = await GetStatusId("BookingStatus", targetStatusName);

                if (targetStatusId == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        $"{targetStatusName} booking status was not found in lookup data.",
                        "حالة الحجز المطلوبة غير موجودة في بيانات النظام."
                    );
                }

                var bookingRequest = await _context.LocationBookingRequests
                    .Include(x => x.BookingStatus)
                    .Where(x =>
                        x.Id == request.BookingRequestId &&
                        x.LocationOwnerId == locationOwnerId.Value &&
                        !x.IsDeleted).FirstOrDefaultAsync();

                if (bookingRequest == null)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Booking request was not found.",
                        "لم يتم العثور على طلب الحجز."
                    );
                }

                if (bookingRequest.BookingStatusId != pendingStatusId.Value)
                {
                    return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                        "Only pending booking requests can be responded to.",
                        "يمكن الرد فقط على طلبات الحجز قيد الانتظار."
                    );
                }

                var oldStatusId = bookingRequest.BookingStatusId;

                bookingRequest.BookingStatusId = targetStatusId.Value;
                bookingRequest.UpdatedBy = currentUserId.ToString();
                bookingRequest.UpdatedAt = DateTime.UtcNow;

                var statusHistory = new BookingStatusHistory
                {
                    LocationBookingRequestId = bookingRequest.Id,
                    FromStatusId = oldStatusId,
                    ToStatusId = targetStatusId.Value,
                    ChangedByUserId = currentUserId,
                    ChangedAt = DateTime.UtcNow,
                    Reason = string.IsNullOrWhiteSpace(request.ResponseMessage)
                        ? null
                        : request.ResponseMessage.Trim(),
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.BookingStatusHistories.Add(statusHistory);

                await _context.SaveChangesAsync();

                var response = await GetReceivedBookingRequestById(request.BookingRequestId,currentUserId);

                if (!response.Success)
                    return response;

                return ApiResponse<BookingRequestResponseDto>.SuccessResponse(
                    response.Data!,
                    request.IsAccepted
                        ? "Booking request accepted successfully."
                        : "Booking request rejected successfully.",
                    request.IsAccepted
                        ? "تم قبول طلب الحجز بنجاح."
                        : "تم رفض طلب الحجز بنجاح."
                );
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Error while responding to booking request. BookingRequestId: {BookingRequestId}, UserId: {UserId}",
                    request.BookingRequestId,
                    currentUserId
                );

                return ApiResponse<BookingRequestResponseDto>.FailureResponse(
                    "An unexpected error occurred while responding to the booking request.",
                    "حدث خطأ غير متوقع أثناء الرد على طلب الحجز."
                );
            }
        }

        private async Task<int?> GetLocationOwnerIdByUserId(int currentUserId)
        {
            return await _context.LocationOwnerProfiles
                .Where(x =>
                    x.UserId == currentUserId &&
                    !x.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        private async Task<int?> GetStatusId(string categoryName, string statusName)
        {
            return await _context.LookupItems
                .Where(x =>
                    x.Name == statusName &&
                    x.LookupCategory.Name == categoryName &&
                    !x.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
