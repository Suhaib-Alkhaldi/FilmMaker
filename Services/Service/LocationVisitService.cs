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

        public async Task<ApiResponse<VisitRequestResponseDto>> CreateVisitRequestAsync(int managerProfileId,CreateVisitRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                if (dto.RequestedVisitDate <= DateTime.UtcNow)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit date must be in the future.",
                        "يجب أن يكون تاريخ الزيارة في المستقبل."
                    );
                }

                var location = await _context.Locations
                    .Include(l => l.LocationOwner)
                    .ThenInclude(o => o.User)
                    .Where(l =>
                           l.Id == dto.LocationId &&
                           l.IsActive &&
                           !l.IsDeleted).FirstOrDefaultAsync();

                if (location == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Location not found.",
                        "الموقع غير موجود."
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

                const int MinimumVisitRequestGapHours = 3;

                var requestedVisitDateUtc = dto.RequestedVisitDate;

                var hasPendingRequestForSameManager = await _context.LocationVisitRequests
                    .AnyAsync(v =>
                        v.LocationId == dto.LocationId &&
                        v.LocationManagerId == managerProfileId &&
                        v.VisitStatusId == pendingStatus.Id &&
                        v.IsActive &&
                        !v.IsDeleted);

                if (hasPendingRequestForSameManager)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "You already have a pending visit request for this location.",
                        "لديك طلب زيارة معلق لهذا الموقع مسبقًا."
                    );
                }

                var hasConflictingPendingRequestForSameLocation = await _context.LocationVisitRequests
                    .AnyAsync(v =>
                        v.LocationId == dto.LocationId &&
                        v.LocationManagerId != managerProfileId &&
                        v.VisitStatusId == pendingStatus.Id &&
                        v.IsActive &&
                        !v.IsDeleted &&
                        v.RequestedVisitDateUtc > requestedVisitDateUtc.AddHours(-MinimumVisitRequestGapHours) &&
                        v.RequestedVisitDateUtc < requestedVisitDateUtc.AddHours(MinimumVisitRequestGapHours));

                if (hasConflictingPendingRequestForSameLocation)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Another pending visit request already exists within 3 hours for this location.",
                        "يوجد طلب زيارة معلق من مدير آخر لهذا الموقع ضمن فترة أقل من 3 ساعات."
                    );
                }

                var visitRequest = new LocationVisitRequest
                {
                    LocationId = location.Id,
                    LocationManagerId = managerProfileId,
                    RequestedVisitDateUtc = requestedVisitDateUtc,
                    RequestMessage = string.IsNullOrWhiteSpace(dto.RequestMessage)
                        ? null
                        : dto.RequestMessage.Trim(),
                    VisitStatusId = pendingStatus.Id,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = managerProfileId.ToString(),
                    IsActive = true,
                    IsDeleted = false
                };

                _context.LocationVisitRequests.Add(visitRequest);
                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Visit request created. ManagerProfileId: {ManagerProfileId}, LocationId: {LocationId}, RequestedVisitDateUtc: {RequestedVisitDateUtc}",
                    managerProfileId,
                    location.Id,
                    requestedVisitDateUtc
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

        public async Task<ApiResponse<List<VisitRequestResponseDto>>> GetVisitRequestsAsync(int currentUserId)
        {
            try
            {
                var managerProfileId = await _context.LocationManagerProfiles
                    .Where(m =>
                        m.UserId == currentUserId &&
                        m.IsActive &&
                        !m.IsDeleted)
                    .Select(m => (int?)m.Id)
                    .FirstOrDefaultAsync();

                if (managerProfileId == null || managerProfileId.Value <= 0)
                {
                    return ApiResponse<List<VisitRequestResponseDto>>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var requests = await _context.LocationVisitRequests
                    .Where(v =>
                        v.LocationManagerId == managerProfileId.Value &&
                        v.IsActive &&
                        !v.IsDeleted)
                    .OrderByDescending(v => v.CreatedAt)
                    .Select(v => new VisitRequestResponseDto
                    {
                        Id = v.Id,
                        LocationId = v.LocationId,
                        LocationName = v.Location.LocationName,
                        City = v.Location.City,

                        LocationOwnerId = v.Location.LocationOwnerId,
                        LocationOwnerName = v.Location.LocationOwner.User.Name,

                        LocationManagerId = v.LocationManagerId,
                        RequestedVisitDateUtc = v.RequestedVisitDateUtc,
                        RequestMessage = v.RequestMessage,

                        Status = v.VisitStatus.Name,

                        OwnerResponseMessage = v.OwnerResponseMessage,
                        RespondedAtUtc = v.RespondedAtUtc,
                        CreatedAt = v.CreatedAt
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<VisitRequestResponseDto>>.SuccessResponse(
                        requests,
                        "No visit requests found.",
                        "لا توجد طلبات زيارة."
                    );
                }

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
                    "Error getting visit requests for current user {CurrentUserId}",
                    currentUserId
                );

                return ApiResponse<List<VisitRequestResponseDto>>.FailureResponse(
                    "An error occurred while retrieving visit requests.",
                    "حدث خطأ أثناء جلب طلبات الزيارة."
                );
            }
        }

        public async Task<ApiResponse<VisitRequestResponseDto>> GetVisitRequestByIdAsync(int requestId,int currentUserId)
        {
            try
            {
                var managerProfileId = await _context.LocationManagerProfiles
                    .Where(m =>
                        m.UserId == currentUserId &&
                        m.IsActive &&
                        !m.IsDeleted)
                    .Select(m => (int?)m.Id)
                    .FirstOrDefaultAsync();

                if (managerProfileId == null || managerProfileId.Value <= 0)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var request = await _context.LocationVisitRequests
                    .Include(v => v.Location)
                        .ThenInclude(l => l.LocationOwner)
                            .ThenInclude(o => o.User)
                    .Include(v => v.LocationManager)
                        .ThenInclude(m => m.User)
                    .Include(v => v.VisitStatus)
                    .FirstOrDefaultAsync(v =>
                        v.Id == requestId &&
                        v.LocationManagerId == managerProfileId.Value &&
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

        public async Task<ApiResponse<VisitRequestResponseDto>> UpdateVisitRequestAsync(int currentUserId,UpdateVisitRequestDto dto)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                if (dto.RequestedVisitDateUtc.HasValue &&
                    dto.RequestedVisitDateUtc.Value <= DateTime.UtcNow)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit date must be in the future.",
                        "يجب أن يكون تاريخ الزيارة في المستقبل."
                    );
                }

                var managerProfileId = await _context.LocationManagerProfiles
                    .Where(m =>
                        m.UserId == currentUserId &&
                        m.IsActive &&
                        !m.IsDeleted)
                    .Select(m => (int?)m.Id)
                    .FirstOrDefaultAsync();

                if (managerProfileId == null || managerProfileId.Value <= 0)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var request = await _context.LocationVisitRequests
                    .Include(v => v.VisitStatus)
                    .FirstOrDefaultAsync(v =>
                        v.Id == dto.RequestId &&
                        v.LocationManagerId == managerProfileId.Value &&
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
                    request.RequestedVisitDateUtc = dto.RequestedVisitDateUtc.Value;
                }

                if (dto.RequestMessage != null)
                {
                    request.RequestMessage = string.IsNullOrWhiteSpace(dto.RequestMessage)
                        ? null
                        : dto.RequestMessage.Trim();
                }

                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                var response = await _context.LocationVisitRequests
                    .Where(v =>
                        v.Id == request.Id &&
                        v.LocationManagerId == managerProfileId.Value &&
                        v.IsActive &&
                        !v.IsDeleted)
                    .Select(v => new VisitRequestResponseDto
                    {
                        Id = v.Id,

                        LocationId = v.LocationId,
                        LocationName = v.Location.LocationName,
                        City = v.Location.City,

                        LocationOwnerId = v.Location.LocationOwnerId,
                        LocationOwnerName = v.Location.LocationOwner.User.Name,

                        LocationManagerId = v.LocationManagerId,

                        RequestedVisitDateUtc = v.RequestedVisitDateUtc,
                        RequestMessage = v.RequestMessage,

                        Status = v.VisitStatus.Name,

                        OwnerResponseMessage = v.OwnerResponseMessage,
                        RespondedAtUtc = v.RespondedAtUtc,
                        CreatedAt = v.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (response == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit request was updated, but response could not be loaded.",
                        "تم تعديل طلب الزيارة، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                _logger.LogInformation(
                    "Visit request {RequestId} updated by UserId {UserId}",
                    dto.RequestId,
                    currentUserId
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
                    dto?.RequestId
                );

                return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                    "An error occurred while updating the visit request.",
                    "حدث خطأ أثناء تعديل طلب الزيارة."
                );
            }
        }

        public async Task<ApiResponse<bool>> CancelVisitRequestAsync(int requestId,int managerProfileId)
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

                var cancelledStatus = await _context.LookupItems
                    .Include(x => x.LookupCategory)
                    .Where(x =>
                x.Name == "Cancelled" &&
                x.LookupCategory.Name == "VisitStatus" &&
                x.IsActive &&
                !x.IsDeleted).FirstOrDefaultAsync();

                if (cancelledStatus == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Cancelled visit status was not found in lookup data.",
                        "حالة إلغاء طلب الزيارة غير موجودة في بيانات النظام."
                    );
                }

                
                request.IsDeleted = true;
                request.IsActive = false;
                request.VisitStatusId = cancelledStatus.Id;
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
                LocationOwnerId = request.Location.LocationOwnerId,
                LocationOwnerName = location.LocationOwner.User.Name,
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