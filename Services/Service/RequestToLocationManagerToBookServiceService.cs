using FilmMaker.Common;
using FilmMaker.DTO.RequestToLocationManagerToBookService;
using FilmMaker.DTO.ServiceBooking;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace FilmMaker.Services.Service
{
    public class RequestToLocationManagerToBookServiceService : IRequestToLocationManagerToBookServiceService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<RequestToLocationManagerToBookServiceService> _logger;

        public RequestToLocationManagerToBookServiceService(
            FilmMakerDbContext context,
            ILogger<RequestToLocationManagerToBookServiceService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> CreateServiceRequestToLocationManager(CreateRequestToLocationManagerToBookServiceDTO request,int currentUserId)
        {
            try
            {
                var validationResult = ValidateCreateServiceRequestToLocationManager(request);

                if (validationResult != null)
                {
                    return validationResult;
                }

                var currentProductionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);
                var currentLocationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                var isProductionCompany = currentProductionCompanyId.HasValue;
                var isLocationManager = currentLocationManagerId.HasValue;

                if (isProductionCompany == isLocationManager)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "User must be either a production company or a location manager.",
                        "يجب أن يكون المستخدم شركة إنتاج أو مدير موقع."
                    );
                }

                var locationBooking = await _context.LocationBookingRequests
                    .Where(x =>
                        x.Id == request.LocationBookingId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.ProductionCompanyId,
                        x.LocationId,
                        x.LocationManagerId,
                        x.BookingStatusId
                    })
                    .FirstOrDefaultAsync();

                if (locationBooking == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location booking was not found.",
                        "لم يتم العثور على حجز الموقع."
                    );
                }

                int finalProductionCompanyId;
                int finalLocationManagerId;

                if (isProductionCompany)
                {
                    finalProductionCompanyId = currentProductionCompanyId.Value;

                    if (locationBooking.ProductionCompanyId != finalProductionCompanyId)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Location booking was not found for this production company.",
                            "لم يتم العثور على حجز الموقع لهذه الشركة."
                        );
                    }

                    if (locationBooking.LocationManagerId.HasValue)
                    {
                        finalLocationManagerId = locationBooking.LocationManagerId.Value;
                    }
                    else
                    {
                        if (!request.LocationManagerId.HasValue || request.LocationManagerId.Value <= 0)
                        {
                            return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                                "Location manager is required because this location booking does not have an assigned manager.",
                                "مدير الموقع مطلوب لأن حجز الموقع لا يحتوي على مدير موقع مسؤول."
                            );
                        }

                        finalLocationManagerId = request.LocationManagerId.Value;
                    }
                }
                else
                {
                    // Current user is Location Manager.
                    // Never trust LocationManagerId from the body here.
                    // Always use the manager from the token.
                    finalLocationManagerId = currentLocationManagerId.Value;
                    finalProductionCompanyId = locationBooking.ProductionCompanyId;

                    if (locationBooking.LocationManagerId.HasValue &&
                        locationBooking.LocationManagerId.Value != finalLocationManagerId)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "You are not the assigned location manager for this booking.",
                            "أنت لست مدير الموقع المسؤول عن هذا الحجز."
                        );
                    }

                    if (!locationBooking.LocationManagerId.HasValue)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "This direct location booking does not have an assigned location manager.",
                            "حجز الموقع المباشر لا يحتوي على مدير موقع مسؤول."
                        );
                    }
                }

                var acceptedLocationBookingStatusId = await GetStatus(
                    "BookingStatus",
                    "Accepted"
                );

                var confirmedLocationBookingStatusId = await GetStatus(
                    "BookingStatus",
                    "Confirmed"
                );

                if (acceptedLocationBookingStatusId == null || confirmedLocationBookingStatusId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location booking status lookup data is missing.",
                        "بيانات حالات حجز الموقع غير مكتملة."
                    );
                }

                var allowedLocationBookingStatusIds = new[]
                {
                    acceptedLocationBookingStatusId.Value,
                    confirmedLocationBookingStatusId.Value
                };

                if (!allowedLocationBookingStatusIds.Contains(locationBooking.BookingStatusId))
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Services can only be requested for accepted or confirmed location bookings.",
                        "يمكن طلب الخدمات فقط لحجوزات المواقع المقبولة أو المؤكدة."
                    );
                }

                var locationManagerExists = await _context.LocationManagerProfiles
                    .AnyAsync(x =>
                        x.Id == finalLocationManagerId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!locationManagerExists)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location manager was not found.",
                        "مدير الموقع غير موجود."
                    );
                }

                var pendingManagerReviewStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    "PendingManagerReview"
                );

                if (pendingManagerReviewStatusId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Pending manager review status was not found in lookup data.",
                        "حالة انتظار مراجعة مدير الموقع غير موجودة في بيانات النظام."
                    );
                }

                foreach (var item in request.Items)
                {
                    if (item.ServiceTypeId.HasValue)
                    {
                        var serviceTypeExists = await _context.LookupItems
                            .AnyAsync(x =>
                                x.Id == item.ServiceTypeId.Value &&
                                x.IsActive &&
                                !x.IsDeleted &&
                                x.LookupCategory.Name == "ServiceType" &&
                                x.LookupCategory.IsActive &&
                                !x.LookupCategory.IsDeleted);

                        if (!serviceTypeExists)
                        {
                            return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                                "One or more service types are invalid.",
                                "واحد أو أكثر من أنواع الخدمات غير صالح."
                            );
                        }
                    }
                }

                var now = DateTime.UtcNow;

                var entity = new RequestToLocationManagerToBookService
                {
                    ProductionCompanyId = finalProductionCompanyId,
                    LocationManagerId = finalLocationManagerId,
                    LocationBookingId = request.LocationBookingId,

                    GeneralNotes = string.IsNullOrWhiteSpace(request.GeneralNotes)
                        ? null
                        : request.GeneralNotes.Trim(),

                    StatusId = pendingManagerReviewStatusId.Value,

                    CreatedAt = now,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false,

                    Items = request.Items.Select(item => new RequestToLocationManagerToBookServiceItem
                    {
                        ServiceTypeId = item.ServiceTypeId,

                        // CustomServiceType = string.IsNullOrWhiteSpace(item.CustomServiceType)
                        //     ? null
                        //     : item.CustomServiceType.Trim(),

                        StartDate = item.StartDate,
                        EndDate = item.EndDate,

                        Details = string.IsNullOrWhiteSpace(item.Details)
                            ? null
                            : item.Details.Trim(),

                        Quantity = item.Quantity,

                        CreatedAt = now,
                        CreatedBy = currentUserId.ToString(),
                        IsActive = true,
                        IsDeleted = false
                    }).ToList()
                };

                await _context.RequestToLocationManagerToBookService.AddAsync(entity);
                await _context.SaveChangesAsync();

                var response = await GetServiceRequestToLocationManagerDtoById(entity.Id);

                if (response == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Request was created, but response data could not be loaded.",
                        "تم إنشاء الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    response,
                    "Service request sent to location manager successfully.",
                    "تم إرسال طلب الخدمات إلى مدير الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error creating service request to location manager for user {UserId}",
                    currentUserId
                );

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while creating the service request.",
                    "حدث خطأ أثناء إنشاء طلب الخدمات."
                );
            }
        }
        public async Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> UpdateServiceRequestToLocationManager(UpdateRequestToLocationManagerToBookServiceDTO request, int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var validationResult = ValidateUpdateServiceRequestToLocationManager(request);

                if (validationResult != null)
                {
                    return validationResult;
                }

                var currentProductionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);
                var currentLocationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                var isProductionCompany = currentProductionCompanyId.HasValue;
                var isLocationManager = currentLocationManagerId.HasValue;

                if (isProductionCompany == isLocationManager)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "User must be either a production company or a location manager.",
                        "يجب أن يكون المستخدم شركة إنتاج أو مدير موقع."
                    );
                }

                var existingRequest = await _context.RequestToLocationManagerToBookService
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.RequestId &&
                        !x.IsDeleted);

                if (existingRequest == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service request was not found.",
                        "طلب الخدمات غير موجود."
                    );
                }

                if (isProductionCompany &&
                    existingRequest.ProductionCompanyId != currentProductionCompanyId.Value)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service request was not found for this production company.",
                        "لم يتم العثور على طلب الخدمات لهذه الشركة."
                    );
                }

                if (isLocationManager &&
                    existingRequest.LocationManagerId != currentLocationManagerId.Value)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service request was not found for this location manager.",
                        "لم يتم العثور على طلب الخدمات لمدير الموقع هذا."
                    );
                }

                var pendingManagerReviewStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    "PendingManagerReview"
                );

                if (pendingManagerReviewStatusId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Pending manager review status was not found in lookup data.",
                        "حالة انتظار مراجعة مدير الموقع غير موجودة في بيانات النظام."
                    );
                }

                if (existingRequest.StatusId != pendingManagerReviewStatusId.Value)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Only pending service requests can be updated.",
                        "يمكن تعديل طلبات الخدمات قيد انتظار مراجعة مدير الموقع فقط."
                    );
                }

                var finalLocationBookingId = request.LocationBookingId ?? existingRequest.LocationBookingId;

                var locationBooking = await _context.LocationBookingRequests
                    .Where(x =>
                        x.Id == finalLocationBookingId &&
                        x.IsActive &&
                        !x.IsDeleted)
                    .Select(x => new
                    {
                        x.Id,
                        x.ProductionCompanyId,
                        x.LocationId,
                        x.LocationManagerId,
                        StatusId = x.BookingStatusId
                    })
                    .FirstOrDefaultAsync();

                if (locationBooking == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location booking was not found.",
                        "لم يتم العثور على حجز الموقع."
                    );
                }

                int finalProductionCompanyId;
                int finalLocationManagerId;

                if (isProductionCompany)
                {
                    finalProductionCompanyId = currentProductionCompanyId.Value;

                    if (locationBooking.ProductionCompanyId != finalProductionCompanyId)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Location booking was not found for this production company.",
                            "لم يتم العثور على حجز الموقع لهذه الشركة."
                        );
                    }

                    if (locationBooking.LocationManagerId.HasValue)
                    {
                        finalLocationManagerId = locationBooking.LocationManagerId.Value;
                    }
                    else
                    {
                        if (request.LocationManagerId.HasValue && request.LocationManagerId.Value > 0)
                        {
                            finalLocationManagerId = request.LocationManagerId.Value;
                        }
                        else
                        {
                            /*
                             * If this is a direct location booking and no new manager is sent,
                             * keep the current service request manager.
                             */
                            finalLocationManagerId = existingRequest.LocationManagerId;
                        }
                    }
                }
                else
                {
                    finalLocationManagerId = currentLocationManagerId.Value;
                    finalProductionCompanyId = locationBooking.ProductionCompanyId;

                    if (existingRequest.LocationManagerId != finalLocationManagerId)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "You are not the assigned location manager for this service request.",
                            "أنت لست مدير الموقع المسؤول عن طلب الخدمات هذا."
                        );
                    }

                    if (locationBooking.LocationManagerId.HasValue &&
                        locationBooking.LocationManagerId.Value != finalLocationManagerId)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "You are not the assigned location manager for this booking.",
                            "أنت لست مدير الموقع المسؤول عن هذا الحجز."
                        );
                    }
                }

                var acceptedLocationBookingStatusId = await GetStatus(
                    "BookingStatus",
                    "Accepted"
                );

                var confirmedLocationBookingStatusId = await GetStatus(
                    "BookingStatus",
                    "Confirmed"
                );

                if (acceptedLocationBookingStatusId == null || confirmedLocationBookingStatusId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location booking status lookup data is missing.",
                        "بيانات حالات حجز الموقع غير مكتملة."
                    );
                }

                var allowedLocationBookingStatusIds = new[]
                {
                    acceptedLocationBookingStatusId.Value,
                    confirmedLocationBookingStatusId.Value
                };

                if (!allowedLocationBookingStatusIds.Contains(locationBooking.StatusId))
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Services can only be requested for accepted or confirmed location bookings.",
                        "يمكن طلب الخدمات فقط لحجوزات المواقع المقبولة أو المؤكدة."
                    );
                }

                var locationManagerExists = await _context.LocationManagerProfiles
                    .AnyAsync(x =>
                        x.Id == finalLocationManagerId &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!locationManagerExists)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location manager was not found.",
                        "مدير الموقع غير موجود."
                    );
                }

                if (request.Items != null)
                {
                    foreach (var item in request.Items)
                    {
                        if (item.ServiceTypeId.HasValue)
                        {
                            var serviceTypeExists = await _context.LookupItems
                                .AnyAsync(x =>
                                    x.Id == item.ServiceTypeId.Value &&
                                    x.IsActive &&
                                    !x.IsDeleted &&
                                    x.LookupCategory.Name == "ServiceType" &&
                                    x.LookupCategory.IsActive &&
                                    !x.LookupCategory.IsDeleted);

                            if (!serviceTypeExists)
                            {
                                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                                    "One or more service types are invalid.",
                                    "واحد أو أكثر من أنواع الخدمات غير صالح."
                                );
                            }
                        }
                    }
                }

                var now = DateTime.UtcNow;

                existingRequest.LocationBookingId = finalLocationBookingId;
                existingRequest.ProductionCompanyId = finalProductionCompanyId;
                existingRequest.LocationManagerId = finalLocationManagerId;

                if (request.GeneralNotes != null)
                {
                    existingRequest.GeneralNotes = string.IsNullOrWhiteSpace(request.GeneralNotes)
                        ? null
                        : request.GeneralNotes.Trim();
                }

                existingRequest.UpdatedAt = now;
                existingRequest.UpdatedBy = currentUserId.ToString();

                if (request.Items != null)
                {
                    var oldItems = await _context.RequestToLocationManagerToBookServiceItems
                        .Where(x =>
                            x.RequestToLocationManagerToBookServiceId == existingRequest.Id &&
                            !x.IsDeleted)
                        .ToListAsync();

                    foreach (var oldItem in oldItems)
                    {
                        oldItem.IsDeleted = true;
                        oldItem.IsActive = false;
                        oldItem.UpdatedAt = now;
                        oldItem.UpdatedBy = currentUserId.ToString();
                    }

                    var newItems = request.Items.Select(item => new RequestToLocationManagerToBookServiceItem
                    {
                        RequestToLocationManagerToBookServiceId = existingRequest.Id,

                        ServiceTypeId = item.ServiceTypeId,

                        // CustomServiceType = string.IsNullOrWhiteSpace(item.CustomServiceType)
                        //     ? null
                        //     : item.CustomServiceType.Trim(),

                        StartDate = item.StartDate.Value,
                        EndDate = item.EndDate.Value,

                        Details = string.IsNullOrWhiteSpace(item.Details)
                            ? null
                            : item.Details.Trim(),

                        Quantity = item.Quantity,

                        CreatedAt = now,
                        CreatedBy = currentUserId.ToString(),
                        IsActive = true,
                        IsDeleted = false
                    }).ToList();

                    await _context.RequestToLocationManagerToBookServiceItems.AddRangeAsync(newItems);
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                var response = await GetServiceRequestToLocationManagerDtoById(existingRequest.Id);

                if (response == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Request was updated, but response data could not be loaded.",
                        "تم تعديل الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    response,
                    "Service request updated successfully.",
                    "تم تعديل طلب الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error updating service request to location manager {RequestId} for user {UserId}",
                    request?.RequestId,
                    currentUserId
                );

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while updating the service request.",
                    "حدث خطأ أثناء تعديل طلب الخدمات."
                );
            }
        }
        public async Task<ApiResponse<bool>> CancelServiceRequestToLocationManager(int requestId,int currentUserId)
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

                var request = await _context.RequestToLocationManagerToBookService
                    .FirstOrDefaultAsync(x =>
                        x.Id == requestId &&
                        x.ProductionCompanyId == productionCompanyId.Value &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (request == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Service request was not found.",
                        "طلب الخدمات غير موجود."
                    );
                }

                var pendingManagerReviewStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    "PendingManagerReview"
                );

                if (pendingManagerReviewStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Pending manager review status was not found in lookup data.",
                        "حالة انتظار مراجعة مدير الموقع غير موجودة في بيانات النظام."
                    );
                }

                if (request.StatusId != pendingManagerReviewStatusId.Value)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only pending service requests can be cancelled.",
                        "يمكن إلغاء طلبات الخدمات قيد انتظار مراجعة مدير الموقع فقط."
                    );
                }

                var cancelledStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    "Cancelled"
                );

                if (cancelledStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Cancelled service request status was not found in lookup data.",
                        "حالة إلغاء طلب الخدمات غير موجودة في بيانات النظام."
                    );
                }

                request.StatusId = cancelledStatusId.Value;
                request.IsActive = false;
                request.UpdatedAt = DateTime.UtcNow;
                request.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service request cancelled successfully.",
                    "تم إلغاء طلب الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error cancelling service request to location manager {RequestId} for user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while cancelling the service request.",
                    "حدث خطأ أثناء إلغاء طلب الخدمات."
                );
            }
        }
        public async Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> GetMySentServiceRequestsToLocationManager(int currentUserId)
        {
            try
            {
                var productionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);

                if (productionCompanyId == null)
                {
                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                        "Production company profile was not found.",
                        "لم يتم العثور على ملف شركة الإنتاج."
                    );
                }

                var requests = await _context.RequestToLocationManagerToBookService
                    .Where(x =>
                        x.ProductionCompanyId == productionCompanyId.Value &&
                        !x.IsDeleted)
                    .Select(x => new ReadRequestToLocationManagerToBookServiceDTO
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,
                        BookingLocationManagerId = x.LocationBooking.LocationManagerId,
                        BookingLocationManagerName = x.LocationBooking.LocationManager != null
                        ? x.LocationBooking.LocationManager.User.Name: null,

                        LocationBookingId = x.LocationBookingId,

                        GeneralNotes = x.GeneralNotes,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new ReadRequestToLocationManagerToBookServiceItemDTO
                            {
                                Id = i.Id,

                                ServiceTypeId = i.ServiceTypeId,
                                ServiceTypeName = i.ServiceTypeId != null
                                    ? i.ServiceType!.Name
                                    : null,

                                //CustomServiceType = i.CustomServiceType,

                                StartDate = i.StartDate,
                                EndDate = i.EndDate,

                                Details = i.Details,
                                Quantity = i.Quantity
                            })
                            .ToList()
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                        requests,
                        "No sent service requests found.",
                        "لا توجد طلبات خدمات مرسلة."
                    );
                }

                return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                    requests,
                    "Sent service requests retrieved successfully.",
                    "تم استرجاع طلبات الخدمات المرسلة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving sent service requests for user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving sent service requests.",
                    "حدث خطأ أثناء استرجاع طلبات الخدمات المرسلة."
                );
            }
        }
        public async Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> GetMyServiceRequestToLocationManagerById(int requestId,int currentUserId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var productionCompanyId = await GetProductionCompanyIdByUserId(currentUserId);

                if (productionCompanyId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Production company profile was not found.",
                        "لم يتم العثور على ملف شركة الإنتاج."
                    );
                }

                var request = await _context.RequestToLocationManagerToBookService
                    .Where(x =>
                        x.Id == requestId &&
                        x.ProductionCompanyId == productionCompanyId.Value &&
                        !x.IsDeleted)
                    .Select(x => new ReadRequestToLocationManagerToBookServiceDTO
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,


                        BookingLocationManagerId = x.LocationBooking.LocationManagerId,
                        BookingLocationManagerName = x.LocationBooking.LocationManager != null
                        ? x.LocationBooking.LocationManager.User.Name: null,

                        LocationBookingId = x.LocationBookingId,

                        GeneralNotes = x.GeneralNotes,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new ReadRequestToLocationManagerToBookServiceItemDTO
                            {
                                Id = i.Id,

                                ServiceTypeId = i.ServiceTypeId,
                                ServiceTypeName = i.ServiceTypeId != null
                                    ? i.ServiceType!.Name
                                    : null,

                                //CustomServiceType = i.CustomServiceType,

                                StartDate = i.StartDate,
                                EndDate = i.EndDate,

                                Details = i.Details,
                                Quantity = i.Quantity
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service request was not found.",
                        "طلب الخدمات غير موجود."
                    );
                }

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    request,
                    "Service request retrieved successfully.",
                    "تم استرجاع طلب الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving service request {RequestId} for user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while retrieving the service request.",
                    "حدث خطأ أثناء استرجاع طلب الخدمات."
                );
            }
        }
        public async Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> GetMyReceivedServiceRequestsToLocationManager(int currentUserId)
        {
            try
            {
                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var requests = await _context.RequestToLocationManagerToBookService
                    .Where(x =>
                        x.LocationManagerId == locationManagerId.Value &&
                        !x.IsDeleted)
                    .Select(x => new ReadRequestToLocationManagerToBookServiceDTO
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        BookingLocationManagerId = x.LocationBooking.LocationManagerId,
                        BookingLocationManagerName = x.LocationBooking.LocationManager != null? x.LocationBooking.LocationManager.User.Name: null,

                        LocationBookingId = x.LocationBookingId,

                        GeneralNotes = x.GeneralNotes,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new ReadRequestToLocationManagerToBookServiceItemDTO
                            {
                                Id = i.Id,

                                ServiceTypeId = i.ServiceTypeId,
                                ServiceTypeName = i.ServiceTypeId != null
                                    ? i.ServiceType!.Name
                                    : null,

                                //CustomServiceType = i.CustomServiceType,

                                StartDate = i.StartDate,
                                EndDate = i.EndDate,

                                Details = i.Details,
                                Quantity = i.Quantity
                            })
                            .ToList()
                    })
                    .ToListAsync();

                if (!requests.Any())
                {
                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                        requests,
                        "No received service requests found.",
                        "لا توجد طلبات خدمات مستلمة."
                    );
                }

                return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                    requests,
                    "Received service requests retrieved successfully.",
                    "تم استرجاع طلبات الخدمات المستلمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving received service requests for location manager user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving received service requests.",
                    "حدث خطأ أثناء استرجاع طلبات الخدمات المستلمة."
                );
            }
        }

        public async Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> GetReceivedServiceRequestToLocationManagerById(int requestId,int currentUserId)
        {
            try
            {
                if (requestId <= 0)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var request = await _context.RequestToLocationManagerToBookService
                    .Where(x =>
                        x.Id == requestId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        !x.IsDeleted)
                    .Select(x => new ReadRequestToLocationManagerToBookServiceDTO
                    {
                        Id = x.Id,

                        ProductionCompanyId = x.ProductionCompanyId,
                        ProductionCompanyName = x.ProductionCompany.User.Name,

                        LocationManagerId = x.LocationManagerId,
                        LocationManagerName = x.LocationManager.User.Name,

                        BookingLocationManagerId = x.LocationBooking.LocationManagerId,
                        BookingLocationManagerName = x.LocationBooking.LocationManager != null
                        ? x.LocationBooking.LocationManager.User.Name: null,

                        LocationBookingId = x.LocationBookingId,

                        GeneralNotes = x.GeneralNotes,

                        StatusId = x.StatusId,
                        StatusName = x.Status.Name,

                        CreatedAt = x.CreatedAt,
                        IsActive = x.IsActive,
                        IsDeleted = x.IsDeleted,

                        Items = x.Items
                            .Where(i => !i.IsDeleted)
                            .Select(i => new ReadRequestToLocationManagerToBookServiceItemDTO
                            {
                                Id = i.Id,

                                ServiceTypeId = i.ServiceTypeId,
                                ServiceTypeName = i.ServiceTypeId != null
                                    ? i.ServiceType!.Name
                                    : null,

                                //CustomServiceType = i.CustomServiceType,

                                StartDate = i.StartDate,
                                EndDate = i.EndDate,

                                Details = i.Details,
                                Quantity = i.Quantity
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (request == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service request was not found.",
                        "طلب الخدمات غير موجود."
                    );
                }

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    request,
                    "Received service request retrieved successfully.",
                    "تم استرجاع طلب الخدمات المستلم بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving received service request {RequestId} for location manager user {UserId}",
                    requestId,
                    currentUserId
                );

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while retrieving the received service request.",
                    "حدث خطأ أثناء استرجاع طلب الخدمات المستلم."
                );
            }
        }
        public async Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> RespondToServiceRequestToLocationManager(RespondRequestToLocationManagerToBookServiceDTO request,int currentUserId)
        {
            try
            {
                if (request == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Invalid request.",
                        "الطلب غير صحيح."
                    );
                }

                if (request.RequestId <= 0)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Invalid request id.",
                        "رقم الطلب غير صالح."
                    );
                }

                var locationManagerId = await GetLocationManagerIdByUserId(currentUserId);

                if (locationManagerId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                var serviceRequest = await _context.RequestToLocationManagerToBookService
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.RequestId &&
                        x.LocationManagerId == locationManagerId.Value &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (serviceRequest == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service request was not found.",
                        "طلب الخدمات غير موجود."
                    );
                }

                var pendingManagerReviewStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    "PendingManagerReview"
                );

                if (pendingManagerReviewStatusId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Pending manager review status was not found in lookup data.",
                        "حالة انتظار مراجعة مدير الموقع غير موجودة في بيانات النظام."
                    );
                }

                if (serviceRequest.StatusId != pendingManagerReviewStatusId.Value)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Only pending service requests can be responded to.",
                        "يمكن الرد فقط على طلبات الخدمات قيد انتظار مراجعة مدير الموقع."
                    );
                }

                var targetStatusName = request.IsAccepted
                    ? "AcceptedByManager"
                    : "RejectedByManager";

                var targetStatusId = await GetStatus(
                    "ServiceRequestToLocationManagerStatus",
                    targetStatusName
                );

                if (targetStatusId == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        $"{targetStatusName} status was not found in lookup data.",
                        "حالة الرد على طلب الخدمات غير موجودة في بيانات النظام."
                    );
                }

                serviceRequest.StatusId = targetStatusId.Value;
                serviceRequest.LocationManagerResponse = string.IsNullOrWhiteSpace(request.ResponseMessage)
                    ? null
                    : request.ResponseMessage.Trim();

                serviceRequest.RespondedAtUtc = DateTime.UtcNow;
                serviceRequest.RespondedByUserId = currentUserId;
                serviceRequest.UpdatedAt = DateTime.UtcNow;
                serviceRequest.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                var response = await GetServiceRequestToLocationManagerDtoById(serviceRequest.Id);

                if (response == null)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Request was updated, but response data could not be loaded.",
                        "تم تحديث الطلب، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    response,
                    request.IsAccepted
                        ? "Service request accepted successfully."
                        : "Service request rejected successfully.",
                    request.IsAccepted
                        ? "تم قبول طلب الخدمات بنجاح."
                        : "تم رفض طلب الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error responding to service request {RequestId} for user {UserId}",
                    request?.RequestId,
                    currentUserId
                );

                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while responding to the service request.",
                    "حدث خطأ أثناء الرد على طلب الخدمات."
                );
            }
        }



        #region Helper Methods
        private ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>? ValidateCreateServiceRequestToLocationManager(CreateRequestToLocationManagerToBookServiceDTO request)
        {
            if (request == null)
            {
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            if (request.LocationBookingId <= 0)
            {
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Location booking is required.",
                    "حجز الموقع مطلوب."
                );
            }

            if (request.Items == null || !request.Items.Any())
            {
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "At least one service item is required.",
                    "يجب إضافة خدمة واحدة على الأقل."
                );
            }

            foreach (var item in request.Items)
            {
                var hasServiceType = item.ServiceTypeId.HasValue && item.ServiceTypeId.Value > 0;
               // var hasCustomServiceType = !string.IsNullOrWhiteSpace(item.CustomServiceType);

                if (!hasServiceType /* && !hasCustomServiceType*/)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Each service item must have either a service type or a custom service type.",
                        "يجب أن تحتوي كل خدمة على نوع خدمة أو نوع خدمة مخصص."
                    );
                }

                if (hasServiceType/* && hasCustomServiceType*/)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Each service item cannot have both service type and custom service type.",
                        "لا يمكن أن تحتوي الخدمة على نوع خدمة ونوع خدمة مخصص معًا."
                    );
                }

                if (item.StartDate <= DateTime.UtcNow)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service item start date must be in the future.",
                        "يجب أن يكون تاريخ بداية الخدمة في المستقبل."
                    );
                }

                if (item.EndDate <= item.StartDate)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service item end date must be after start date.",
                        "يجب أن يكون تاريخ نهاية الخدمة بعد تاريخ البداية."
                    );
                }

                if (item.Quantity.HasValue && item.Quantity.Value <= 0)
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service item quantity must be greater than zero.",
                        "يجب أن تكون كمية الخدمة أكبر من صفر."
                    );
                }
            }

            return null;
        }



        private async Task<ReadRequestToLocationManagerToBookServiceDTO?> GetServiceRequestToLocationManagerDtoById(int requestId)
        {
            return await _context.RequestToLocationManagerToBookService
                        .Where(x => x.Id == requestId)
                        .Select(x => new ReadRequestToLocationManagerToBookServiceDTO
                        {
                                Id = x.Id,

                                ProductionCompanyId = x.ProductionCompanyId,
                                ProductionCompanyName = x.ProductionCompany.User.Name,

                                LocationManagerId = x.LocationManagerId,
                                LocationManagerName = x.LocationManager.User.Name,

                                LocationBookingId = x.LocationBookingId,

                                BookingLocationManagerId = x.LocationBooking.LocationManagerId,
                                BookingLocationManagerName = x.LocationBooking.LocationManager != null
                                    ? x.LocationBooking.LocationManager.User.Name
                                    : null,

                                GeneralNotes = x.GeneralNotes,

                                StatusId = x.StatusId,
                                StatusName = x.Status.Name,

                                CreatedAt = x.CreatedAt,
                                IsActive = x.IsActive,
                                IsDeleted = x.IsDeleted,

                                Items = x.Items
                                    .Where(i => !i.IsDeleted)
                                    .Select(i => new ReadRequestToLocationManagerToBookServiceItemDTO
                                    {
                                        Id = i.Id,

                                        ServiceTypeId = i.ServiceTypeId,
                                        ServiceTypeName = i.ServiceTypeId != null
                                            ? i.ServiceType!.Name
                                            : null,

                                        //CustomServiceType = i.CustomServiceType,

                                        StartDate = i.StartDate,
                                        EndDate = i.EndDate,

                                        Details = i.Details,
                                        Quantity = i.Quantity
                                    })
                                    .ToList()
                        }).FirstOrDefaultAsync();
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

        private ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>? ValidateUpdateServiceRequestToLocationManager(UpdateRequestToLocationManagerToBookServiceDTO request)
        { 
            if (request == null)
            { 
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح.");
            }
            if (request.RequestId <= 0) 
            {
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Invalid request id.",
                    "رقم الطلب غير صالح.");
            }
            if (request.LocationBookingId.HasValue && request.LocationBookingId.Value <= 0)
            {
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Invalid location booking.",
                    "حجز الموقع غير صالح.");
            }
            if (request.LocationManagerId.HasValue && request.LocationManagerId.Value <= 0) 
            {
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "Invalid location manager.",
                    "مدير الموقع غير صالح.");
            }
            if (request.Items != null)
            {
                if (!request.Items.Any()) 
                {
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "At least one service item is required.",
                        "يجب إضافة خدمة واحدة على الأقل."); 
                }
                foreach (var item in request.Items)
                {
                    var hasServiceType = item.ServiceTypeId.HasValue && item.ServiceTypeId.Value > 0;
                    //var hasCustomServiceType = !string.IsNullOrWhiteSpace(item.CustomServiceType);
                    if (!hasServiceType /*&& !hasCustomServiceType*/) {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Service type or custom service type is required.",
                            "نوع الخدمة أو نوع الخدمة المخصص مطلوب."); 
                    }
                    if (hasServiceType /*&& hasCustomServiceType*/)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Use either service type or custom service type, not both.",
                            "استخدم نوع خدمة أو نوع خدمة مخصص، وليس الاثنين معًا.");
                    }
                    if (item.StartDate >= item.EndDate) 
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Item start date must be before end date.",
                            "تاريخ بداية الخدمة يجب أن يكون قبل تاريخ النهاية.");
                    } 
                    if (item.Quantity.HasValue && item.Quantity.Value <= 0)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Quantity must be greater than zero.",
                            "يجب أن تكون الكمية أكبر من صفر.");
                    }
                }
            } 
            return null;
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