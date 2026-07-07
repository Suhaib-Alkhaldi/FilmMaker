using FilmMaker.Common;
using FilmMaker.DTO.Location.Response;
using FilmMaker.DTO.ServiceProvider;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class ServicesProvidedService : IServicesProvidedService
    {
        private readonly FilmMakerDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<ServicesProvidedService> _logger;
        private readonly IMediaService _mediaService;   

        public ServicesProvidedService(FilmMakerDbContext context, IConfiguration configuration, ILogger<ServicesProvidedService> logger
            ,IMediaService mediaService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _mediaService = mediaService;
        }



        public async Task<ApiResponse<GetServiceDTO>> AddService(CreateServiceDTO serviceDto,int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (serviceDto == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Invalid service data.",
                        "بيانات الخدمة غير صالحة."
                    );
                }

                var serviceProviderId = await GetServiceProviderIdAsync(currentUserId);

                if (serviceProviderId == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Invalid service provider.",
                        "مزود الخدمة غير صالح."
                    );
                }

                if (string.IsNullOrWhiteSpace(serviceDto.ServiceName))
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service name is required.",
                        "اسم الخدمة مطلوب."
                    );
                }

                if (string.IsNullOrWhiteSpace(serviceDto.Description))
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service description is required.",
                        "وصف الخدمة مطلوب."
                    );
                }

                if (serviceDto.Price <= 0)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Price must be greater than zero.",
                        "يجب أن يكون السعر أكبر من صفر."
                    );
                }
                if (serviceDto.AvailableQuantity.HasValue && serviceDto.AvailableQuantity.Value <= 0)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Available quantity must be greater than zero.",
                        "يجب أن تكون الكمية المتوفرة أكبر من صفر."
                    );
                }
                var classificationValidation = await ValidateServiceClassificationAsync(
                    serviceProviderId.Value,
                    serviceDto.ServiceTypeId
                );

                if (classificationValidation != null)
                {
                    return classificationValidation;
                }

                var mediaValidationResult = await _mediaService.ValidateMediaOwnership(
                    serviceDto.MediaIds,
                    currentUserId
                );

                if (!mediaValidationResult.Success)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        mediaValidationResult.MessageEn,
                        mediaValidationResult.MessageAr
                    );
                }

                var mediaItems = mediaValidationResult.Data ?? new List<Media>();

                var mediaBusinessValidation = await ValidateServiceMedia(
                    mediaItems,
                    serviceDto.MediaIds
                );

                if (mediaBusinessValidation != null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        mediaBusinessValidation.MessageEn,
                        mediaBusinessValidation.MessageAr
                    );
                }

                var hasOfficialServiceType = serviceDto.ServiceTypeId.HasValue &&
                                             serviceDto.ServiceTypeId.Value > 0;

                var service = new ServicesProvided
                {
                    ServiceName = serviceDto.ServiceName.Trim(),
                    Description = serviceDto.Description.Trim(),
                    DailyPrice = serviceDto.Price,

                    ServiceTypeId = hasOfficialServiceType
                        ? serviceDto.ServiceTypeId.Value
                        : null,

                    // CustomServiceType = hasOfficialServiceType
                    //     ? null
                    //     : serviceDto.CustomServiceType!.Trim(),
                    AvailableQuantity = serviceDto.AvailableQuantity,

                    IsCustom = !hasOfficialServiceType,

                    ServiceProviderId = serviceProviderId.Value,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false
                };

                await _context.ServicesProvided.AddAsync(service);
                await _context.SaveChangesAsync();

                var serviceMediaLinks = mediaItems
                    .Select(media => new ServicesMedia
                    {
                        ServicesProvidedId = service.Id,
                        MediaId = media.Id,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = currentUserId.ToString(),
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                if (serviceMediaLinks.Any())
                {
                    await _context.ServicesMedia.AddRangeAsync(serviceMediaLinks);
                    await _context.SaveChangesAsync();
                }

                var responseDto = await GetServiceDtoByIdAsync(service.Id);

                if (responseDto == null)
                {
                    await transaction.RollbackAsync();

                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service was created, but response data could not be loaded.",
                        "تم إنشاء الخدمة، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                await transaction.CommitAsync();

                return ApiResponse<GetServiceDTO>.SuccessResponse(
                    responseDto,
                    "Service added successfully.",
                    "تمت إضافة الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error adding service for user {UserId}",
                    currentUserId
                );

                return ApiResponse<GetServiceDTO>.FailureResponse(
                    "An error occurred while adding the service.",
                    "حدث خطأ أثناء إضافة الخدمة."
                );
            }
        }
        public async Task<ApiResponse<GetServiceDTO>> UpdateService(UpdateServiceDTO serviceDto,int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                if (serviceDto == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Invalid service data.",
                        "بيانات الخدمة غير صالحة."
                    );
                }

                if (serviceDto.Id <= 0)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Invalid service id.",
                        "رقم الخدمة غير صالح."
                    );
                }

                var serviceData = await _context.ServicesProvided
                    .Where(s =>
                        s.Id == serviceDto.Id &&
                        !s.IsDeleted)
                    .Select(s => new
                    {
                        Service = s,
                        ServiceProviderUserId = s.ServiceProvider.UserId
                    })
                    .FirstOrDefaultAsync();

                if (serviceData == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service not found.",
                        "الخدمة غير موجودة."
                    );
                }

                if (serviceData.ServiceProviderUserId != currentUserId)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "You are not authorized to update this service.",
                        "غير مصرح لك بتعديل هذه الخدمة."
                    );
                }

                var service = serviceData.Service;

                var finalServiceName = serviceDto.ServiceName != null
                    ? serviceDto.ServiceName.Trim()
                    : service.ServiceName;

                var finalDescription = serviceDto.Description != null
                    ? serviceDto.Description.Trim()
                    : service.Description;

                var finalPrice = serviceDto.Price ?? service.DailyPrice;

                var isServiceTypeProvided = serviceDto.ServiceTypeId.HasValue;
               // var isCustomServiceTypeProvided = serviceDto.CustomServiceType != null;

                var finalServiceTypeId = service.ServiceTypeId;
                var finalCustomServiceType = service.CustomServiceType;

                if (isServiceTypeProvided /*|| isCustomServiceTypeProvided*/)
                {
                    var hasOfficialServiceType = serviceDto.ServiceTypeId.HasValue &&
                                                 serviceDto.ServiceTypeId.Value > 0;

                    finalServiceTypeId = hasOfficialServiceType
                        ? serviceDto.ServiceTypeId.Value
                        : null;

                    // finalCustomServiceType = hasOfficialServiceType
                    //     ? null
                    //     : serviceDto.CustomServiceType?.Trim();
                }

                var finalAvailableQuantity = serviceDto.AvailableQuantity ?? service.AvailableQuantity;
                if (string.IsNullOrWhiteSpace(finalServiceName))
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service name is required.",
                        "اسم الخدمة مطلوب."
                    );
                }

                if (string.IsNullOrWhiteSpace(finalDescription))
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service description is required.",
                        "وصف الخدمة مطلوب."
                    );
                }

                if (finalPrice <= 0)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Price must be greater than zero.",
                        "يجب أن يكون السعر أكبر من صفر."
                    );
                }

                if (finalAvailableQuantity.HasValue && finalAvailableQuantity.Value <= 0)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Available quantity must be greater than zero.",
                        "يجب أن تكون الكمية المتوفرة أكبر من صفر."
                    );
                }

                if (isServiceTypeProvided /*|| isCustomServiceTypeProvided*/)
                {
                    var classificationValidation = await ValidateServiceClassificationAsync(
                        service.ServiceProviderId,
                        finalServiceTypeId
                       // finalCustomServiceType
                    );

                    if (classificationValidation != null)
                    {
                        return classificationValidation;
                    }
                }

                service.ServiceName = finalServiceName;
                service.Description = finalDescription;
                service.DailyPrice = finalPrice;

                service.ServiceTypeId = finalServiceTypeId;
                service.CustomServiceType = finalServiceTypeId.HasValue
                    ? null
                    : finalCustomServiceType;

                service.IsCustom = !finalServiceTypeId.HasValue;

                service.AvailableQuantity = finalAvailableQuantity;

                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = currentUserId.ToString();

                if (serviceDto.MediaIds != null)
                {
                    var mediaValidationResult = await _mediaService.ValidateMediaOwnership(
                        serviceDto.MediaIds,
                        currentUserId
                    );

                    if (!mediaValidationResult.Success)
                    {
                        return ApiResponse<GetServiceDTO>.FailureResponse(
                            mediaValidationResult.MessageEn,
                            mediaValidationResult.MessageAr
                        );
                    }

                    var mediaItems = mediaValidationResult.Data ?? new List<Media>();

                    var mediaBusinessValidation = await ValidateServiceMedia(
                        mediaItems,
                        serviceDto.MediaIds
                    );

                    if (mediaBusinessValidation != null)
                    {
                        return ApiResponse<GetServiceDTO>.FailureResponse(
                            mediaBusinessValidation.MessageEn,
                            mediaBusinessValidation.MessageAr
                        );
                    }

                    var oldServiceMediaLinks = await _context.ServicesMedia
                        .Where(x =>
                            x.ServicesProvidedId == service.Id &&
                            !x.IsDeleted)
                        .ToListAsync();

                    foreach (var oldLink in oldServiceMediaLinks)
                    {
                        oldLink.IsDeleted = true;
                        oldLink.IsActive = false;
                        oldLink.UpdatedAt = DateTime.UtcNow;
                        oldLink.UpdatedBy = currentUserId.ToString();
                    }

                    var newServiceMediaLinks = mediaItems
                        .Select(media => new ServicesMedia
                        {
                            ServicesProvidedId = service.Id,
                            MediaId = media.Id,
                            IsActive = true,
                            IsDeleted = false,
                            CreatedBy = currentUserId.ToString(),
                            CreatedAt = DateTime.UtcNow
                        })
                        .ToList();

                    if (newServiceMediaLinks.Any())
                    {
                        await _context.ServicesMedia.AddRangeAsync(newServiceMediaLinks);
                    }
                }

                await _context.SaveChangesAsync();

                var responseDto = await GetServiceDtoByIdAsync(service.Id);

                if (responseDto == null)
                {
                    await transaction.RollbackAsync();

                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service was updated, but response data could not be loaded.",
                        "تم تحديث الخدمة، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                await transaction.CommitAsync();

                return ApiResponse<GetServiceDTO>.SuccessResponse(
                    responseDto,
                    "Service updated successfully.",
                    "تم تحديث الخدمة بنجاح."
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error updating service {ServiceId} for user {UserId}",
                    serviceDto?.Id,
                    currentUserId
                );

                return ApiResponse<GetServiceDTO>.FailureResponse(
                    "An error occurred while updating the service.",
                    "حدث خطأ أثناء تحديث الخدمة."
                );
            }
        }
        public async Task<ApiResponse<bool>> DeleteService(int serviceId, int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var service = await _context.ServicesProvided
                    .Where(s =>
                        s.Id == serviceId &&
                        !s.IsDeleted)
                    .Select(s => new
                    {
                        Entity = s,
                        ServiceProviderUserId = s.ServiceProvider.UserId
                    })
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );
                }

                if (service.ServiceProviderUserId != currentUserId)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to delete this service",
                        "غير مصرح لك بحذف هذه الخدمة"
                    );
                }

                service.Entity.IsDeleted = true;
                service.Entity.IsActive = false;
                service.Entity.UpdatedAt = DateTime.UtcNow;
                service.Entity.UpdatedBy = currentUserId.ToString();

                var serviceMediaLinks = await _context.ServicesMedia
                    .Where(x =>
                        x.ServicesProvidedId == serviceId &&
                        !x.IsDeleted)
                    .ToListAsync();

                foreach (var mediaLink in serviceMediaLinks)
                {
                    mediaLink.IsDeleted = true;
                    mediaLink.IsActive = false;
                    mediaLink.UpdatedAt = DateTime.UtcNow;
                    mediaLink.UpdatedBy = currentUserId.ToString();
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service deleted successfully",
                    "تم حذف الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error deleting service {ServiceId} for user {UserId}",
                    serviceId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while deleting the service",
                    "حدث خطأ أثناء حذف الخدمة"
                );
            }
        }
        public async Task<ApiResponse<GetServiceDTO?>> GetServiceById(int serviceId)
        {
            try
            {
                var service = await _context.ServicesProvided
                    .Where(s =>
                        s.Id == serviceId &&
                        s.IsActive &&
                        !s.IsDeleted)
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,
                        AvailableQuantity = s.AvailableQuantity,
                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null
                        ? s.ServiceType!.Name: string.Empty,

                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .FirstOrDefaultAsync();

                if (service == null)
                {
                    return ApiResponse<GetServiceDTO?>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );
                }

                return ApiResponse<GetServiceDTO?>.SuccessResponse(
                    service,
                    "Service retrieved successfully",
                    "تم استرجاع الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving service {ServiceId}",
                    serviceId
                );

                return ApiResponse<GetServiceDTO?>.FailureResponse(
                    "An error occurred while retrieving the service",
                    "حدث خطأ أثناء استرجاع الخدمة"
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceDTO>>> GetAllServices()
        {
            try
            {
                var services = await _context.ServicesProvided
                    .Where(s =>
                        s.IsActive &&
                        !s.IsDeleted)
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,
                        AvailableQuantity = s.AvailableQuantity,
                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null? s.ServiceType!.Name: string.Empty,

                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully",
                    "تم استرجاع الخدمات بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving all services");

                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving services",
                    "حدث خطأ أثناء استرجاع الخدمات"
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceDTO>>> GetMyServices(int currentUserId,bool includeDeleted = false)
        {
            try
            {
                var services = await _context.ServicesProvided
                    .Where(s =>
                        s.ServiceProvider.UserId == currentUserId &&
                        s.IsDeleted == includeDeleted)
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,

                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null ? s.ServiceType!.Name : string.Empty,

                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .ToListAsync();
                if (!services.Any())
                {
                    return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                        services,
                        includeDeleted
                            ? "No deleted services found."
                            : "No active services found.",
                        includeDeleted
                            ? "لا توجد خدمات محذوفة."
                            : "لا توجد خدمات فعالة."
                    );
                }

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Your services retrieved successfully",
                    "تم استرجاع خدماتك بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving services for user {UserId}",
                    currentUserId
                );

                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving your services",
                    "حدث خطأ أثناء استرجاع خدماتك"
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceDTO>>> GetServicesByProvider(int providerId)
        {
            try
            {
                if (providerId <= 0)
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Invalid service provider id.",
                        "رقم مزود الخدمة غير صالح."
                    );
                }

                var providerExists = await _context.ServiceProviderProfiles
                    .AnyAsync(p =>
                        p.Id == providerId &&
                        p.IsActive &&
                        !p.IsDeleted);

                if (!providerExists)
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Service provider was not found.",
                        "مزود الخدمة غير موجود."
                    );
                }

                var services = await _context.ServicesProvided
                    .Where(s =>
                        s.ServiceProviderId == providerId &&
                        s.IsActive &&
                        !s.IsDeleted)
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,
                        AvailableQuantity = s.AvailableQuantity,
                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null
                            ? s.ServiceType!.Name
                            : string.Empty,

                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .ToListAsync();

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully",
                    "تم استرجاع الخدمات بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving services for provider {ProviderId}",
                    providerId
                );

                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving the provider's services",
                    "حدث خطأ أثناء استرجاع خدمات مزود الخدمة"
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceDTO>>> GetServicesByServiceType(int serviceTypeId)
        {
            try
            {
                if (serviceTypeId <= 0)
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Invalid service type id.",
                        "رقم نوع الخدمة غير صالح."
                    );
                }

                var serviceTypeExists = await _context.LookupItems
                    .AnyAsync(st =>
                        st.Id == serviceTypeId &&
                        st.IsActive &&
                        !st.IsDeleted &&
                        st.LookupCategory.Name == "ServiceType" &&
                        st.LookupCategory.IsActive &&
                        !st.LookupCategory.IsDeleted);

                if (!serviceTypeExists)
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Service type was not found.",
                        "نوع الخدمة غير موجود."
                    );
                }

                var services = await _context.ServicesProvided
                    .Where(s =>
                        s.ServiceTypeId == serviceTypeId &&
                        !s.IsCustom &&
                        s.IsActive &&
                        !s.IsDeleted)
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,
                        AvailableQuantity = s.AvailableQuantity,
                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null
                            ? s.ServiceType!.Name
                            : string.Empty,

                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .ToListAsync();

                if (!services.Any())
                {
                    return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                        services,
                        "No services found for this service type.",
                        "لا توجد خدمات لهذا النوع."
                    );
                }

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully.",
                    "تم استرجاع الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving services for type {ServiceTypeId}",
                    serviceTypeId
                );

                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving services by type.",
                    "حدث خطأ أثناء استرجاع الخدمات حسب النوع."
                );
            }
        }
        public async Task<ApiResponse<GetServiceDTO>> SetServiceActive(int serviceId, int currentUserId)
        {
            try
            {
                var serviceData = await _context.ServicesProvided
                    .Where(s =>
                        s.Id == serviceId &&
                        !s.IsDeleted)
                    .Select(s => new
                    {
                        Service = s,
                        ServiceProviderUserId = s.ServiceProvider.UserId
                    })
                    .FirstOrDefaultAsync();

                if (serviceData == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );
                }

                if (serviceData.ServiceProviderUserId != currentUserId)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "You are not authorized to activate this service",
                        "غير مصرح لك بتفعيل هذه الخدمة"
                    );
                }

                if (serviceData.Service.IsActive)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service is already active",
                        "الخدمة مفعلة بالفعل"
                    );
                }

                serviceData.Service.IsActive = true;
                serviceData.Service.UpdatedAt = DateTime.UtcNow;
                serviceData.Service.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                var responseDto = await GetServiceDtoByIdAsync(serviceData.Service.Id);

                if (responseDto == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service was activated, but response data could not be loaded.",
                        "تم تفعيل الخدمة، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<GetServiceDTO>.SuccessResponse(
                    responseDto,
                    "Service activated successfully",
                    "تم تفعيل الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error activating service {ServiceId} for user {UserId}",
                    serviceId,
                    currentUserId
                );

                return ApiResponse<GetServiceDTO>.FailureResponse(
                    "An error occurred while activating the service",
                    "حدث خطأ أثناء تفعيل الخدمة"
                );
            }
        }
        public async Task<ApiResponse<GetServiceDTO>> SetServiceInactive(int serviceId, int currentUserId)
        {
            try
            {
                var serviceData = await _context.ServicesProvided
                    .Where(s =>
                        s.Id == serviceId &&
                        !s.IsDeleted)
                    .Select(s => new
                    {
                        Service = s,
                        ServiceProviderUserId = s.ServiceProvider.UserId
                    })
                    .FirstOrDefaultAsync();

                if (serviceData == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );
                }

                if (serviceData.ServiceProviderUserId != currentUserId)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "You are not authorized to deactivate this service",
                        "غير مصرح لك بتعطيل هذه الخدمة"
                    );
                }

                if (!serviceData.Service.IsActive)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service is already inactive",
                        "الخدمة معطلة بالفعل"
                    );
                }

                var rejectedStatusId = await GetLookupItemIdAsync("ServiceBookingStatus", "Rejected");
                var cancelledStatusId = await GetLookupItemIdAsync("ServiceBookingStatus", "Cancelled");
                var completedStatusId = await GetLookupItemIdAsync("ServiceBookingStatus", "Completed");

                if (rejectedStatusId == null || cancelledStatusId == null || completedStatusId == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service booking status lookup data is missing.",
                        "بيانات حالات حجز الخدمة غير مكتملة."
                    );
                }

                var ignoredStatusIds = new[]
                {
            rejectedStatusId.Value,
            cancelledStatusId.Value,
            completedStatusId.Value
        };

                var hasActiveBookings = await _context.ServiceBookings
                    .AnyAsync(b =>
                        b.ServiceId == serviceId &&
                        b.IsActive &&
                        !b.IsDeleted &&
                        !ignoredStatusIds.Contains(b.StatusId));

                if (hasActiveBookings)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "This service cannot be deactivated because it has active booking requests.",
                        "لا يمكن تعطيل هذه الخدمة لأنها مرتبطة بطلبات حجز نشطة."
                    );
                }

                serviceData.Service.IsActive = false;
                serviceData.Service.UpdatedAt = DateTime.UtcNow;
                serviceData.Service.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                var responseDto = await GetServiceDtoByIdAsync(serviceData.Service.Id);

                if (responseDto == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service was deactivated, but response data could not be loaded.",
                        "تم تعطيل الخدمة، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                return ApiResponse<GetServiceDTO>.SuccessResponse(
                    responseDto,
                    "Service deactivated successfully",
                    "تم تعطيل الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error deactivating service {ServiceId} for user {UserId}",
                    serviceId,
                    currentUserId
                );

                return ApiResponse<GetServiceDTO>.FailureResponse(
                    "An error occurred while deactivating the service",
                    "حدث خطأ أثناء تعطيل الخدمة"
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceDTO>>> SearchServices(string searchTerm)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Search term is required.",
                        "كلمة البحث مطلوبة."
                    );
                }

                var normalizedSearchTerm = searchTerm.Trim();

                var services = await _context.ServicesProvided
                    .Where(s =>
                        s.IsActive &&
                        !s.IsDeleted &&
                        (
                            s.ServiceName.Contains(normalizedSearchTerm) ||
                            s.Description.Contains(normalizedSearchTerm) ||
                            (s.ServiceTypeId != null && s.ServiceType!.Name.Contains(normalizedSearchTerm)) ||
                            (s.CustomServiceType != null && s.CustomServiceType.Contains(normalizedSearchTerm))
                        ))
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,

                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null ? s.ServiceType!.Name : string.Empty,
                        AvailableQuantity = s.AvailableQuantity,
                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .ToListAsync();

                if (!services.Any())
                {
                    return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                        services,
                        "No services found matching your search.",
                        "لا توجد خدمات مطابقة لعملية البحث."
                    );
                }

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully.",
                    "تم استرجاع الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error searching services. SearchTerm: {SearchTerm}",
                    searchTerm
                );

                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while searching services.",
                    "حدث خطأ أثناء البحث عن الخدمات."
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceDTO>>> SearchServicesByServiceType(int serviceTypeId,string searchTerm)
        {
            try
            {
                if (serviceTypeId <= 0)
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Invalid service type id.",
                        "رقم نوع الخدمة غير صالح."
                    );
                }

                if (string.IsNullOrWhiteSpace(searchTerm))
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Search term is required.",
                        "كلمة البحث مطلوبة."
                    );
                }

                var serviceTypeExists = await _context.LookupItems
                    .AnyAsync(st =>
                        st.Id == serviceTypeId &&
                        st.IsActive &&
                        !st.IsDeleted &&
                        st.LookupCategory.Name == "ServiceType" &&
                        st.LookupCategory.IsActive &&
                        !st.LookupCategory.IsDeleted);

                if (!serviceTypeExists)
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Service type was not found.",
                        "نوع الخدمة غير موجود."
                    );
                }

                var normalizedSearchTerm = searchTerm.Trim();

                var services = await _context.ServicesProvided
                    .Where(s =>
                        s.IsActive &&
                        !s.IsDeleted &&
                        !s.IsCustom &&
                        s.ServiceTypeId == serviceTypeId &&
                        (
                            s.ServiceName.Contains(normalizedSearchTerm) ||
                            s.Description.Contains(normalizedSearchTerm) ||
                            s.ServiceType!.Name.Contains(normalizedSearchTerm)
                        ))
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,
                        AvailableQuantity = s.AvailableQuantity,
                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null
                            ? s.ServiceType!.Name
                            : string.Empty,

                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .ToListAsync();

                if (!services.Any())
                {
                    return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                        services,
                        "No services found matching your search for this service type.",
                        "لا توجد خدمات مطابقة لعملية البحث لهذا النوع."
                    );
                }

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully.",
                    "تم استرجاع الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error searching services by service type. ServiceTypeId: {ServiceTypeId}, SearchTerm: {SearchTerm}",
                    serviceTypeId,
                    searchTerm
                );

                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while searching services by type.",
                    "حدث خطأ أثناء البحث عن الخدمات حسب النوع."
                );
            }
        }
        public async Task<ApiResponse<GetServiceDTO>> RestoreDeletedService(int serviceId, int currentUserId)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var serviceData = await _context.ServicesProvided
                    .Where(s =>
                        s.Id == serviceId &&
                        s.IsDeleted)
                    .Select(s => new
                    {
                        Service = s,
                        ServiceProviderUserId = s.ServiceProvider.UserId
                    })
                    .FirstOrDefaultAsync();

                if (serviceData == null)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Deleted service not found",
                        "الخدمة المحذوفة غير موجودة"
                    );
                }

                if (serviceData.ServiceProviderUserId != currentUserId)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "You are not authorized to restore this service",
                        "غير مصرح لك باستعادة هذه الخدمة"
                    );
                }

                serviceData.Service.IsDeleted = false;
                serviceData.Service.IsActive = true;
                serviceData.Service.UpdatedAt = DateTime.UtcNow;
                serviceData.Service.UpdatedBy = currentUserId.ToString();

                var serviceMediaLinks = await _context.ServicesMedia
                    .Where(x =>
                        x.ServicesProvidedId == serviceId &&
                        x.IsDeleted)
                    .ToListAsync();

                foreach (var mediaLink in serviceMediaLinks)
                {
                    mediaLink.IsDeleted = false;
                    mediaLink.IsActive = true;
                    mediaLink.UpdatedAt = DateTime.UtcNow;
                    mediaLink.UpdatedBy = currentUserId.ToString();
                }

                await _context.SaveChangesAsync();

                var responseDto = await GetServiceDtoByIdAsync(serviceData.Service.Id);

                if (responseDto == null)
                {
                    await transaction.RollbackAsync();

                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Service was restored, but response data could not be loaded.",
                        "تمت استعادة الخدمة، لكن تعذر تحميل بيانات الاستجابة."
                    );
                }

                await transaction.CommitAsync();

                return ApiResponse<GetServiceDTO>.SuccessResponse(
                    responseDto,
                    "Service restored successfully",
                    "تمت استعادة الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                _logger.LogError(
                    ex,
                    "Error restoring service {ServiceId} for user {UserId}",
                    serviceId,
                    currentUserId
                );

                return ApiResponse<GetServiceDTO>.FailureResponse(
                    "An error occurred while restoring the service",
                    "حدث خطأ أثناء استعادة الخدمة"
                );
            }
        }
        public async Task<ApiResponse<List<GetServiceDTO>>> GetServicesByCustomServiceType(string customServiceTypeName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(customServiceTypeName))
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Custom service type is required.",
                        "نوع الخدمة المخصص مطلوب."
                    );
                }

                var normalizedCustomType = customServiceTypeName.Trim().ToLower();

                var customTypeExists = await _context.ServiceProviderServiceTypes
                    .AnyAsync(x =>
                        x.IsCustom &&
                        x.CustomServiceTypeName != null &&
                        x.CustomServiceTypeName.ToLower() == normalizedCustomType &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!customTypeExists)
                {
                    return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                        "Custom service type was not found.",
                        "نوع الخدمة المخصص غير موجود."
                    );
                }

                var services = await _context.ServicesProvided
                    .Where(s =>
                        s.IsCustom &&
                        s.CustomServiceType != null &&
                        s.CustomServiceType.ToLower() == normalizedCustomType &&
                        s.IsActive &&
                        !s.IsDeleted)
                    .Select(s => new GetServiceDTO
                    {
                        Id = s.Id,
                        ServiceName = s.ServiceName,
                        Description = s.Description,
                        Price = s.DailyPrice,
                        AvailableQuantity = s.AvailableQuantity,
                        ServiceTypeId = s.ServiceTypeId,
                        ServiceTypeName = s.ServiceTypeId != null
                            ? s.ServiceType!.Name
                            : string.Empty,

                        // CustomServiceType = s.CustomServiceType,
                        // IsCustomServiceType = s.IsCustom,

                        ServiceProviderId = s.ServiceProviderId,
                        ServiceProviderName = s.ServiceProvider.User.Name,

                        CreatedDate = s.CreatedAt
                    })
                    .ToListAsync();

                if (!services.Any())
                {
                    return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                        services,
                        "No services found for this custom service type.",
                        "لا توجد خدمات لهذا النوع المخصص."
                    );
                }

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully.",
                    "تم استرجاع الخدمات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error retrieving services by custom service type {CustomServiceTypeName}",
                    customServiceTypeName
                );

                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving services by custom service type.",
                    "حدث خطأ أثناء استرجاع الخدمات حسب نوع الخدمة المخصص."
                );
            }
        }



        #region Helper Methods
        private async Task<int?> GetServiceProviderIdAsync(int userId)
        {
            var profile = await _context.ServiceProviderProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            return profile?.Id;
        }
        private async Task<ApiResponse<bool>?> ValidateServiceMedia(List<Media> mediaItems, List<int> requestedMediaIds)
        {
            var distinctMediaIds = requestedMediaIds.Distinct().ToList();

            var alreadyLinked = await _context.ServicesMedia
                .AnyAsync(x =>
                    distinctMediaIds.Contains(x.MediaId) &&
                    !x.IsDeleted);

            if (alreadyLinked)
            {
                return ApiResponse<bool>.FailureResponse(
                    "One or more media files are already linked to another location.",
                    "واحد أو أكثر من الملفات مرتبط بموقع آخر."
                );
            }

            var hasImage = mediaItems.Any(x =>
                x.MediaType != null &&
                x.MediaType.Name == "Image");



            if (!hasImage)
            {
                return ApiResponse<bool>.FailureResponse(
                    "At least one image are required.",
                    "يجب إضافة صورة واحدة على الأقل ."
                );
            }

            return null;
        }
        private async Task<int?> GetLookupItemIdAsync(string categoryName, string itemName)
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
        private async Task<GetServiceDTO?> GetServiceDtoByIdAsync(int serviceId)
        {
            return await _context.ServicesProvided
                .Where(s => s.Id == serviceId)
                .Select(s => new GetServiceDTO
                {
                    Id = s.Id,
                    ServiceName = s.ServiceName,
                    Description = s.Description,
                    Price = s.DailyPrice,
                    AvailableQuantity = s.AvailableQuantity,
                    ServiceTypeId = s.ServiceTypeId,
                    ServiceTypeName = s.ServiceTypeId != null
                        ? s.ServiceType!.Name
                        : string.Empty,

                    // CustomServiceType = s.CustomServiceType,
                    // IsCustomServiceType = s.IsCustom,

                    ServiceProviderId = s.ServiceProviderId,
                    ServiceProviderName = s.ServiceProvider.User.Name,

                    CreatedDate = s.CreatedAt
                })
                .FirstOrDefaultAsync();
        }
        private async Task<ApiResponse<GetServiceDTO>?> ValidateServiceClassificationAsync(int serviceProviderId,int? serviceTypeId /*string? customServiceTypeName*/)
        {
            var hasOfficialServiceType = serviceTypeId.HasValue && serviceTypeId.Value > 0;
           // var hasCustomServiceType = !string.IsNullOrWhiteSpace(customServiceTypeName);

            if (!hasOfficialServiceType /* && !hasCustomServiceType*/)
            {
                return ApiResponse<GetServiceDTO>.FailureResponse(
                    "Service type or custom service type is required.",
                    "نوع الخدمة أو نوع الخدمة المخصص مطلوب."
                );
            }

            // if (hasOfficialServiceType /*&& hasCustomServiceType*/)
            // {
            //     return ApiResponse<GetServiceDTO>.FailureResponse(
            //         "Choose either service type or custom service type, not both.",
            //         "اختر نوع خدمة رسمي أو نوع خدمة مخصص، وليس الاثنين معًا."
            //     );
            // }

            if (hasOfficialServiceType)
            {
                var serviceTypeExists = await _context.LookupItems
                    .AnyAsync(st =>
                        st.Id == serviceTypeId.Value &&
                        st.IsActive &&
                        !st.IsDeleted &&
                        st.LookupCategory.Name == "ServiceType" &&
                        st.LookupCategory.IsActive &&
                        !st.LookupCategory.IsDeleted);

                if (!serviceTypeExists)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "Invalid service type.",
                        "نوع الخدمة غير صالح."
                    );
                }

                var providerHasThisServiceType = await _context.ServiceProviderServiceTypes
                    .AnyAsync(x =>
                        x.ServiceProviderId == serviceProviderId &&
                        x.ServiceTypeId == serviceTypeId.Value &&
                        x.IsCustom == false &&
                        x.IsActive &&
                        !x.IsDeleted);

                if (!providerHasThisServiceType)
                {
                    return ApiResponse<GetServiceDTO>.FailureResponse(
                        "You cannot add or update a service outside your registered service types.",
                        "لا يمكنك إضافة أو تعديل خدمة خارج أنواع الخدمات المسجلة في ملفك."
                    );
                }

                return null;
            }

            //var normalizedCustomType = customServiceTypeName!.Trim().ToLower();

            // var providerHasThisCustomType = await _context.ServiceProviderServiceTypes
            //     .AnyAsync(x =>
            //         x.ServiceProviderId == serviceProviderId &&
            //         x.IsCustom == true &&
            //         x.CustomServiceTypeName != null &&
            //         //x.CustomServiceTypeName.ToLower() == normalizedCustomType &&
            //         x.IsActive &&
            //         !x.IsDeleted);
            //
            // if (!providerHasThisCustomType)
            // {
            //     return ApiResponse<GetServiceDTO>.FailureResponse(
            //         "You cannot add or update a service under a custom service type that is not registered in your profile.",
            //         "لا يمكنك إضافة أو تعديل خدمة تحت نوع خدمة مخصص غير مسجل في ملفك."
            //     );
            // }

            return null;
        }
        #endregion
    }
}