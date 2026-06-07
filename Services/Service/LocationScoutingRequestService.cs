using FilmMaker.Common;
using FilmMaker.DTO.LocationScouting.Request;
using FilmMaker.DTO.LocationScouting.Response;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationScoutingRequestService : ILocationScoutingRequestService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<LocationScoutingRequestService> _logger;

        public LocationScoutingRequestService(FilmMakerDbContext context,ILogger<LocationScoutingRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ApiResponse<LocationScoutingRequestResponseDto>> CreateLocationScoutingRequest(CreateLocationScoutingRequestDto dto,int currentUserId)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                var validationResult = ValidateLocationScoutingRequest(
                    dto.LocationManagerId,
                    dto.CityId,
                    dto.StartDate,
                    dto.EndDate,
                    dto.Requirements,
                    dto.MinBudget,
                    dto.MaxBudget
                );

                if (validationResult != null)
                {
                    return validationResult;
                }

                var productionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);

                if (productionCompanyId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Production company profile was not found.",
                        "لم يتم العثور على ملف شركة الإنتاج."
                    );
                }

                var locationManagerExists = await _context.LocationManagerProfiles
                    .AnyAsync(x =>
                        x.Id == dto.LocationManagerId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!locationManagerExists)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location manager was not found.",
                        "مدير الموقع غير موجود."
                    );
                }

                if (dto.CityId.HasValue)
                {
                    var cityExists = await _context.LookupItems
                        .AnyAsync(x =>
                            x.Id == dto.CityId.Value &&
                            x.IsActive &&
                            !x.IsDeleted &&
                            x.LookupCategory.Name == "City" &&
                            x.LookupCategory.IsActive &&
                            !x.LookupCategory.IsDeleted);

                    if (!cityExists)
                    {
                        return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                            "City was not found.",
                            "المدينة غير موجودة."
                        );
                    }
                }

                var pendingStatusId = await GetStatus(
                    "LocationScoutingRequestStatus",
                    "Pending"
                );

                if (pendingStatusId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Pending scouting request status was not found in lookup data.",
                        "حالة طلب البحث قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                var entity = new LocationScoutingRequest
                {
                    ProductionCompanyId = productionCompanyId.Value,
                    LocationManagerId = dto.LocationManagerId,

                    CityId = dto.CityId,

                    StartDate = dto.StartDate,
                    EndDate = dto.EndDate,

                    Requirements = dto.Requirements.Trim(),
                    Notes = string.IsNullOrWhiteSpace(dto.Notes)
                        ? null
                        : dto.Notes.Trim(),

                    MinBudget = dto.MinBudget,
                    MaxBudget = dto.MaxBudget,

                    StatusId = pendingStatusId.Value,

                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false
                };

                await _context.LocationScoutingRequests.AddAsync(entity);
                await _context.SaveChangesAsync();

                var response = await GetLocationScoutingRequestDtoByIdAsync(entity.Id);

                if (response == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Request was created, but response data could not be loaded.",
                        "تم إنشاء الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<LocationScoutingRequestResponseDto>.SuccessResponse(
                    response,
                    "Location scouting request created successfully.",
                    "تم إنشاء طلب البحث عن موقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating location scouting request for user {UserId}",
                    currentUserId
                );

                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "An error occurred while creating the location scouting request.",
                    "حدث خطأ أثناء إنشاء طلب البحث عن موقع."
                );
            }
        }
        public async Task<ApiResponse<LocationScoutingRequestResponseDto>> UpdateLocationScoutingRequest(UpdateLocationScoutingRequestDto dto,int currentUserId)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                if (dto.RequestId <= 0)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var validationResult = ValidateLocationScoutingRequest(
                    dto.LocationManagerId,
                    dto.CityId,
                    dto.StartDate,
                    dto.EndDate,
                    dto.Requirements,
                    dto.MinBudget,
                    dto.MaxBudget
                );

                if (validationResult != null)
                {
                    return validationResult;
                }

                var productionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);

                if (productionCompanyId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Production company profile was not found.",
                        "لم يتم العثور على ملف شركة الإنتاج."
                    );
                }

                var request = await _context.LocationScoutingRequests
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.RequestId &&
                        x.ProductionCompanyId == productionCompanyId.Value &&
                        !x.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location scouting request was not found.",
                        "لم يتم العثور على طلب البحث عن موقع."
                    );
                }

                var pendingStatusId = await GetStatus(
                    "LocationScoutingRequestStatus",
                    "Pending"
                );

                if (pendingStatusId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Pending scouting request status was not found in lookup data.",
                        "حالة طلب البحث قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                if (request.StatusId != pendingStatusId.Value)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Only pending scouting requests can be updated.",
                        "يمكن تعديل طلبات البحث قيد الانتظار فقط."
                    );
                }

                var locationManagerExists = await _context.LocationManagerProfiles
                    .AnyAsync(x =>
                        x.Id == dto.LocationManagerId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!locationManagerExists)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location manager was not found.",
                        "مدير الموقع غير موجود."
                    );
                }

                

                if (dto.CityId.HasValue)
                {
                    var cityExists = await _context.LookupItems
                        .AnyAsync(x =>
                            x.Id == dto.CityId.Value &&
                            x.IsActive &&
                            !x.IsDeleted &&
                            x.LookupCategory.Name == "City" &&
                            x.LookupCategory.IsActive &&
                            !x.LookupCategory.IsDeleted);

                    if (!cityExists)
                    {
                        return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                            "City was not found.",
                            "المدينة غير موجودة."
                        );
                    }
                }

                request.LocationManagerId = dto.LocationManagerId;
                request.CityId = dto.CityId;

                request.StartDate = dto.StartDate;
                request.EndDate = dto.EndDate;

                request.Requirements = dto.Requirements.Trim();
                request.Notes = string.IsNullOrWhiteSpace(dto.Notes)
                    ? null
                    : dto.Notes.Trim();

                request.MinBudget = dto.MinBudget;
                request.MaxBudget = dto.MaxBudget;

                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                var response = await GetLocationScoutingRequestDtoByIdAsync(request.Id);

                if (response == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Request was updated, but response data could not be loaded.",
                        "تم تعديل الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<LocationScoutingRequestResponseDto>.SuccessResponse(
                    response,
                    "Location scouting request updated successfully.",
                    "تم تعديل طلب البحث عن موقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error updating location scouting request {RequestId} for user {UserId}",
                    dto?.RequestId,
                    currentUserId
                );

                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "An error occurred while updating the location scouting request.",
                    "حدث خطأ أثناء تعديل طلب البحث عن موقع."
                );
            }
        }
        public async Task<ApiResponse<bool>> CancelLocationScoutingRequest(int requestId,int currentUserId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var productionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);

                if (productionCompanyId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Production company profile was not found.",
                        "لم يتم العثور على ملف شركة الإنتاج."
                    );
                }

                var request = await _context.LocationScoutingRequests
                    .Where(x =>
                        x.Id == requestId &&
                        x.ProductionCompanyId == productionCompanyId.Value &&
                        x.IsActive &&
                        !x.IsDeleted).FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Location scouting request was not found.",
                        "لم يتم العثور على طلب البحث عن موقع."
                    );
                }

                var pendingStatusId = await GetStatus("LocationScoutingRequestStatus","Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Pending scouting request status was not found in lookup data.",
                        "حالة طلب البحث قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                if (request.StatusId != pendingStatusId.Value)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending scouting requests can be cancelled.",
                        "يمكن إلغاء طلبات البحث قيد الانتظار فقط."
                    );
                }

                var cancelledStatusId = await GetStatus(
                    "LocationScoutingRequestStatus",
                    "Cancelled"
                );

                if (cancelledStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Cancelled scouting request status was not found in lookup data.",
                        "حالة إلغاء طلب البحث غير موجودة في بيانات النظام."
                    );
                }

                request.StatusId = cancelledStatusId.Value;
                request.IsActive = false;
                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Location scouting request cancelled successfully.",
                    "تم إلغاء طلب البحث عن موقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cancelling location scouting request {RequestId} for user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the location scouting request.",
                    "حدث خطأ أثناء إلغاء طلب البحث عن موقع."
                );
            }
        }
        public async Task<ApiResponse<LocationScoutingRequestResponseDto>> GetLocationScoutingRequestById(int requestId,int currentUserId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var productionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);

                var locationManagerId = await _context.LocationManagerProfiles
                    .Where(x =>
                        x.UserId == currentUserId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => (int?)x.Id)
                    .FirstOrDefaultAsync();

                if (productionCompanyId == null && locationManagerId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "You are not authorized to view this request.",
                        "غير مصرح لك بعرض هذا الطلب."
                    );
                }

                var request = await _context.LocationScoutingRequests
                    .Where(x =>
                        x.Id == requestId &&
                        !x.IsDeleted &&
                        (
                            (productionCompanyId != null && x.ProductionCompanyId == productionCompanyId.Value) ||
                            (locationManagerId != null && x.LocationManagerId == locationManagerId.Value)
                        ))
                    .Select(x => new LocationScoutingRequestResponseDto
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        CityId = x.CityId,
                        CityName = x.CityId != null
                            ? x.City!.Name
                            : null,

                        StartDate = x.StartDate,
                        EndDate = x.EndDate,

                        Requirements = x.Requirements,
                        Notes = x.Notes,

                        MinBudget = x.MinBudget,
                        MaxBudget = x.MaxBudget,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        LocationManagerResponse = x.LocationManagerResponse,
                        RespondedAtUtc = x.RespondedAtUtc,
                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted
                    })
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location scouting request was not found.",
                        "لم يتم العثور على طلب البحث عن موقع."
                    );
                }

                return ApiResponse<LocationScoutingRequestResponseDto>.SuccessResponse(
                    request,
                    "Location scouting request fetched successfully.",
                    "تم جلب طلب البحث عن موقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching location scouting request {RequestId} for user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "An error occurred while fetching the location scouting request.",
                    "حدث خطأ أثناء جلب طلب البحث عن موقع."
                );
            }
        }
        public async Task<ApiResponse<List<LocationScoutingRequestResponseDto>>> GetMyReceivedLocationScoutingRequests(int currentUserId)
        {
            try
            {
                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<List<LocationScoutingRequestResponseDto>>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var requests = await _context.LocationScoutingRequests
                    .Where(x =>
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new LocationScoutingRequestResponseDto
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        CityId = x.CityId,
                        CityName = x.CityId != null
                            ? x.City!.Name
                            : null,

                        StartDate = x.StartDate,
                        EndDate = x.EndDate,

                        Requirements = x.Requirements,
                        Notes = x.Notes,

                        MinBudget = x.MinBudget,
                        MaxBudget = x.MaxBudget,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        LocationManagerResponse = x.LocationManagerResponse,
                        RespondedAtUtc = x.RespondedAtUtc,
                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<LocationScoutingRequestResponseDto>>.SuccessResponse(
                        requests,
                        "No received location scouting requests found.",
                        "لا توجد طلبات بحث عن موقع مستلمة."
                    );
                }

                return ApiResponse<List<LocationScoutingRequestResponseDto>>.SuccessResponse(
                    requests,
                    "Received location scouting requests fetched successfully.",
                    "تم جلب طلبات البحث عن موقع المستلمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching received location scouting requests for user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<LocationScoutingRequestResponseDto>>.FailureResponse(
                    "An error occurred while fetching received location scouting requests.",
                    "حدث خطأ أثناء جلب طلبات البحث عن موقع المستلمة."
                );
            }
        }
        public async Task<ApiResponse<List<LocationScoutingRequestResponseDto>>> GetMySentLocationScoutingRequests(int currentUserId)
        {
            try
            {
                var productionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);

                if (productionCompanyId == null)
                {
                    return ApiResponse<List<LocationScoutingRequestResponseDto>>.FailureResponse(
                        "Production company profile was not found.",
                        "لم يتم العثور على ملف شركة الإنتاج."
                    );
                }

                var requests = await _context.LocationScoutingRequests
                    .Where(x =>
                        x.ProductionCompanyId == productionCompanyId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new LocationScoutingRequestResponseDto
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        CityId = x.CityId,
                        CityName = x.CityId != null
                            ? x.City!.Name
                            : null,

                        StartDate = x.StartDate,
                        EndDate = x.EndDate,

                        Requirements = x.Requirements,
                        Notes = x.Notes,

                        MinBudget = x.MinBudget,
                        MaxBudget = x.MaxBudget,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        LocationManagerResponse = x.LocationManagerResponse,
                        RespondedAtUtc = x.RespondedAtUtc,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<LocationScoutingRequestResponseDto>>.SuccessResponse(
                        requests,
                        "No sent location scouting requests found.",
                        "لا توجد طلبات بحث عن موقع مرسلة."
                    );
                }

                return ApiResponse<List<LocationScoutingRequestResponseDto>>.SuccessResponse(
                    requests,
                    "Sent location scouting requests fetched successfully.",
                    "تم جلب طلبات البحث عن موقع المرسلة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching sent location scouting requests for user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<LocationScoutingRequestResponseDto>>.FailureResponse(
                    "An error occurred while fetching sent location scouting requests.",
                    "حدث خطأ أثناء جلب طلبات البحث عن موقع المرسلة."
                );
            }
        }
        public async Task<ApiResponse<LocationScoutingRequestResponseDto>> RespondToLocationScoutingRequest(RespondLocationScoutingRequestDto dto,int currentUserId)
        {
            try
            {
                if (dto == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                if (dto.RequestId <= 0)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var request = await _context.LocationScoutingRequests
                    .FirstOrDefaultAsync(x =>
                        x.Id == dto.RequestId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location scouting request was not found.",
                        "لم يتم العثور على طلب البحث عن موقع."
                    );
                }

                var pendingStatusId = await GetStatus("LocationScoutingRequestStatus","Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Pending scouting request status was not found in lookup data.",
                        "حالة طلب البحث قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                if (request.StatusId != pendingStatusId.Value)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Only pending scouting requests can be responded to.",
                        "يمكن الرد فقط على طلبات البحث قيد الانتظار."
                    );
                }

                var targetStatusName = dto.IsAccepted ? "Accepted" : "Rejected";

                var targetStatusId = await GetStatus("LocationScoutingRequestStatus",targetStatusName);

                if (targetStatusId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        $"{targetStatusName} scouting request status was not found in lookup data.",
                        "حالة طلب البحث غير موجودة في بيانات النظام."
                    );
                }

                request.StatusId = targetStatusId.Value;
                request.LocationManagerResponse = string.IsNullOrWhiteSpace(dto.ResponseMessage)
                    ? null
                    : dto.ResponseMessage.Trim();

                request.RespondedAtUtc = DateTime.UtcNow;
                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                var response = await GetLocationScoutingRequestDtoByIdAsync(request.Id);

                if (response == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Request was updated, but response data could not be loaded.",
                        "تم تحديث الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<LocationScoutingRequestResponseDto>.SuccessResponse(
                    response,
                    dto.IsAccepted
                        ? "Location scouting request accepted successfully."
                        : "Location scouting request rejected successfully.",
                    dto.IsAccepted
                        ? "تم قبول طلب البحث عن موقع بنجاح."
                        : "تم رفض طلب البحث عن موقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error responding to location scouting request {RequestId} for user {UserId}",
                    dto?.RequestId,
                    currentUserId
                );

                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "An error occurred while responding to the location scouting request.",
                    "حدث خطأ أثناء الرد على طلب البحث عن موقع."
                );
            }
        }
        public async Task<ApiResponse<LocationScoutingRequestResponseDto>> GetManagerLocationScoutingRequestById(int requestId,int currentUserId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var request = await _context.LocationScoutingRequests
                    .Where(x =>
                        x.Id == requestId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new LocationScoutingRequestResponseDto
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        CityId = x.CityId,
                        CityName = x.CityId != null
                            ? x.City!.Name
                            : null,

                        StartDate = x.StartDate,
                        EndDate = x.EndDate,

                        Requirements = x.Requirements,
                        Notes = x.Notes,

                        MinBudget = x.MinBudget,
                        MaxBudget = x.MaxBudget,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        LocationManagerResponse = x.LocationManagerResponse,
                        RespondedAtUtc = x.RespondedAtUtc,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted
                    })
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                        "Location scouting request was not found.",
                        "لم يتم العثور على طلب البحث عن موقع."
                    );
                }

                return ApiResponse<LocationScoutingRequestResponseDto>.SuccessResponse(
                    request,
                    "Location scouting request fetched successfully.",
                    "تم جلب طلب البحث عن موقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error fetching location scouting request {RequestId} for manager user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "An error occurred while fetching the location scouting request.",
                    "حدث خطأ أثناء جلب طلب البحث عن موقع."
                );
            }
        }


        #region Helper methods
        private ApiResponse<LocationScoutingRequestResponseDto>? ValidateLocationScoutingRequest(int locationManagerId,int? cityId,DateTime startDate,DateTime endDate,string requirements,decimal? minBudget,decimal? maxBudget)
        {
            if (locationManagerId <= 0)
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Location manager is required.",
                    "مدير الموقع مطلوب."
                );
            }

            if (startDate <= DateTime.UtcNow)
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Start date must be in the future.",
                    "يجب أن يكون تاريخ البداية في المستقبل."
                );
            }

            if (endDate <= startDate)
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "End date must be after start date.",
                    "يجب أن يكون تاريخ النهاية بعد تاريخ البداية."
                );
            }

            if (string.IsNullOrWhiteSpace(requirements))
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Requirements are required.",
                    "المتطلبات مطلوبة."
                );
            }

            if (minBudget.HasValue && minBudget.Value < 0)
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Minimum budget cannot be negative.",
                    "لا يمكن أن تكون أقل ميزانية قيمة سالبة."
                );
            }

            if (maxBudget.HasValue && maxBudget.Value < 0)
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Maximum budget cannot be negative.",
                    "لا يمكن أن تكون أعلى ميزانية قيمة سالبة."
                );
            }

            if (minBudget.HasValue &&
                maxBudget.HasValue &&
                maxBudget.Value < minBudget.Value)
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Maximum budget must be greater than or equal to minimum budget.",
                    "يجب أن تكون أعلى ميزانية أكبر من أو تساوي أقل ميزانية."
                );
            }

            if (cityId.HasValue && cityId.Value <= 0)
            {
                return ApiResponse<LocationScoutingRequestResponseDto>.FailureResponse(
                    "Invalid city.",
                    "المدينة غير صالحة."
                );
            }

            return null;
        }
        private async Task<LocationScoutingRequestResponseDto?> GetLocationScoutingRequestDtoByIdAsync(int requestId)
        {
            return await _context.LocationScoutingRequests
                .Where(x => x.Id == requestId)
                .Select(x => new LocationScoutingRequestResponseDto
                {
                    Id = x.Id,

                    ProductionCompanyId = x.ProductionCompanyId,
                    ProductionCompanyName = x.ProductionCompany.User.Name,

                    LocationManagerId = x.LocationManagerId,
                    LocationManagerName = x.LocationManager.User.Name,
                    CityId = x.CityId,
                    CityName = x.CityId != null
                        ? x.City!.Name
                        : null,

                    StartDate = x.StartDate,
                    EndDate = x.EndDate,

                    Requirements = x.Requirements,
                    Notes = x.Notes,

                    MinBudget = x.MinBudget,
                    MaxBudget = x.MaxBudget,

                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,

                    LocationManagerResponse = x.LocationManagerResponse,
                    RespondedAtUtc = x.RespondedAtUtc,
                    CreatedAt = x.CreatedAt,
                    IsActive = x.IsActive,
                    IsDeleted = x.IsDeleted
                })
                .FirstOrDefaultAsync();
        }
        private async Task<int?> GetProductionCompanyIdByUserId(int currentUserId)
        {
            return await _context.ProductionCompanyProfiles
                .Where(x =>
                    x.UserId == currentUserId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }
        private async Task<int?> GetStatus(string categoryName, string itemName)
        {
            return await _context.LookupItems
                .Where(x =>
                    x.Name == itemName &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    x.LookupCategory.Name == categoryName &&
                    x.LookupCategory.IsActive &&
                    !x.LookupCategory.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }
        private async Task<int?> GetLocationManagerIdByUserId(int currentUserId)
        {
            return await _context.LocationManagerProfiles
                .Where(x =>
                    x.UserId == currentUserId &&
                    x.IsActive &&
                    !x.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }
        #endregion
    }
}
