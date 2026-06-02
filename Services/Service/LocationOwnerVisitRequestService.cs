using FilmMaker.Common;
using FilmMaker.DTO.Visit;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationOwnerVisitRequestService : ILocationOwnerVisitRequestService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<LocationOwnerVisitRequestService> _logger;

        public LocationOwnerVisitRequestService(FilmMakerDbContext context,ILogger<LocationOwnerVisitRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<LocationVisitRequestResponseDto>> RespondVisitRequest(RespondLocationVisitRequestDto request,int currentUserId)
        {
            if (request == null)
            {
                return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var pendingStatusId = await GetStatusId("VisitStatus", "Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Pending visit request status was not found in lookup data.",
                        "حالة طلب الزيارة قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }

                var targetStatusName = request.IsAccepted ? "Accepted" : "Rejected";

                var targetStatusId = await GetStatusId("VisitStatus", targetStatusName);

                if (targetStatusId == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        $"{targetStatusName} visit request status was not found in lookup data.",
                        "حالة طلب الزيارة غير موجودة في بيانات النظام."
                    );
                }

                var visitRequest = await _context.LocationVisitRequests
                    .Where(x =>
                        x.Id == request.VisitRequestId &&
                        x.Location.LocationOwnerId == locationOwnerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (visitRequest == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Visit request was not found.",
                        "لم يتم العثور على طلب الزيارة."
                    );
                }

                if (visitRequest.VisitStatusId != pendingStatusId.Value)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Only pending visit requests can be responded to.",
                        "يمكن الرد فقط على طلبات الزيارة قيد الانتظار."
                    );
                }

                visitRequest.VisitStatusId = targetStatusId.Value;
                visitRequest.OwnerResponseMessage = string.IsNullOrWhiteSpace(request.ResponseMessage)
                    ? null
                    : request.ResponseMessage.Trim();

                visitRequest.RespondedAtUtc = DateTime.UtcNow;
                visitRequest.RespondedByUserId = currentUserId;
                visitRequest.UpdatedBy = currentUserId.ToString();
                visitRequest.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var response = await GetReceivedVisitRequestByIdAnyStatus(request.VisitRequestId,currentUserId);

                if (!response.Success)
                    return response;

                return ApiResponse<LocationVisitRequestResponseDto>.SuccessResponse(
                    response.Data!,
                    request.IsAccepted
                        ? "Visit request accepted successfully."
                        : "Visit request rejected successfully.",
                    request.IsAccepted
                        ? "تم قبول طلب الزيارة بنجاح."
                        : "تم رفض طلب الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while responding to visit request. VisitRequestId: {VisitRequestId}, UserId: {UserId}",
                    request.VisitRequestId,
                    currentUserId
                );

                return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                    "An unexpected error occurred while responding to the visit request.",
                    "حدث خطأ غير متوقع أثناء الرد على طلب الزيارة."
                );
            }
        }

        public async Task<ApiResponse<LocationVisitRequestResponseDto>> GetReceivedVisitRequestByIdAnyStatus(int visitRequestId,int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var visitRequest = await _context.LocationVisitRequests
                    .Where(x =>
                        x.Id == visitRequestId &&
                        x.Location.LocationOwnerId == locationOwnerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new LocationVisitRequestResponseDto
                    {
                        VisitRequestId = x.Id,

                        LocationId = x.LocationId,
                        LocationName = x.Location.LocationName,

                        LocationOwnerId = x.Location.LocationOwnerId,
                        LocationOwnerName = x.Location.LocationOwner != null
                            ? x.Location.LocationOwner.User.Name
                            : string.Empty,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : string.Empty,

                        RequestedVisitDateUtc = x.RequestedVisitDateUtc,
                        RequestMessage = x.RequestMessage,

                        VisitStatusId = x.VisitStatusId,
                        VisitStatus = x.VisitStatus.Name,

                        OwnerResponseMessage = x.OwnerResponseMessage,
                        RespondedAtUtc = x.RespondedAtUtc,
                        RespondedByUserId = x.RespondedByUserId,

                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (visitRequest == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Visit request was not found.",
                        "لم يتم العثور على طلب الزيارة."
                    );
                }

                return ApiResponse<LocationVisitRequestResponseDto>.SuccessResponse(
                    visitRequest,
                    "Visit request fetched successfully.",
                    "تم جلب طلب الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching visit request by id. VisitRequestId: {VisitRequestId}, UserId: {UserId}",
                    visitRequestId,
                    currentUserId
                );

                return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                    "An unexpected error occurred while fetching the visit request.",
                    "حدث خطأ غير متوقع أثناء جلب طلب الزيارة."
                );
            }
        }
        public async Task<ApiResponse<LocationVisitRequestResponseDto>> GetReceivedVisitRequestById(int visitRequestId, int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }


                var pendingStatusId = await GetStatusId("VisitStatus", "Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Pending visit status was not found in lookup data.",
                        "حالة طلب الزيارة قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }



                var visitRequest = await _context.LocationVisitRequests
                    .Where(x =>
                        x.Id == visitRequestId &&
                        x.Location.LocationOwnerId == locationOwnerId.Value &&
                        x.VisitStatusId == pendingStatusId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new LocationVisitRequestResponseDto
                    {
                        VisitRequestId = x.Id,

                        LocationId = x.LocationId,
                        LocationName = x.Location.LocationName,

                        LocationOwnerId = x.Location.LocationOwnerId,
                        LocationOwnerName = x.Location.LocationOwner.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        RequestedVisitDateUtc = x.RequestedVisitDateUtc,
                        RequestMessage = x.RequestMessage,

                        VisitStatusId = x.VisitStatusId,
                        VisitStatus = x.VisitStatus.Name,

                        OwnerResponseMessage = x.OwnerResponseMessage,
                        RespondedAtUtc = x.RespondedAtUtc,
                        RespondedByUserId = x.RespondedByUserId,

                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (visitRequest == null)
                {
                    return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                        "Visit request was not found.",
                        "لم يتم العثور على طلب الزيارة."
                    );
                }

                return ApiResponse<LocationVisitRequestResponseDto>.SuccessResponse(
                    visitRequest,
                    "Visit request fetched successfully.",
                    "تم جلب طلب الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching visit request by id. VisitRequestId: {VisitRequestId}, UserId: {UserId}",
                    visitRequestId,
                    currentUserId
                );

                return ApiResponse<LocationVisitRequestResponseDto>.FailureResponse(
                    "An unexpected error occurred while fetching the visit request.",
                    "حدث خطأ غير متوقع أثناء جلب طلب الزيارة."
                );
            }
        }
        public async Task<ApiResponse<List<LocationVisitRequestResponseDto>>> GetReceivedVisitRequests(int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<List<LocationVisitRequestResponseDto>>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }



                var pendingStatusId = await GetStatusId("VisitStatus", "Pending");

                if (pendingStatusId == null)
                {
                    return ApiResponse<List<LocationVisitRequestResponseDto>>.FailureResponse(
                        "Pending visit status was not found in lookup data.",
                        "حالة طلب الزيارة قيد الانتظار غير موجودة في بيانات النظام."
                    );
                }




                var requests = await _context.LocationVisitRequests
                    .Where(x =>
                        x.Location.LocationOwnerId == locationOwnerId.Value &&
                        x.VisitStatusId == pendingStatusId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .OrderByDescending(x => x.CreatedAt)
                    .Select(x => new LocationVisitRequestResponseDto
                    {
                        VisitRequestId = x.Id,

                        LocationId = x.LocationId,
                        LocationName = x.Location.LocationName,

                        LocationOwnerId = x.Location.LocationOwnerId,
                        LocationOwnerName = x.Location.LocationOwner.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        RequestedVisitDateUtc = x.RequestedVisitDateUtc,
                        RequestMessage = x.RequestMessage,

                        VisitStatusId = x.VisitStatusId,
                        VisitStatus = x.VisitStatus.Name,

                        OwnerResponseMessage = x.OwnerResponseMessage,
                        RespondedAtUtc = x.RespondedAtUtc,
                        RespondedByUserId = x.RespondedByUserId,

                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<LocationVisitRequestResponseDto>>.SuccessResponse(
                        requests,
                        "No visit requests found.",
                        "لا توجد طلبات زيارة."
                    );
                }

                return ApiResponse<List<LocationVisitRequestResponseDto>>.SuccessResponse(
                    requests,
                    "Visit requests fetched successfully.",
                    "تم جلب طلبات الزيارة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching received visit requests. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<List<LocationVisitRequestResponseDto>>.FailureResponse(
                    "An unexpected error occurred while fetching visit requests.",
                    "حدث خطأ غير متوقع أثناء جلب طلبات الزيارة."
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
