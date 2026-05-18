using FilmMaker.Common;
using FilmMaker.DTO.LocationVisit;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationVisitService : ILocationVisitService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<LocationVisitService> _logger;

        public LocationVisitService(
            FilmMakerDbContext context,
            ILogger<LocationVisitService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<VisitRequestResponseDto>> CreateVisitRequestAsync(
            int managerProfileId,
            CreateVisitRequestDto dto)
        {
            try
            {
                if (dto.RequestedVisitDate < DateTime.UtcNow)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit date cannot be in the past.",
                        "لا يمكن تحديد تاريخ زيارة في الماضي."
                    );
                }

                var location = await _context.Locations
                    .FirstOrDefaultAsync(l =>
                        l.Id == dto.LocationId &&
                        l.IsActive &&
                        !l.IsDeleted);

                if (location == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Location not found.",
                        "الموقع غير موجود."
                    );
                }

                var locationOwner = await _context.LocationOwnerProfiles
                    .FirstOrDefaultAsync(o =>
                        o.Id == dto.LocationOwnerId &&
                        o.IsActive &&
                        !o.IsDeleted);

                if (locationOwner == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Location owner not found.",
                        "مالك الموقع غير موجود."
                    );
                }

                var locationManager = await _context.LocationManagerProfiles
                    .FirstOrDefaultAsync(m =>
                        m.Id == managerProfileId &&
                        m.IsActive &&
                        !m.IsDeleted);

                if (locationManager == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Location manager not found.",
                        "مدير الموقع غير موجود."
                    );
                }

                var pendingStatus = await _context.LookupItems
                    .Include(l => l.LookupCategory)
                    .FirstOrDefaultAsync(l =>
                        l.Name == "Pending" &&
                        l.LookupCategory.Name == "VisitStatus" &&
                        l.IsActive &&
                        !l.IsDeleted);

                if (pendingStatus == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit status configuration error.",
                        "خطأ في إعداد حالة الزيارة."
                    );
                }

                var hasPending = await _context.LocationVisitRequests
                    .AnyAsync(v =>
                        v.LocationId == dto.LocationId &&
                        v.LocationManagerId == managerProfileId &&
                        v.VisitStatusId == pendingStatus.Id &&
                        v.IsActive &&
                        !v.IsDeleted);

                if (hasPending)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "A pending visit request already exists for this location.",
                        "يوجد طلب زيارة معلق لهذا الموقع مسبقاً."
                    );
                }

                var visitRequest = new LocationVisitRequest
                {
                    LocationId = dto.LocationId,
                    LocationOwnerId = dto.LocationOwnerId,
                    LocationManagerId = managerProfileId,
                    RequestedVisitDateUtc = dto.RequestedVisitDate,
                    RequestMessage = dto.RequestMessage,
                    VisitStatusId = pendingStatus.Id,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                _context.LocationVisitRequests.Add(visitRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Visit request created. ManagerProfileId: {ManagerProfileId}, LocationId: {LocationId}",
                    managerProfileId,
                    dto.LocationId
                );

                var response = MapToDto(
                    visitRequest,
                    location,
                    pendingStatus.Name
                );

                return ApiResponse<VisitRequestResponseDto>.SuccessResponse(
                    response,
                    "Visit request created successfully.",
                    "تم إنشاء طلب الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating visit request. ManagerProfileId: {ManagerProfileId}",
                    managerProfileId
                );

                return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                    "An error occurred while creating the visit request.",
                    "حدث خطأ أثناء إنشاء طلب الزيارة."
                );
            }
        }

        public async Task<ApiResponse<List<VisitRequestResponseDto>>> GetVisitRequestsAsync(
            int managerProfileId)
        {
            try
            {
                var requests = await _context.LocationVisitRequests
                    .Include(v => v.Location)
                    .Include(v => v.VisitStatus)
                    .Where(v =>
                        v.LocationManagerId == managerProfileId &&
                        v.IsActive &&
                        !v.IsDeleted)
                    .OrderByDescending(v => v.CreatedAt)
                    .Select(v => new VisitRequestResponseDto
                    {
                        Id = v.Id,
                        LocationId = v.LocationId,
                        LocationName = v.Location.LocationName,
                        City = v.Location.City,
                        LocationOwnerId = v.LocationOwnerId,
                        LocationManagerId = v.LocationManagerId,
                        RequestedVisitDateUtc = v.RequestedVisitDateUtc,
                        RequestMessage = v.RequestMessage,
                        Status = v.VisitStatus.Name,
                        OwnerResponseMessage = v.OwnerResponseMessage,
                        RespondedAtUtc = v.RespondedAtUtc,
                        CreatedAt = v.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<VisitRequestResponseDto>>.SuccessResponse(
                    requests,
                    "Visit requests retrieved successfully.",
                    "تم جلب طلبات الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting visit requests for manager {ManagerProfileId}",
                    managerProfileId
                );

                return ApiResponse<List<VisitRequestResponseDto>>.FailureResponse(
                    "An error occurred while retrieving visit requests.",
                    "حدث خطأ أثناء جلب طلبات الزيارة."
                );
            }
        }

        public async Task<ApiResponse<VisitRequestResponseDto>> GetVisitRequestByIdAsync(
            int requestId,
            int managerProfileId)
        {
            try
            {
                var request = await _context.LocationVisitRequests
                    .Include(v => v.Location)
                    .Include(v => v.VisitStatus)
                    .FirstOrDefaultAsync(v =>
                        v.Id == requestId &&
                        v.LocationManagerId == managerProfileId &&
                        v.IsActive &&
                        !v.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit request not found.",
                        "طلب الزيارة غير موجود."
                    );
                }

                var response = MapToDto(
                    request,
                    request.Location,
                    request.VisitStatus.Name
                );

                return ApiResponse<VisitRequestResponseDto>.SuccessResponse(
                    response,
                    "Visit request retrieved successfully.",
                    "تم جلب طلب الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error getting visit request {RequestId}",
                    requestId
                );

                return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                    "An error occurred while retrieving the visit request.",
                    "حدث خطأ أثناء جلب طلب الزيارة."
                );
            }
        }

        public async Task<ApiResponse<VisitRequestResponseDto>> UpdateVisitRequestAsync(
            int requestId,
            int managerProfileId,
            UpdateVisitRequestDto dto)
        {
            try
            {
                if (dto.RequestedVisitDateUtc.HasValue &&
                    dto.RequestedVisitDateUtc < DateTime.UtcNow)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit date cannot be in the past.",
                        "لا يمكن تحديد تاريخ زيارة في الماضي."
                    );
                }

                var request = await _context.LocationVisitRequests
                    .Include(v => v.Location)
                    .Include(v => v.VisitStatus)
                    .FirstOrDefaultAsync(v =>
                        v.Id == requestId &&
                        v.LocationManagerId == managerProfileId &&
                        v.IsActive &&
                        !v.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit request not found.",
                        "طلب الزيارة غير موجود."
                    );
                }

                if (request.VisitStatus.Name != "Pending")
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Only pending visit requests can be updated.",
                        "يمكن تعديل طلبات الزيارة المعلقة فقط."
                    );
                }

                if (dto.RequestedVisitDateUtc.HasValue)
                {
                    request.RequestedVisitDateUtc =
                        dto.RequestedVisitDateUtc.Value;
                }

                if (dto.RequestMessage is not null)
                {
                    request.RequestMessage = dto.RequestMessage;
                }

                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Visit request {RequestId} updated",
                    requestId
                );

                var response = MapToDto(
                    request,
                    request.Location,
                    request.VisitStatus.Name
                );

                return ApiResponse<VisitRequestResponseDto>.SuccessResponse(
                    response,
                    "Visit request updated successfully.",
                    "تم تعديل طلب الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating visit request {RequestId}",
                    requestId
                );

                return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                    "An error occurred while updating the visit request.",
                    "حدث خطأ أثناء تعديل طلب الزيارة."
                );
            }
        }

        public async Task<ApiResponse<bool>> CancelVisitRequestAsync(
            int requestId,
            int managerProfileId)
        {
            try
            {
                var request = await _context.LocationVisitRequests
                    .Include(v => v.VisitStatus)
                    .FirstOrDefaultAsync(v =>
                        v.Id == requestId &&
                        v.LocationManagerId == managerProfileId &&
                        v.IsActive &&
                        !v.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Visit request not found.",
                        "طلب الزيارة غير موجود."
                    );
                }

                if (request.VisitStatus.Name != "Pending")
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending visit requests can be cancelled.",
                        "يمكن إلغاء طلبات الزيارة المعلقة فقط."
                    );
                }

                request.IsDeleted = true;
                request.IsActive = false;
                request.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Visit request {RequestId} cancelled by manager {ManagerProfileId}",
                    requestId,
                    managerProfileId
                );

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Visit request cancelled successfully.",
                    "تم إلغاء طلب الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cancelling visit request {RequestId}",
                    requestId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the visit request.",
                    "حدث خطأ أثناء إلغاء طلب الزيارة."
                );
            }
        }

        private static VisitRequestResponseDto MapToDto(
            LocationVisitRequest request,
            Location location,
            string statusName)
        {
            return new VisitRequestResponseDto
            {
                Id = request.Id,
                LocationId = request.LocationId,
                LocationName = location.LocationName,
                City = location.City,
                LocationOwnerId = request.LocationOwnerId,
                LocationManagerId = request.LocationManagerId,
                RequestedVisitDateUtc = request.RequestedVisitDateUtc,
                RequestMessage = request.RequestMessage,
                Status = statusName,
                OwnerResponseMessage = request.OwnerResponseMessage,
                RespondedAtUtc = request.RespondedAtUtc,
                CreatedAt = request.CreatedAt
            };
        }
    }
}