using FilmMaker.Common;
using FilmMaker.DTO.ServiceProviderBooking;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class ServiceProviderRequestService : IServiceProviderRequestService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<ServiceProviderRequestService> _logger;
        

        public ServiceProviderRequestService(FilmMakerDbContext context, ILogger<ServiceProviderRequestService> logger)
        {
            _context = context;
            _logger = logger;
        }
        public async Task<ApiResponse<GetServiceProviderRequestDTO>> SendServiceRequestToProvider(
    SendServiceRequestToProviderDTO request,
    int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var validationResult = ValidateSendServiceRequestToProvider(request);

                if (validationResult != null)
                {
                    return validationResult;
                }

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var originalRequest = await _context.RequestToLocationManagerToBookService
                    .Where(x =>
                        x.Id == request.ServiceRequestToLocationManagerId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.StatusId,
                        x.LocationManagerId
                    })
                    .FirstOrDefaultAsync();

                if (originalRequest == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service request was not found.",
                        "طلب الخدمات غير موجود."
                    );
                }

                var acceptedByManagerStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    "AcceptedByManager"
                );

                if (acceptedByManagerStatusId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Accepted by manager status was not found in lookup data.",
                        "حالة قبول مدير الموقع غير موجودة في بيانات النظام."
                    );
                }

                if (originalRequest.StatusId != acceptedByManagerStatusId.Value)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Only accepted service requests can be sent to a service provider.",
                        "يمكن إرسال طلبات الخدمات المقبولة فقط إلى مزود الخدمة."
                    );
                }

                var serviceProviderExists = await _context.ServiceProviderProfiles
                    .AnyAsync(x =>
                        x.Id == request.ServiceProviderId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!serviceProviderExists)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider was not found.",
                        "مزود الخدمة غير موجود."
                    );
                }

                var requestItemIds = request.Items
                    .Select(x => x.RequestToLocationManagerItemId)
                    .Distinct()
                    .ToList();

                var originalItems = await _context.RequestToLocationManagerToBookServiceItems
                    .Where(x =>
                        requestItemIds.Contains(x.Id) &&
                        x.RequestToLocationManagerToBookServiceId == originalRequest.Id &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.ServiceTypeId,
                        x.CustomServiceType,
                        x.Quantity,
                        x.StartDate,
                        x.EndDate
                    })
                    .ToListAsync();

                if (originalItems.Count != requestItemIds.Count)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "One or more request items were not found.",
                        "واحد أو أكثر من عناصر الطلب غير موجود."
                    );
                }

                var serviceIds = request.Items
                    .Select(x => x.ServiceId)
                    .Distinct()
                    .ToList();

                var selectedServices = await _context.ServicesProvided
                    .Where(x =>
                        serviceIds.Contains(x.Id) &&
                        x.ServiceProviderId == request.ServiceProviderId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.ServiceProviderId,
                        x.ServiceTypeId,
                        x.CustomServiceType,
                        x.IsCustom,
                        x.AvailableQuantity
                    })
                    .ToListAsync();

                if (selectedServices.Count != serviceIds.Count)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "One or more selected services were not found for this provider.",
                        "واحدة أو أكثر من الخدمات المحددة غير موجودة لدى مزود الخدمة."
                    );
                }

                var pendingProviderReviewStatusId = await GetStatus(
                    "ServiceProviderRequestStatus",
                    "PendingProviderReview"
                );

                var acceptedByProviderStatusId = await GetStatus(
                    "ServiceProviderRequestStatus",
                    "AcceptedByProvider"
                );

                if (pendingProviderReviewStatusId == null || acceptedByProviderStatusId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider request status lookup data is missing.",
                        "بيانات حالات طلب مزود الخدمة غير مكتملة."
                    );
                }

                var blockingProviderRequestStatusIds = new[]
                {
                    pendingProviderReviewStatusId.Value,
                    acceptedByProviderStatusId.Value
                };

                var alreadySentItemIds = await _context.ServiceProviderRequestItems
                    .Where(x =>
                        requestItemIds.Contains(x.RequestToLocationManagerToBookServiceItemId) &&
                        blockingProviderRequestStatusIds.Contains(x.ServiceProviderRequest.StatusId) &&
                        x.ServiceProviderRequest.IsActive &&
                        !x.ServiceProviderRequest.IsDeleted &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => x.RequestToLocationManagerToBookServiceItemId)
                    .Distinct()
                    .ToListAsync();

                if (alreadySentItemIds.Any())
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "One or more request items are already pending or accepted by a service provider.",
                        "واحد أو أكثر من عناصر الطلب قيد المراجعة أو تم قبوله من مزود خدمة."
                    );
                }

                foreach (var requestItem in request.Items)
                {
                    var originalItem = originalItems.First(x =>
                        x.Id == requestItem.RequestToLocationManagerItemId);

                    var selectedService = selectedServices.First(x =>
                        x.Id == requestItem.ServiceId);

                    var providerSupportsRequestedType = await ProviderSupportsRequestedServiceType(
                        request.ServiceProviderId,
                        originalItem.ServiceTypeId,
                        originalItem.CustomServiceType
                    );

                    if (!providerSupportsRequestedType)
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "The selected provider does not support one or more requested service types.",
                            "مزود الخدمة المحدد لا يدعم واحدًا أو أكثر من أنواع الخدمات المطلوبة."
                        );
                    }

                    var serviceMatchesRequestedType = DoesSelectedServiceMatchRequestedItem(
                        selectedService.ServiceTypeId,
                        selectedService.CustomServiceType,
                        selectedService.IsCustom,
                        originalItem.ServiceTypeId,
                        originalItem.CustomServiceType
                    );

                    if (!serviceMatchesRequestedType)
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "One or more selected services do not match the requested service type.",
                            "واحدة أو أكثر من الخدمات المحددة لا تطابق نوع الخدمة المطلوبة."
                        );
                    }

                    if (originalItem.Quantity.HasValue &&
                        selectedService.AvailableQuantity.HasValue &&
                        selectedService.AvailableQuantity.Value < originalItem.Quantity.Value)
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "Selected service does not have enough available quantity.",
                            "الخدمة المحددة لا تحتوي على كمية متوفرة كافية."
                        );
                    }
                }

                var now = DateTime.UtcNow;

                var entity = new ServiceProviderRequest
                {
                    RequestToLocationManagerToBookServiceId = originalRequest.Id,
                    LocationManagerId = locationManagerId.Value,
                    ServiceProviderId = request.ServiceProviderId,

                    MessageToProvider = string.IsNullOrWhiteSpace(request.MessageToProvider)
                        ? null
                        : request.MessageToProvider.Trim(),

                    StatusId = pendingProviderReviewStatusId.Value,

                    CreatedAt = now,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false,

                    Items = request.Items.Select(item => new ServiceProviderRequestItem
                    {
                        RequestToLocationManagerToBookServiceItemId = item.RequestToLocationManagerItemId,
                        ServiceId = item.ServiceId,

                        CreatedAt = now,
                        CreatedBy = currentUserId.ToString(),
                        IsActive = true,
                        IsDeleted = false
                    }).ToList()
                };

                await _context.ServiceProviderRequests.AddAsync(entity);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var response = await GetServiceProviderRequestDtoById(entity.Id);

                if (response == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Request was sent, but response data could not be loaded.",
                        "تم إرسال الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<GetServiceProviderRequestDTO>.SuccessResponse(
                    response,
                    "Service request sent to provider successfully.",
                    "تم إرسال طلب الخدمة إلى مزود الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error sending service request to provider for user {UserId}",
                    currentUserId
                );

                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "An error occurred while sending the service request to provider.",
                    "حدث خطأ أثناء إرسال طلب الخدمة إلى مزود الخدمة."
                );
            }
        }

        public async Task<ApiResponse<GetServiceProviderRequestDTO>> UpdateServiceProviderRequest(
    UpdateServiceProviderRequestDTO request,
    int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var validationResult = ValidateUpdateServiceProviderRequest(request);

                if (validationResult != null)
                {
                    return validationResult;
                }

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var existingRequest = await _context.ServiceProviderRequests
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.RequestId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (existingRequest == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider request was not found.",
                        "طلب مزود الخدمة غير موجود."
                    );
                }

                var pendingProviderReviewStatusId = await GetStatus(
                    "ServiceProviderRequestStatus",
                    "PendingProviderReview"
                );

                if (pendingProviderReviewStatusId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Pending provider review status was not found in lookup data.",
                        "حالة انتظار مراجعة مزود الخدمة غير موجودة في بيانات النظام."
                    );
                }

                if (existingRequest.StatusId != pendingProviderReviewStatusId.Value)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Only pending provider requests can be updated.",
                        "يمكن تعديل طلبات مزود الخدمة قيد الانتظار فقط."
                    );
                }

                var originalServiceRequest = await _context.RequestToLocationManagerToBookService
                    .Where(x =>
                        x.Id == existingRequest.RequestToLocationManagerToBookServiceId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.StatusId
                    })
                    .FirstOrDefaultAsync();

                if (originalServiceRequest == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Original service request was not found.",
                        "طلب الخدمات الأصلي غير موجود."
                    );
                }

                var acceptedByManagerStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    "AcceptedByManager"
                );

                if (acceptedByManagerStatusId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Accepted by manager status was not found in lookup data.",
                        "حالة قبول مدير الموقع غير موجودة في بيانات النظام."
                    );
                }

                if (originalServiceRequest.StatusId != acceptedByManagerStatusId.Value)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Only accepted service requests can be sent to a service provider.",
                        "يمكن إرسال طلبات الخدمات المقبولة فقط إلى مزود الخدمة."
                    );
                }

                var shouldUpdateItems = request.Items != null && request.Items.Any();
                var isChangingProvider = request.ServiceProviderId.HasValue &&
                                         request.ServiceProviderId.Value != existingRequest.ServiceProviderId;

                if (isChangingProvider && !shouldUpdateItems)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Items are required when changing the service provider.",
                        "يجب إرسال عناصر الطلب عند تغيير مزود الخدمة."
                    );
                }

                var finalServiceProviderId = request.ServiceProviderId ?? existingRequest.ServiceProviderId;

                var serviceProviderExists = await _context.ServiceProviderProfiles
                    .AnyAsync(x =>
                        x.Id == finalServiceProviderId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!serviceProviderExists)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider was not found.",
                        "مزود الخدمة غير موجود."
                    );
                }

                if (shouldUpdateItems)
                {
                    var requestItemIds = request.Items!
                        .Select(x => x.RequestToLocationManagerItemId!.Value)
                        .Distinct()
                        .ToList();

                    var originalItems = await _context.RequestToLocationManagerToBookServiceItems
                        .Where(x =>
                            requestItemIds.Contains(x.Id) &&
                            x.RequestToLocationManagerToBookServiceId == originalServiceRequest.Id &&
                            x.IsActive &&
                            !x.IsDeleted)
                        .Select(x => new
                        {
                            x.Id,
                            x.ServiceTypeId,
                            x.CustomServiceType,
                            x.Quantity,
                            x.StartDate,
                            x.EndDate
                        })
                        .ToListAsync();

                    if (originalItems.Count != requestItemIds.Count)
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "One or more request items were not found.",
                            "واحد أو أكثر من عناصر الطلب غير موجود."
                        );
                    }

                    var serviceIds = request.Items!
                        .Select(x => x.ServiceId!.Value)
                        .Distinct()
                        .ToList();

                    var selectedServices = await _context.ServicesProvided
                        .Where(x =>
                            serviceIds.Contains(x.Id) &&
                            x.ServiceProviderId == finalServiceProviderId &&
                            x.IsActive &&
                            !x.IsDeleted)
                        .Select(x => new
                        {
                            x.Id,
                            x.ServiceProviderId,
                            x.ServiceTypeId,
                            x.CustomServiceType,
                            x.IsCustom,
                            x.AvailableQuantity
                        })
                        .ToListAsync();

                    if (selectedServices.Count != serviceIds.Count)
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "One or more selected services were not found for this provider.",
                            "واحدة أو أكثر من الخدمات المحددة غير موجودة لدى مزود الخدمة."
                        );
                    }

                    var alreadySentItemIds = await _context.ServiceProviderRequestItems
                        .Where(x =>
                            requestItemIds.Contains(x.RequestToLocationManagerToBookServiceItemId) &&
                            x.ServiceProviderRequestId != existingRequest.Id &&
                            x.ServiceProviderRequest.IsActive &&
                            !x.ServiceProviderRequest.IsDeleted &&
                            x.IsActive &&
                            !x.IsDeleted)
                        .Select(x => x.RequestToLocationManagerToBookServiceItemId)
                        .Distinct()
                        .ToListAsync();

                    if (alreadySentItemIds.Any())
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "One or more request items were already sent to another service provider.",
                            "واحد أو أكثر من عناصر الطلب تم إرساله مسبقًا إلى مزود خدمة آخر."
                        );
                    }

                    foreach (var requestItem in request.Items!)
                    {
                        var originalItem = originalItems.First(x =>
                            x.Id == requestItem.RequestToLocationManagerItemId!.Value);

                        var selectedService = selectedServices.First(x =>
                            x.Id == requestItem.ServiceId!.Value);

                        var providerSupportsRequestedType = await ProviderSupportsRequestedServiceType(
                            finalServiceProviderId,
                            originalItem.ServiceTypeId,
                            originalItem.CustomServiceType
                        );

                        if (!providerSupportsRequestedType)
                        {
                            return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                                "The selected provider does not support one or more requested service types.",
                                "مزود الخدمة المحدد لا يدعم واحدًا أو أكثر من أنواع الخدمات المطلوبة."
                            );
                        }

                        var serviceMatchesRequestedType = DoesSelectedServiceMatchRequestedItem(
                            selectedService.ServiceTypeId,
                            selectedService.CustomServiceType,
                            selectedService.IsCustom,
                            originalItem.ServiceTypeId,
                            originalItem.CustomServiceType
                        );

                        if (!serviceMatchesRequestedType)
                        {
                            return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                                "One or more selected services do not match the requested service type.",
                                "واحدة أو أكثر من الخدمات المحددة لا تطابق نوع الخدمة المطلوبة."
                            );
                        }

                        if (originalItem.Quantity.HasValue &&
                            selectedService.AvailableQuantity.HasValue &&
                            selectedService.AvailableQuantity.Value < originalItem.Quantity.Value)
                        {
                            return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                                "Selected service does not have enough available quantity.",
                                "الخدمة المحددة لا تحتوي على كمية متوفرة كافية."
                            );
                        }
                    }
                }

                var now = DateTime.UtcNow;

                existingRequest.ServiceProviderId = finalServiceProviderId;

                if (request.MessageToProvider != null)
                {
                    existingRequest.MessageToProvider = string.IsNullOrWhiteSpace(request.MessageToProvider)
                        ? null
                        : request.MessageToProvider.Trim();
                }

                existingRequest.UpdatedAt = now;
                existingRequest.UpdatedBy = currentUserId.ToString();

                if (shouldUpdateItems)
                {
                    var oldItems = await _context.ServiceProviderRequestItems
                        .Where(x =>
                            x.ServiceProviderRequestId == existingRequest.Id &&
                            !x.IsDeleted)
                        .ToListAsync();

                    foreach (var oldItem in oldItems)
                    {
                        oldItem.IsDeleted = true;
                        oldItem.IsActive = false;
                        oldItem.UpdatedAt = now;
                        oldItem.UpdatedBy = currentUserId.ToString();
                    }

                    var newItems = request.Items!.Select(item => new ServiceProviderRequestItem
                    {
                        ServiceProviderRequestId = existingRequest.Id,
                        RequestToLocationManagerToBookServiceItemId = item.RequestToLocationManagerItemId!.Value,
                        ServiceId = item.ServiceId!.Value,

                        CreatedAt = now,
                        CreatedBy = currentUserId.ToString(),
                        IsActive = true,
                        IsDeleted = false
                    }).ToList();

                    await _context.ServiceProviderRequestItems.AddRangeAsync(newItems);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await GetServiceProviderRequestDtoById(existingRequest.Id);

                if (response == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Request was updated, but response data could not be loaded.",
                        "تم تعديل الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<GetServiceProviderRequestDTO>.SuccessResponse(
                    response,
                    "Service provider request updated successfully.",
                    "تم تعديل طلب مزود الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error updating service provider request {RequestId} for user {UserId}",
                    request?.RequestId,
                    currentUserId
                );

                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "An error occurred while updating the service provider request.",
                    "حدث خطأ أثناء تعديل طلب مزود الخدمة."
                );
            }
        }

        public async Task<ApiResponse<bool>> CancelServiceProviderRequest(int requestId,int currentUserId)
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

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var request = await _context.ServiceProviderRequests
                    .FirstOrDefaultAsync(x =>
                        x.Id == requestId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Service provider request was not found.",
                        "طلب مزود الخدمة غير موجود."
                    );
                }

                var pendingProviderReviewStatusId = await GetStatus(
                    "ServiceProviderRequestStatus",
                    "PendingProviderReview"
                );

                if (pendingProviderReviewStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Pending provider review status was not found in lookup data.",
                        "حالة انتظار مراجعة مزود الخدمة غير موجودة في بيانات النظام."
                    );
                }

                if (request.StatusId != pendingProviderReviewStatusId.Value)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending provider requests can be cancelled.",
                        "يمكن إلغاء طلبات مزود الخدمة قيد انتظار المراجعة فقط."
                    );
                }

                var cancelledStatusId = await GetStatus(
                    "ServiceProviderRequestStatus",
                    "Cancelled"
                );

                if (cancelledStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Cancelled provider request status was not found in lookup data.",
                        "حالة إلغاء طلب مزود الخدمة غير موجودة في بيانات النظام."
                    );
                }

                request.StatusId = cancelledStatusId.Value;
                request.IsActive = false;
                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service provider request cancelled successfully.",
                    "تم إلغاء طلب مزود الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cancelling service provider request {RequestId} for user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the service provider request.",
                    "حدث خطأ أثناء إلغاء طلب مزود الخدمة."
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceProviderRequestDTO>>> GetMySentServiceProviderRequests(int currentUserId)
        {
            try
            {
                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<List<GetServiceProviderRequestDTO>>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var requests = await _context.ServiceProviderRequests
                    .Where(x =>
                        x.LocationManagerId == locationManagerId.Value &&
                        !x.IsDeleted)
                    .Select(x => new GetServiceProviderRequestDTO
                    {
                        Id = x.Id,

                        ServiceRequestToLocationManagerId = x.RequestToLocationManagerToBookServiceId,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        ServiceProviderId = x.ServiceProviderId,
                        ServiceProviderName = x.ServiceProvider.User.Name,

                        MessageToProvider = x.MessageToProvider,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        ServiceProviderResponse = x.ServiceProviderResponse,
                        RespondedAtUtc = x.RespondedAtUtc,
                        RespondedByUserId = x.RespondedByUserId,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new GetServiceProviderRequestItemDTO
                            {
                                Id = i.Id,

                                RequestToLocationManagerItemId =
                                    i.RequestToLocationManagerToBookServiceItemId,

                                ServiceId = i.ServiceId,
                                ServiceName = i.Service.ServiceName,

                                ServiceTypeId =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId,

                                ServiceTypeName =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId != null
                                        ? i.RequestToLocationManagerToBookServiceItem.ServiceType!.Name
                                        : null,

                                // CustomServiceType =
                                //     i.RequestToLocationManagerToBookServiceItem.CustomServiceType,

                                RequestedQuantity =
                                    i.RequestToLocationManagerToBookServiceItem.Quantity,

                                AvailableQuantity =
                                    i.Service.AvailableQuantity,

                                StartDate =
                                    i.RequestToLocationManagerToBookServiceItem.StartDate,

                                EndDate =
                                    i.RequestToLocationManagerToBookServiceItem.EndDate,
                                
                                Details =
                                    i.RequestToLocationManagerToBookServiceItem.Details
                            })
                            .ToList()
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<GetServiceProviderRequestDTO>>.SuccessResponse(
                        requests,
                        "No sent service provider requests found.",
                        "لا توجد طلبات مرسلة لمزودي الخدمات."
                    );
                }

                return ApiResponse<List<GetServiceProviderRequestDTO>>.SuccessResponse(
                    requests,
                    "Sent service provider requests retrieved successfully.",
                    "تم استرجاع طلبات مزودي الخدمات المرسلة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving sent service provider requests for location manager user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<GetServiceProviderRequestDTO>>.FailureResponse(
                    "An error occurred while retrieving sent service provider requests.",
                    "حدث خطأ أثناء استرجاع طلبات مزودي الخدمات المرسلة."
                );
            }
        }

        public async Task<ApiResponse<GetServiceProviderRequestDTO>> GetMySentServiceProviderRequestById(int requestId,int currentUserId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var request = await _context.ServiceProviderRequests
                    .Where(x =>
                        x.Id == requestId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        !x.IsDeleted)
                    .Select(x => new GetServiceProviderRequestDTO
                    {
                        Id = x.Id,

                        ServiceRequestToLocationManagerId = x.RequestToLocationManagerToBookServiceId,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        ServiceProviderId = x.ServiceProviderId,
                        ServiceProviderName = x.ServiceProvider.User.Name,

                        MessageToProvider = x.MessageToProvider,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        ServiceProviderResponse = x.ServiceProviderResponse,
                        RespondedAtUtc = x.RespondedAtUtc,
                        RespondedByUserId = x.RespondedByUserId,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new GetServiceProviderRequestItemDTO
                            {
                                Id = i.Id,

                                RequestToLocationManagerItemId =
                                    i.RequestToLocationManagerToBookServiceItemId,

                                ServiceId = i.ServiceId,
                                ServiceName = i.Service.ServiceName,

                                ServiceTypeId =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId,

                                ServiceTypeName =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId != null
                                        ? i.RequestToLocationManagerToBookServiceItem.ServiceType!.Name
                                        : null,

                                // CustomServiceType =
                                //     i.RequestToLocationManagerToBookServiceItem.CustomServiceType,

                                RequestedQuantity =
                                    i.RequestToLocationManagerToBookServiceItem.Quantity,

                                AvailableQuantity =
                                    i.Service.AvailableQuantity,

                                StartDate =
                                    i.RequestToLocationManagerToBookServiceItem.StartDate,

                                EndDate =
                                    i.RequestToLocationManagerToBookServiceItem.EndDate,

                                Details =
                                    i.RequestToLocationManagerToBookServiceItem.Details
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider request was not found.",
                        "طلب مزود الخدمة غير موجود."
                    );
                }

                return ApiResponse<GetServiceProviderRequestDTO>.SuccessResponse(
                    request,
                    "Service provider request retrieved successfully.",
                    "تم استرجاع طلب مزود الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving sent service provider request {RequestId} for location manager user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "An error occurred while retrieving the service provider request.",
                    "حدث خطأ أثناء استرجاع طلب مزود الخدمة."
                );
            }
        }

        public async Task<ApiResponse<List<GetServiceProviderRequestDTO>>> GetMyReceivedServiceProviderRequests(int currentUserId)
        {
            try
            {
                var serviceProviderId = await GetServiceProviderIdByUserId(currentUserId);

                if (serviceProviderId == null)
                {
                    return ApiResponse<List<GetServiceProviderRequestDTO>>.FailureResponse(
                        "Service provider profile was not found.",
                        "لم يتم العثور على ملف مزود الخدمة."
                    );
                }

                var requests = await _context.ServiceProviderRequests
                    .Where(x =>
                        x.ServiceProviderId == serviceProviderId.Value &&
                        !x.IsDeleted)
                    .Select(x => new GetServiceProviderRequestDTO
                    {
                        Id = x.Id,

                        ServiceRequestToLocationManagerId = x.RequestToLocationManagerToBookServiceId,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        ServiceProviderId = x.ServiceProviderId,
                        ServiceProviderName = x.ServiceProvider.User.Name,

                        MessageToProvider = x.MessageToProvider,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        ServiceProviderResponse = x.ServiceProviderResponse,
                        RespondedAtUtc = x.RespondedAtUtc,
                        RespondedByUserId = x.RespondedByUserId,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new GetServiceProviderRequestItemDTO
                            {
                                Id = i.Id,

                                RequestToLocationManagerItemId =
                                    i.RequestToLocationManagerToBookServiceItemId,

                                ServiceId = i.ServiceId,
                                ServiceName = i.Service.ServiceName,

                                ServiceTypeId =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId,

                                ServiceTypeName =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId != null
                                        ? i.RequestToLocationManagerToBookServiceItem.ServiceType!.Name
                                        : null,

                                // CustomServiceType =
                                //     i.RequestToLocationManagerToBookServiceItem.CustomServiceType,

                                RequestedQuantity =
                                    i.RequestToLocationManagerToBookServiceItem.Quantity,

                                AvailableQuantity =
                                    i.Service.AvailableQuantity,

                                StartDate =
                                    i.RequestToLocationManagerToBookServiceItem.StartDate,

                                EndDate =
                                    i.RequestToLocationManagerToBookServiceItem.EndDate,

                                Details =
                                    i.RequestToLocationManagerToBookServiceItem.Details
                            })
                            .ToList()
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<GetServiceProviderRequestDTO>>.SuccessResponse(
                        requests,
                        "No received service provider requests found.",
                        "لا توجد طلبات مستلمة من مديري المواقع."
                    );
                }

                return ApiResponse<List<GetServiceProviderRequestDTO>>.SuccessResponse(
                    requests,
                    "Received service provider requests retrieved successfully.",
                    "تم استرجاع طلبات مزود الخدمة المستلمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving received service provider requests for user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<GetServiceProviderRequestDTO>>.FailureResponse(
                    "An error occurred while retrieving received service provider requests.",
                    "حدث خطأ أثناء استرجاع طلبات مزود الخدمة المستلمة."
                );
            }
        }

        public async Task<ApiResponse<GetServiceProviderRequestDTO>> GetMyReceivedServiceProviderRequestById(int requestId,int currentUserId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var serviceProviderId = await GetServiceProviderIdByUserId(currentUserId);

                if (serviceProviderId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider profile was not found.",
                        "لم يتم العثور على ملف مزود الخدمة."
                    );
                }

                var request = await _context.ServiceProviderRequests
                    .Where(x =>
                        x.Id == requestId &&
                        x.ServiceProviderId == serviceProviderId.Value &&
                        !x.IsDeleted)
                    .Select(x => new GetServiceProviderRequestDTO
                    {
                        Id = x.Id,

                        ServiceRequestToLocationManagerId = x.RequestToLocationManagerToBookServiceId,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        ServiceProviderId = x.ServiceProviderId,
                        ServiceProviderName = x.ServiceProvider.User.Name,

                        MessageToProvider = x.MessageToProvider,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        ServiceProviderResponse = x.ServiceProviderResponse,
                        RespondedAtUtc = x.RespondedAtUtc,
                        RespondedByUserId = x.RespondedByUserId,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new GetServiceProviderRequestItemDTO
                            {
                                Id = i.Id,

                                RequestToLocationManagerItemId =
                                    i.RequestToLocationManagerToBookServiceItemId,

                                ServiceId = i.ServiceId,
                                ServiceName = i.Service.ServiceName,

                                ServiceTypeId =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId,

                                ServiceTypeName =
                                    i.RequestToLocationManagerToBookServiceItem.ServiceTypeId != null
                                        ? i.RequestToLocationManagerToBookServiceItem.ServiceType!.Name
                                        : null,

                                // CustomServiceType =
                                //     i.RequestToLocationManagerToBookServiceItem.CustomServiceType,

                                RequestedQuantity =
                                    i.RequestToLocationManagerToBookServiceItem.Quantity,

                                AvailableQuantity =
                                    i.Service.AvailableQuantity,

                                StartDate =
                                    i.RequestToLocationManagerToBookServiceItem.StartDate,

                                EndDate =
                                    i.RequestToLocationManagerToBookServiceItem.EndDate,

                                Details =
                                    i.RequestToLocationManagerToBookServiceItem.Details
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider request was not found.",
                        "طلب مزود الخدمة غير موجود."
                    );
                }

                return ApiResponse<GetServiceProviderRequestDTO>.SuccessResponse(
                    request,
                    "Service provider request retrieved successfully.",
                    "تم استرجاع طلب مزود الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving received service provider request {RequestId} for user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "An error occurred while retrieving the service provider request.",
                    "حدث خطأ أثناء استرجاع طلب مزود الخدمة."
                );
            }
        }

        public async Task<ApiResponse<GetServiceProviderRequestDTO>> RespondToServiceProviderRequest(RespondServiceProviderRequestDTO request,int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (request == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                if (request.RequestId <= 0)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var serviceProviderId = await GetServiceProviderIdByUserId(currentUserId);

                if (serviceProviderId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider profile was not found.",
                        "لم يتم العثور على ملف مزود الخدمة."
                    );
                }

                var serviceProviderRequest = await _context.ServiceProviderRequests
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.RequestId &&
                        x.ServiceProviderId == serviceProviderId.Value &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (serviceProviderRequest == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Service provider request was not found.",
                        "طلب مزود الخدمة غير موجود."
                    );
                }

                var pendingProviderReviewStatusId = await GetStatus(
                    "ServiceProviderRequestStatus",
                    "PendingProviderReview"
                );

                if (pendingProviderReviewStatusId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Pending provider review status was not found in lookup data.",
                        "حالة انتظار مراجعة مزود الخدمة غير موجودة في بيانات النظام."
                    );
                }

                if (serviceProviderRequest.StatusId != pendingProviderReviewStatusId.Value)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Only pending provider requests can be responded to.",
                        "يمكن الرد فقط على طلبات مزود الخدمة قيد الانتظار."
                    );
                }

                var targetStatusName = request.IsAccepted
                    ? "AcceptedByProvider"
                    : "RejectedByProvider";

                var targetStatusId = await GetStatus(
                    "ServiceProviderRequestStatus",
                    targetStatusName
                );

                if (targetStatusId == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        $"{targetStatusName} status was not found in lookup data.",
                        "حالة رد مزود الخدمة غير موجودة في بيانات النظام."
                    );
                }

                serviceProviderRequest.StatusId = targetStatusId.Value;
                serviceProviderRequest.ServiceProviderResponse = string.IsNullOrWhiteSpace(request.ResponseMessage)
                    ? null
                    : request.ResponseMessage.Trim();

                serviceProviderRequest.RespondedAtUtc = DateTime.UtcNow;
                serviceProviderRequest.RespondedByUserId = currentUserId;
                serviceProviderRequest.UpdatedAt = DateTime.UtcNow;
                serviceProviderRequest.UpdatedBy = currentUserId.ToString();

                if (request.IsAccepted)
                {
                    var acceptedBookingStatusId = await GetStatus(
                        "ServiceBookingStatus",
                        "Accepted"
                    );

                    var rejectedBookingStatusId = await GetStatus(
                        "ServiceBookingStatus",
                        "Rejected"
                    );

                    var cancelledBookingStatusId = await GetStatus(
                        "ServiceBookingStatus",
                        "Cancelled"
                    );

                    if (acceptedBookingStatusId == null ||
                        rejectedBookingStatusId == null ||
                        cancelledBookingStatusId == null)
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "Service booking status lookup data is missing.",
                            "بيانات حالات حجز الخدمة غير مكتملة."
                        );
                    }

                    var ignoredBookingStatusIds = new[]
                    {
                        rejectedBookingStatusId.Value,
                        cancelledBookingStatusId.Value
                    };

                    var providerRequestItems = await _context.ServiceProviderRequestItems
                        .Where(x =>
                            x.ServiceProviderRequestId == serviceProviderRequest.Id &&
                            x.IsActive &&
                            !x.IsDeleted)
                        .Select(x => new
                        {
                            ServiceProviderRequestItemId = x.Id,
                            x.ServiceId,

                            StartDate = x.RequestToLocationManagerToBookServiceItem.StartDate,
                            EndDate = x.RequestToLocationManagerToBookServiceItem.EndDate,
                            Quantity = x.RequestToLocationManagerToBookServiceItem.Quantity,
                            Details = x.RequestToLocationManagerToBookServiceItem.Details,

                            AvailableQuantity = x.Service.AvailableQuantity,

                            LocationBookingId = x.ServiceProviderRequest
                                .RequestToLocationManagerToBookService
                                .LocationBookingId,

                            ProductionCompanyUserId = x.ServiceProviderRequest
                                .RequestToLocationManagerToBookService
                                .ProductionCompany
                                .UserId
                        })
                        .ToListAsync();

                    if (!providerRequestItems.Any())
                    {
                        return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                            "Provider request has no active items.",
                            "طلب مزود الخدمة لا يحتوي على خدمات فعالة."
                        );
                    }

                    foreach (var item in providerRequestItems)
                    {
                        var requestedQuantity = item.Quantity ?? 1;

                        if (requestedQuantity <= 0)
                        {
                            return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                                "Requested quantity must be greater than zero.",
                                "يجب أن تكون الكمية المطلوبة أكبر من صفر."
                            );
                        }

                        if (item.AvailableQuantity.HasValue &&
                            item.AvailableQuantity.Value < requestedQuantity)
                        {
                            return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                                "Selected service does not have enough available quantity.",
                                "الخدمة المحددة لا تحتوي على كمية متوفرة كافية."
                            );
                        }

                        var bookedQuantityInSamePeriod = await _context.ServiceBookings
                            .Where(b =>
                                b.ServiceId == item.ServiceId &&
                                !b.IsDeleted &&
                                !ignoredBookingStatusIds.Contains(b.StatusId) &&
                                b.BookingStartDate < item.EndDate &&
                                b.BookingEndDate > item.StartDate)
                            .SumAsync(b => b.Quantity ?? 1);

                        if (item.AvailableQuantity.HasValue &&
                            bookedQuantityInSamePeriod + requestedQuantity > item.AvailableQuantity.Value)
                        {
                            return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                                "Selected service does not have enough available quantity for the selected dates.",
                                "الخدمة المحددة لا تحتوي على كمية متوفرة كافية في التواريخ المحددة."
                            );
                        }

                        var booking = new ServiceBooking
                        {
                            ServiceId = item.ServiceId,

                            RequesterId = item.ProductionCompanyUserId,

                            LocationBookingId = item.LocationBookingId,
                            ServiceProviderRequestItemId = item.ServiceProviderRequestItemId,

                            Quantity = requestedQuantity,
                            Notes = item.Details,

                            StatusId = acceptedBookingStatusId.Value,

                            BookingStartDate = item.StartDate,
                            BookingEndDate = item.EndDate,

                            CreatedAt = DateTime.UtcNow,
                            CreatedBy = currentUserId.ToString(),
                            IsActive = true,
                            IsDeleted = false
                        };

                        await _context.ServiceBookings.AddAsync(booking);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await GetServiceProviderRequestDtoById(serviceProviderRequest.Id);

                if (response == null)
                {
                    return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                        "Request was updated, but response data could not be loaded.",
                        "تم تحديث الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<GetServiceProviderRequestDTO>.SuccessResponse(
                    response,
                    request.IsAccepted
                        ? "Service provider request accepted successfully."
                        : "Service provider request rejected successfully.",
                    request.IsAccepted
                        ? "تم قبول طلب مزود الخدمة بنجاح."
                        : "تم رفض طلب مزود الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error responding to service provider request {RequestId} for user {UserId}",
                    request?.RequestId,
                    currentUserId
                );

                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "An error occurred while responding to the service provider request.",
                    "حدث خطأ أثناء الرد على طلب مزود الخدمة."
                );
            }
        }


        #region Private Helper Methods
        private ApiResponse<GetServiceProviderRequestDTO>? ValidateSendServiceRequestToProvider(SendServiceRequestToProviderDTO request)
        {
            if (request == null)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            if (request.ServiceRequestToLocationManagerId <= 0)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Service request is required.",
                    "طلب الخدمات مطلوب."
                );
            }

            if (request.ServiceProviderId <= 0)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Service provider is required.",
                    "مزود الخدمة مطلوب."
                );
            }

            if (request.Items == null || !request.Items.Any())
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "At least one service item is required.",
                    "يجب إضافة خدمة واحدة على الأقل."
                );
            }

            if (request.Items.Any(x => x.RequestToLocationManagerItemId <= 0))
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid request item.",
                    "عنصر الطلب غير صالح."
                );
            }

            if (request.Items.Any(x => x.ServiceId <= 0))
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid service.",
                    "الخدمة غير صالحة."
                );
            }

            var duplicatedRequestItems = request.Items
                .GroupBy(x => x.RequestToLocationManagerItemId)
                .Any(g => g.Count() > 1);

            if (duplicatedRequestItems)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "The same request item cannot be sent more than once in the same provider request.",
                    "لا يمكن إرسال نفس عنصر الطلب أكثر من مرة في نفس طلب مزود الخدمة."
                );
            }

            return null;
        }

        private async Task<bool> ProviderSupportsRequestedServiceType(int serviceProviderId,int? serviceTypeId,string? customServiceType)
        {
            if (serviceTypeId.HasValue)
            {
                return await _context.ServiceProviderServiceTypes
                    .AnyAsync(x =>
                        x.ServiceProviderId == serviceProviderId &&
                        x.ServiceTypeId == serviceTypeId.Value &&
                        !x.IsCustom &&
                        x.IsActive &&
                        !x.IsDeleted);
            }

            if (!string.IsNullOrWhiteSpace(customServiceType))
            {
                var normalizedCustomType = customServiceType.Trim().ToLower();

                return await _context.ServiceProviderServiceTypes
                    .AnyAsync(x =>
                        x.ServiceProviderId == serviceProviderId &&
                        x.IsCustom &&
                        x.CustomServiceTypeName != null &&
                        x.CustomServiceTypeName.Trim().ToLower() == normalizedCustomType &&
                        x.IsActive &&
                        !x.IsDeleted);
            }

            return false;
        }

        private bool DoesSelectedServiceMatchRequestedItem(int? selectedServiceTypeId,string? selectedCustomServiceType,bool selectedServiceIsCustom,int? requestedServiceTypeId,string? requestedCustomServiceType)
        {
            if (requestedServiceTypeId.HasValue)
            {
                return selectedServiceTypeId == requestedServiceTypeId.Value &&
                       !selectedServiceIsCustom;
            }

            if (!string.IsNullOrWhiteSpace(requestedCustomServiceType))
            {
                if (!selectedServiceIsCustom ||
                    string.IsNullOrWhiteSpace(selectedCustomServiceType))
                {
                    return false;
                }

                return selectedCustomServiceType.Trim().ToLower() ==
                       requestedCustomServiceType.Trim().ToLower();
            }

            return false;
        }

        private async Task<GetServiceProviderRequestDTO?> GetServiceProviderRequestDtoById(int requestId)
        {
            return await _context.ServiceProviderRequests
                .Where(x => x.Id == requestId)
                .Select(x => new GetServiceProviderRequestDTO
                {
                    Id = x.Id,

                    ServiceRequestToLocationManagerId = x.RequestToLocationManagerToBookServiceId,

                    LocationManagerId = x.LocationManagerId,
                    LocationManagerName = x.LocationManager.User.Name,

                    ServiceProviderId = x.ServiceProviderId,
                    ServiceProviderName = x.ServiceProvider.User.Name,

                    MessageToProvider = x.MessageToProvider,

                    StatusId = x.StatusId,
                    StatusName = x.Status.Name,

                    ServiceProviderResponse = x.ServiceProviderResponse,
                    RespondedAtUtc = x.RespondedAtUtc,
                    RespondedByUserId = x.RespondedByUserId,

                    CreatedAt = x.CreatedAt,
                    IsActive = x.IsActive,
                    IsDeleted = x.IsDeleted,

                    Items = x.Items
                        .Where(i => !i.IsDeleted)
                        .Select(i => new GetServiceProviderRequestItemDTO
                        {
                            Id = i.Id,

                            RequestToLocationManagerItemId =
                                i.RequestToLocationManagerToBookServiceItemId,

                            ServiceId = i.ServiceId,
                            ServiceName = i.Service.ServiceName,

                            ServiceTypeId = i.RequestToLocationManagerToBookServiceItem.ServiceTypeId,

                            ServiceTypeName = i.RequestToLocationManagerToBookServiceItem.ServiceTypeId != null
                                ? i.RequestToLocationManagerToBookServiceItem.ServiceType!.Name
                                : null,

                            // CustomServiceType =
                            //     i.RequestToLocationManagerToBookServiceItem.CustomServiceType,

                            RequestedQuantity =
                                i.RequestToLocationManagerToBookServiceItem.Quantity,

                            AvailableQuantity =
                                i.Service.AvailableQuantity,

                            StartDate =
                                i.RequestToLocationManagerToBookServiceItem.StartDate,

                            EndDate =
                                i.RequestToLocationManagerToBookServiceItem.EndDate,

                            Details =
                                i.RequestToLocationManagerToBookServiceItem.Details
                        })
                        .ToList()
                })
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
        private async Task<int?> GetStatus(string categoryName, string statusName)
        {
            return await _context.LookupItems
                .Where(x =>
                    x.Name == statusName &&
                    x.IsActive &&
                    !x.IsDeleted &&
                    x.LookupCategory.Name == categoryName &&
                    x.LookupCategory.IsActive &&
                    !x.LookupCategory.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        private ApiResponse<GetServiceProviderRequestDTO>? ValidateUpdateServiceProviderRequest(
    UpdateServiceProviderRequestDTO request)
        {
            if (request == null)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            if (request.RequestId <= 0)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid request id.",
                    "رقم الطلب غير صالح."
                );
            }

            if (request.ServiceProviderId.HasValue && request.ServiceProviderId.Value <= 0)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid service provider.",
                    "مزود الخدمة غير صالح."
                );
            }

            // Partial update:
            // If Items is null or empty, do not update items.
            if (request.Items == null || !request.Items.Any())
            {
                return null;
            }

            if (request.Items.Any(x =>
                    !x.RequestToLocationManagerItemId.HasValue ||
                    x.RequestToLocationManagerItemId.Value <= 0))
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid request item.",
                    "عنصر الطلب غير صالح."
                );
            }

            if (request.Items.Any(x =>
                    !x.ServiceId.HasValue ||
                    x.ServiceId.Value <= 0))
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "Invalid service.",
                    "الخدمة غير صالحة."
                );
            }

            var duplicatedRequestItems = request.Items
                .GroupBy(x => x.RequestToLocationManagerItemId!.Value)
                .Any(g => g.Count() > 1);

            if (duplicatedRequestItems)
            {
                return ApiResponse<GetServiceProviderRequestDTO>.FailureResponse(
                    "The same request item cannot be sent more than once in the same provider request.",
                    "لا يمكن إرسال نفس عنصر الطلب أكثر من مرة في نفس طلب مزود الخدمة."
                );
            }

            return null;
        }
        private async Task<int?> GetServiceProviderIdByUserId(int currentUserId)
        {
            return await _context.ServiceProviderProfiles
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
