using FilmMaker.Common;
using FilmMaker.DTO.LocationManager;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace FilmMaker.Services.Service
{
    public class LocationManagementService : ILocationManagementServicee
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<LocationManagementService> _logger;

        public LocationManagementService(
            FilmMakerDbContext context,
            ILogger<LocationManagementService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<List<LocationSummaryDto>>> GetManagedLocationsAsync(int managerId)
        {
            try
            {
                var locations = await _context.Locations
                    .Include(l => l.LocationStatus)
                    .Include(l => l.Media)
                    .Where(l =>
                        l.LocationManagerId == managerId &&
                        l.IsActive &&
                        !l.IsDeleted)
                    .Select(l => new LocationSummaryDto
                    {
                        Id = l.Id,
                        LocationName = l.LocationName,
                        City = l.City,
                        Address = l.Address,
                        DailyPrice = l.DailyPrice,
                        LocationStatus = l.LocationStatus.Name,
                        MediaUrl = l.Media.FirstOrDefault() != null
                            ? l.Media.First().FileUrl
                            : null,
                        HasManager = true
                    })
                    .ToListAsync();

                return ApiResponse<List<LocationSummaryDto>>.SuccessResponse(
                    locations,
                    "Managed locations retrieved successfully.",
                    "تم جلب المواقع التي يديرها المستخدم بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while getting managed locations. ManagerId: {ManagerId}",
                    managerId
                );

                return ApiResponse<List<LocationSummaryDto>>.FailureResponse(
                    "An error occurred while retrieving managed locations.",
                    "حدث خطأ أثناء جلب المواقع المدارة."
                );
            }
        }

        public async Task<ApiResponse<List<ManagementRequestResponseDto>>> GetMyRequestsAsync(int userId)
        {
            try
            {
                var managerProfileId = await _context.Users
                    .Where(u => u.Id == userId)
                    .Select(u => u.LocationManagerProfile.Id)
                    .FirstOrDefaultAsync();

                if (managerProfileId == 0)
                {
                    return ApiResponse<List<ManagementRequestResponseDto>>.FailureResponse(
                        "Manager profile not found.",
                        "لم يتم العثور على ملف المدير."
                    );
                }

                var requests = await _context.LocationManagementRequests
                    .Include(r => r.Location)
                    .Where(r => r.LocationManagerProfileId == managerProfileId)
                    .Select(r => new ManagementRequestResponseDto
                    {
                        Id = r.Id,
                        LocationId = r.LocationId,
                        LocationName = r.Location.LocationName,
                        City = r.Location.City,
                        Message = r.Message,
                        Status = r.Status,
                        CreatedAt = r.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<ManagementRequestResponseDto>>.SuccessResponse(
                    requests,
                    "Requests retrieved successfully.",
                    "تم جلب الطلبات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while getting manager requests for user {UserId}", userId);

                return ApiResponse<List<ManagementRequestResponseDto>>.FailureResponse(
                    "Failed to retrieve requests.",
                    "حدث خطأ أثناء جلب الطلبات."
                );
            }
        }

        public async Task<ApiResponse<ManagementRequestResponseDto>> SendManageRequestAsync(
            int managerProfileId,
            ManagementRequestDto dto)
        {
            if (dto == null)
            {
                return ApiResponse<ManagementRequestResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            try
            {
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l =>
                        l.Id == dto.LocationId &&
                        l.IsActive &&
                        !l.IsDeleted);

                if (location == null)
                {
                    return ApiResponse<ManagementRequestResponseDto>.FailureResponse(
                        "Location not found or inactive.",
                        "الموقع غير موجود أو غير نشط."
                    );
                }

                if (location.LocationManagerId != null)
                {
                    return ApiResponse<ManagementRequestResponseDto>.FailureResponse(
                        "This location already has a manager.",
                        "هذا الموقع لديه مدير مرتبط مسبقاً."
                    );
                }

                var hasPendingRequest = await _context.LocationManagementRequests
                    .AnyAsync(r =>
                        r.LocationManagerProfileId == managerProfileId &&
                        r.LocationId == dto.LocationId &&
                        r.Status == "Pending");

                if (hasPendingRequest)
                {
                    return ApiResponse<ManagementRequestResponseDto>.FailureResponse(
                        "A pending request already exists for this location.",
                        "يوجد طلب معلق لهذا الموقع مسبقاً."
                    );
                }

                var request = new LocationManagementRequest
                {
                    LocationId = dto.LocationId,
                    LocationManagerProfileId = managerProfileId,
                    Message = string.IsNullOrWhiteSpace(dto.Message)
                        ? null
                        : dto.Message.Trim(),
                    Status = "Pending",
                    CreatedAt = DateTime.UtcNow
                };

                _context.LocationManagementRequests.Add(request);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Management request created successfully. ManagerProfileId: {ManagerProfileId}, LocationId: {LocationId}",
                    managerProfileId,
                    dto.LocationId
                );

                var response = new ManagementRequestResponseDto
                {
                    Id = request.Id,
                    LocationId = request.LocationId,
                    LocationName = location.LocationName,
                    City = location.City,
                    Message = request.Message,
                    Status = request.Status,
                    CreatedAt = request.CreatedAt
                };

                return ApiResponse<ManagementRequestResponseDto>.SuccessResponse(
                    response,
                    "Management request sent successfully.",
                    "تم إرسال طلب الإدارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while sending management request. ManagerProfileId: {ManagerProfileId}, LocationId: {LocationId}",
                    managerProfileId,
                    dto.LocationId
                );

                return ApiResponse<ManagementRequestResponseDto>.FailureResponse(
                    "An unexpected error occurred while sending the request.",
                    "حدث خطأ غير متوقع أثناء إرسال الطلب."
                );
            }
        }
    }
}