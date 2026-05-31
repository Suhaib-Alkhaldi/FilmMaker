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

        public async Task<ApiResponse<VisitRequestResponseDto>> CreateVisitRequestAsync(int currentUserId,CreateVisitRequestDto dto)
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

                var location = await _context.Locations.Where(l =>
                        l.Id == dto.LocationId &&
                        l.IsActive &&
                        !l.IsDeleted).SingleOrDefaultAsync();

                if (location == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Location not found.",
                        "الموقع غير موجود."
                    );
                }

                var pendingStatusId = await GetStatus("VisitStatus", "Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit status configuration error.",
                        "خطأ في إعداد حالة الزيارة."
                    );
                }

                var requestedVisitDateUtc = dto.RequestedVisitDate;

                var hasPendingRequestForSameManager = await _context.LocationVisitRequests
                    .AnyAsync(v =>
                        v.LocationId == location.Id &&
                        v.LocationManagerId == managerProfileId.Value &&
                        v.VisitStatusId == pendingStatusId.Value &&
                        v.IsActive &&
                        !v.IsDeleted);

                if (hasPendingRequestForSameManager)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "You already have a pending visit request for this location.",
                        "لديك طلب زيارة معلق لهذا الموقع مسبقًا."
                    );
                }

                var visitRequest = new LocationVisitRequest
                {
                    LocationId = location.Id,
                    LocationManagerId = managerProfileId.Value,
                    RequestedVisitDateUtc = requestedVisitDateUtc,
                    RequestMessage = string.IsNullOrWhiteSpace(dto.RequestMessage)
                        ? null
                        : dto.RequestMessage.Trim(),
                    VisitStatusId = pendingStatusId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false
                };

                _context.LocationVisitRequests.Add(visitRequest);
                await _context.SaveChangesAsync();

                var response = await _context.LocationVisitRequests
                    .Where(v =>
                        v.Id == visitRequest.Id &&
                        v.IsActive &&
                        !v.IsDeleted)
                    .Select(v => new VisitRequestResponseDto
                    {
                        Id = v.Id,

                        LocationId = v.LocationId,
                        LocationName = v.Location.LocationName,
                        City = v.Location.City,

                        LocationOwnerId = v.Location.LocationOwnerId,
                        LocationOwnerName = v.Location.LocationOwner != null
                            ? v.Location.LocationOwner.User.Name
                            : string.Empty,

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
                        "Visit request was created, but response could not be loaded.",
                        "تم إنشاء طلب الزيارة، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                _logger.LogInformation(
                    "Visit request created. UserId: {UserId}, ManagerProfileId: {ManagerProfileId}, LocationId: {LocationId}, RequestedVisitDateUtc: {RequestedVisitDateUtc}",
                    currentUserId,
                    managerProfileId.Value,
                    location.Id,
                    requestedVisitDateUtc
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
                    "Error creating visit request. UserId: {UserId}",
                    currentUserId
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

                var response = await _context.LocationVisitRequests
                    .Where(v =>
                        v.Id == requestId &&
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
                        LocationOwnerName = v.Location.LocationOwner != null
                            ? v.Location.LocationOwner.User.Name
                            : string.Empty,

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
                        "Visit request not found.",
                        "طلب الزيارة غير موجود."
                    );
                }

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
                    "Error getting visit request {RequestId} for UserId {UserId}",
                    requestId,
                    currentUserId
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

                var pendingStatusId = await GetStatus("VisitStatus", "Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Pending visit request status was not found in lookup data.",
                        "حالة طلب الزيارة قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                var request = await _context.LocationVisitRequests
                    .Where(v =>
                        v.Id == dto.RequestId &&
                        v.LocationManagerId == managerProfileId.Value &&
                        v.IsActive &&
                        !v.IsDeleted).SingleOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit request not found.",
                        "طلب الزيارة غير موجود."
                    );
                }

                if (request.VisitStatusId != pendingStatusId.Value)
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
                        LocationOwnerName = v.Location.LocationOwner != null
                            ? v.Location.LocationOwner.User.Name
                            : string.Empty,

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

        public async Task<ApiResponse<bool>> CancelVisitRequestAsync(int requestId,int currentUserId)
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
                    return ApiResponse<bool>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var pendingStatusId = await GetStatus("VisitStatus", "Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Pending visit status was not found in lookup data.",
                        "حالة طلب الزيارة قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                var cancelledStatusId = await GetStatus("VisitStatus", "Cancelled");

                if (cancelledStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Cancelled visit status was not found in lookup data.",
                        "حالة إلغاء طلب الزيارة غير موجودة في بيانات النظام."
                    );
                }

                var request = await _context.LocationVisitRequests
                    .FirstOrDefaultAsync(v =>
                        v.Id == requestId &&
                        v.LocationManagerId == managerProfileId.Value &&
                        v.IsActive &&
                        !v.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Visit request not found.",
                        "طلب الزيارة غير موجود."
                    );
                }

                if (request.VisitStatusId != pendingStatusId.Value)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending visit requests can be cancelled.",
                        "يمكن إلغاء طلبات الزيارة المعلقة فقط."
                    );
                }

                request.VisitStatusId = cancelledStatusId.Value;
                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Visit request {RequestId} cancelled by UserId {UserId}, ManagerProfileId {ManagerProfileId}",
                    requestId,
                    currentUserId,
                    managerProfileId.Value
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
                    "Error cancelling visit request {RequestId} for UserId {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the visit request.",
                    "حدث خطأ أثناء إلغاء طلب الزيارة."
                );
            }
        }





        private async Task<int?> GetStatus(string categoryName, string statusName)
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