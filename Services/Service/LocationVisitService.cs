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

                var productionCompanyId = await _context.ProductionCompanyProfiles
                    .Where(p =>
                        p.UserId == currentUserId &&
                        p.IsActive &&
                        !p.IsDeleted)
                    .Select(p => (int?)p.Id)
                    .FirstOrDefaultAsync();

                var isLocationManager = managerProfileId.HasValue;
                var isProductionCompany = productionCompanyId.HasValue;

                if (isLocationManager == isProductionCompany)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "User must be either a location manager or a production company.",
                        "يجب أن يكون المستخدم مدير موقع أو شركة إنتاج."
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

                bool hasPendingRequest;

                if (isLocationManager)
                {
                    hasPendingRequest = await _context.LocationVisitRequests
                        .AnyAsync(v =>
                            v.LocationId == location.Id &&
                            v.LocationManagerId == managerProfileId.Value &&
                            v.VisitStatusId == pendingStatusId.Value &&
                            v.IsActive &&
                            !v.IsDeleted);
                }
                else
                {
                    hasPendingRequest = await _context.LocationVisitRequests
                        .AnyAsync(v =>
                            v.LocationId == location.Id &&
                            v.ProductionCompanyId == productionCompanyId.Value &&
                            v.VisitStatusId == pendingStatusId.Value &&
                            v.IsActive &&
                            !v.IsDeleted);
                }

                if (hasPendingRequest)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "You already have a pending visit request for this location.",
                        "لديك طلب زيارة معلق لهذا الموقع مسبقًا."
                    );
                }

                var visitRequest = new LocationVisitRequest
                {
                    LocationId = location.Id,

                    LocationManagerId = isLocationManager
        ? managerProfileId.Value
        : null,

                    ProductionCompanyId = isProductionCompany
        ? productionCompanyId.Value
        : null,

                    RequestedByUserId = currentUserId,

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

                        LocationOwnerName =
                            v.Location.LocationOwner != null &&
                            v.Location.LocationOwner.User != null
                                ? v.Location.LocationOwner.User.Name
                                : string.Empty,

                        LocationManagerId = v.LocationManagerId,

                        LocationManagerName =
                            v.LocationManager != null &&
                            v.LocationManager.User != null
                                ? v.LocationManager.User.Name
                                : null,

                        ProductionCompanyId = v.ProductionCompanyId,

                        ProductionCompanyName =
                            v.ProductionCompany != null &&
                            v.ProductionCompany.User != null
                                ? v.ProductionCompany.User.Name
                                : null,

                        RequestedByUserId = v.RequestedByUserId,

                        RequesterType = v.LocationManagerId.HasValue
                            ? "LocationManager"
                            : "ProductionCompany",

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
    "Visit request created. UserId: {UserId}, RequesterType: {RequesterType}, ManagerProfileId: {ManagerProfileId}, ProductionCompanyId: {ProductionCompanyId}, LocationId: {LocationId}, RequestedVisitDateUtc: {RequestedVisitDateUtc}",
    currentUserId,
    isLocationManager ? "LocationManager" : "ProductionCompany",
    managerProfileId,
    productionCompanyId,
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
                var locationManagerId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var productionCompanyId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isLocationManager = locationManagerId.HasValue;
                var isProductionCompany = productionCompanyId.HasValue;

                if (isLocationManager == isProductionCompany)
                {
                    return ApiResponse<List<VisitRequestResponseDto>>.FailureResponse(
                        "User must be either a location manager or a production company.",
                        "يجب أن يكون المستخدم مدير موقع أو شركة إنتاج."
                    );
                }

                var requests = await _context.LocationVisitRequests
                    .Where(v =>
                        (v.LocationManagerId == currentUserId || v.ProductionCompanyId == currentUserId) &&
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

                        LocationOwnerName =
                            v.Location.LocationOwner != null &&
                            v.Location.LocationOwner.User != null
                                ? v.Location.LocationOwner.User.Name
                                : string.Empty,

                        LocationManagerId = v.LocationManagerId,

                        LocationManagerName =
                            v.LocationManager != null &&
                            v.LocationManager.User != null
                                ? v.LocationManager.User.Name
                                : null,

                        ProductionCompanyId = v.ProductionCompanyId,

                        ProductionCompanyName =
                            v.ProductionCompany != null &&
                            v.ProductionCompany.User != null
                                ? v.ProductionCompany.User.Name
                                : null,

                        RequestedByUserId = v.RequestedByUserId,

                        RequesterType = v.LocationManagerId.HasValue
                            ? "LocationManager"
                            : "ProductionCompany",

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
                var locationManagerId = await _context.LocationManagerProfiles
                     .Where(x =>
                         x.UserId == currentUserId &&
                         x.IsActive &&
                         !x.IsDeleted)
                     .Select(x => (int?)x.Id)
                     .FirstOrDefaultAsync();

                var productionCompanyId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isLocationManager = locationManagerId.HasValue;
                var isProductionCompany = productionCompanyId.HasValue;

                if (isLocationManager == isProductionCompany)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "User must be either a location manager or a production company.",
                        "يجب أن يكون المستخدم مدير موقع أو شركة إنتاج."
                    );
                }

                var response = await _context.LocationVisitRequests
                    .Where(v =>
                        v.Id == requestId &&
                        (v.LocationManagerId == currentUserId || v.ProductionCompanyId == currentUserId) &&
                        v.IsActive &&
                        !v.IsDeleted)
                    .Select(v => new VisitRequestResponseDto
                    {
                        Id = v.Id,

                        LocationId = v.LocationId,
                        LocationName = v.Location.LocationName,
                        City = v.Location.City,

                        LocationOwnerId = v.Location.LocationOwnerId,

                        LocationOwnerName =
                            v.Location.LocationOwner != null &&
                            v.Location.LocationOwner.User != null
                                ? v.Location.LocationOwner.User.Name
                                : string.Empty,

                        LocationManagerId = v.LocationManagerId,

                        LocationManagerName =
                            v.LocationManager != null &&
                            v.LocationManager.User != null
                                ? v.LocationManager.User.Name
                                : null,

                        ProductionCompanyId = v.ProductionCompanyId,

                        ProductionCompanyName =
                            v.ProductionCompany != null &&
                            v.ProductionCompany.User != null
                                ? v.ProductionCompany.User.Name
                                : null,

                        RequestedByUserId = v.RequestedByUserId,

                        RequesterType = v.LocationManagerId.HasValue
                            ? "LocationManager"
                            : "ProductionCompany",

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

                if (dto.RequestId <= 0)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Invalid visit request id.",
                        "رقم طلب الزيارة غير صالح."
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

                var locationManagerId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var productionCompanyId = await _context.ProductionCompanyProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                var isLocationManager = locationManagerId.HasValue;
                var isProductionCompany = productionCompanyId.HasValue;

                if (isLocationManager == isProductionCompany)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "User must be either a location manager or a production company.",
                        "يجب أن يكون المستخدم مدير موقع أو شركة إنتاج."
                    );
                }

                var pendingStatusId = await GetStatus(
                    "VisitStatus",
                    "Pending"
                );

                if (pendingStatusId == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Pending visit request status was not found in lookup data.",
                        "حالة طلب الزيارة قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                var visitRequest = await _context.LocationVisitRequests
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.RequestId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (visitRequest == null)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit request was not found for the current user.",
                        "لم يتم العثور على طلب الزيارة للمستخدم الحالي."
                    );
                }

                /*
                 * Additional consistency check:
                 * Ensure the requester profile stored on the request still matches
                 * the authenticated user's current profile.
                 */
                if (isLocationManager &&
                    visitRequest.LocationManagerId != locationManagerId.Value)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit request was not found for this location manager.",
                        "لم يتم العثور على طلب الزيارة لمدير الموقع هذا."
                    );
                }

                if (isProductionCompany &&
                    visitRequest.ProductionCompanyId != productionCompanyId.Value)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Visit request was not found for this production company.",
                        "لم يتم العثور على طلب الزيارة لشركة الإنتاج هذه."
                    );
                }

                if (visitRequest.VisitStatusId != pendingStatusId.Value)
                {
                    return ApiResponse<VisitRequestResponseDto>.FailureResponse(
                        "Only pending visit requests can be updated.",
                        "يمكن تعديل طلبات الزيارة المعلقة فقط."
                    );
                }

                /*
                 * Partial update:
                 * Only update fields that were actually provided.
                 */
                if (dto.RequestedVisitDateUtc.HasValue)
                {
                    visitRequest.RequestedVisitDateUtc =
                        dto.RequestedVisitDateUtc.Value;
                }

                if (dto.RequestMessage != null)
                {
                    visitRequest.RequestMessage =
                        string.IsNullOrWhiteSpace(dto.RequestMessage)
                            ? null
                            : dto.RequestMessage.Trim();
                }

                visitRequest.UpdatedAt = DateTime.UtcNow;
                visitRequest.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                var response = await _context.LocationVisitRequests
                    .Where(x =>
                        x.Id == visitRequest.Id &&
                        x.RequestedByUserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new VisitRequestResponseDto
                    {
                        Id = x.Id,

                        LocationId = x.LocationId,
                        LocationName = x.Location.LocationName,
                        City = x.Location.City,

                        LocationOwnerId = x.Location.LocationOwnerId,

                        LocationOwnerName =
                            x.Location.LocationOwner != null &&
                            x.Location.LocationOwner.User != null
                                ? x.Location.LocationOwner.User.Name
                                : string.Empty,

                        LocationManagerId = x.LocationManagerId,

                        LocationManagerName =
                            x.LocationManager != null &&
                            x.LocationManager.User != null
                                ? x.LocationManager.User.Name
                                : null,

                        ProductionCompanyId = x.ProductionCompanyId,

                        ProductionCompanyName =
                            x.ProductionCompany != null &&
                            x.ProductionCompany.User != null
                                ? x.ProductionCompany.User.Name
                                : null,

                        RequestedByUserId = x.RequestedByUserId,

                        RequesterType = x.LocationManagerId.HasValue
                            ? "LocationManager"
                            : "ProductionCompany",

                        RequestedVisitDateUtc = x.RequestedVisitDateUtc,
                        RequestMessage = x.RequestMessage,

                        Status = x.VisitStatus.Name,

                        OwnerResponseMessage = x.OwnerResponseMessage,
                        RespondedAtUtc = x.RespondedAtUtc,

                        CreatedAt = x.CreatedAt
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
                    "Visit request {RequestId} updated by UserId {UserId}, RequesterType {RequesterType}",
                    dto.RequestId,
                    currentUserId,
                    isLocationManager
                        ? "LocationManager"
                        : "ProductionCompany"
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
                    "Error updating visit request {RequestId} by UserId {UserId}",
                    dto?.RequestId,
                    currentUserId
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