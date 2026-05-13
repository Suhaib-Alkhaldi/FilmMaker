using FilmMaker.Common;
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

        public ServicesProvidedService(FilmMakerDbContext context, IConfiguration configuration, ILogger<ServicesProvidedService> logger)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<ApiResponse<bool>> AddService(CreateServiceDTO serviceDto, int currentUserId)
        {
            try
            {

                if(serviceDto == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Invalid service data",
                        "بيانات الخدمة غير صالحة"
                    );
                if(string.IsNullOrWhiteSpace(serviceDto.ServiceName))
                    return ApiResponse<bool>.FailureResponse(
                        "Service name is required",
                        "اسم الخدمة مطلوب"
                    );

                if (string.IsNullOrWhiteSpace(serviceDto.Description))
                    return ApiResponse<bool>.FailureResponse(
                        "Service Description is required",
                        "وصف الخدمة مطلوب"
                    );

                if (serviceDto.Price <= 0)
                    return ApiResponse<bool>.FailureResponse(
                        "Price must be greater than zero",
                        "يجب أن يكون السعر أكبر من صفر"
                    );

                var serviceType = await _context.LookupItems.FirstOrDefaultAsync(st => st.Id == serviceDto.ServiceTypeId);

                if (serviceType == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Invalid service type",
                        "نوع الخدمة غير صالح"
                    );

                var isLookupCategoryServiceType = await _context.LookupCategories.AnyAsync(st => st.Id == serviceType.LookupCategoryId && st.Name == "Service Type");


                if (isLookupCategoryServiceType == false)
                    return ApiResponse<bool>.FailureResponse(
                        "Invalid service type",
                        "نوع الخدمة غير صالح"
                    );

                var service = new ServicesProvided
                {
                    ServiceName = serviceDto.ServiceName,
                    Description = serviceDto.Description,
                    Price = serviceDto.Price,
                    ServiceTypeId = serviceDto.ServiceTypeId,
                    ServiceProviderId = currentUserId,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.ToString(),
                    IsActive = true,
                    IsDeleted = false
                };

                await _context.ServicesProvided.AddAsync(service);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service added successfully",
                    "تمت إضافة الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding service for provider {ProviderId}", currentUserId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while adding the service",
                    "حدث خطأ أثناء إضافة الخدمة"
                );
            }
        }

        public async Task<ApiResponse<bool>> UpdateService(UpdateServiceDTO serviceDto, int currentUserId)
        {
            try
            {
                var service = await _context.ServicesProvided.Include(s => s.ServiceProvider)
                    .FirstOrDefaultAsync(s => s.Id == serviceDto.Id && !s.IsDeleted);

                if (service == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );

                if (service.ServiceProvider.UserId != currentUserId)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to update this service",
                        "غير مصرح لك بتعديل هذه الخدمة"
                    );

                if (string.IsNullOrWhiteSpace(serviceDto.ServiceName))
                    return ApiResponse<bool>.FailureResponse(
                        "Service name is required",
                        "اسم الخدمة مطلوب"
                    );

                if (string.IsNullOrWhiteSpace(serviceDto.Description))
                    return ApiResponse<bool>.FailureResponse(
                        "Service Description is required",
                        "وصف الخدمة مطلوب"
                    );

                if (serviceDto.Price <= 0)
                    return ApiResponse<bool>.FailureResponse(
                        "Price must be greater than zero",
                        "يجب أن يكون السعر أكبر من صفر"
                    );
                var serviceType = await _context.LookupItems.FirstOrDefaultAsync(st => st.Id == serviceDto.ServiceTypeId);

                if (serviceType == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Invalid service type",
                        "نوع الخدمة غير صالح"
                    );

                var isLookupCategoryServiceType = await _context.LookupCategories.AnyAsync(st => st.Id == serviceType.LookupCategoryId && st.Name == "Service Type");


                if (isLookupCategoryServiceType == false)
                    return ApiResponse<bool>.FailureResponse(
                        "Invalid service type",
                        "نوع الخدمة غير صالح"
                    );

             

                service.ServiceName = serviceDto.ServiceName;
                service.Description = serviceDto.Description;
                service.Price = serviceDto.Price;
                service.ServiceTypeId = serviceDto.ServiceTypeId;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = currentUserId.ToString();

                _context.ServicesProvided.Update(service);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service updated successfully",
                    "تم تحديث الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating service {ServiceId}", serviceDto.Id);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while updating the service",
                    "حدث خطأ أثناء تحديث الخدمة"
                );
            }
        }

        public async Task<ApiResponse<bool>> DeleteService(int serviceId, int currentUserId)
        {
            try
            {
                var service = await _context.ServicesProvided.Include(s => s.ServiceProvider)
                    .FirstOrDefaultAsync(s => s.Id == serviceId && !s.IsDeleted);

                if (service == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );

                if (service.ServiceProvider.UserId != currentUserId)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to delete this service",
                        "غير مصرح لك بحذف هذه الخدمة"
                    );

                // Soft delete
                service.IsDeleted = true;
                service.IsActive = false;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = currentUserId.ToString();

                _context.ServicesProvided.Update(service);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service deleted successfully",
                    "تم حذف الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", serviceId);
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
                    .Include(s => s.ServiceType)
                    .Include(s => s.ServiceProvider)
                        .ThenInclude(sp => sp.User)
                    .FirstOrDefaultAsync(s => s.Id == serviceId && !s.IsDeleted && s.IsActive);

                if (service == null)
                    return ApiResponse<GetServiceDTO?>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );

                return ApiResponse<GetServiceDTO?>.SuccessResponse(
                    MapToDTO(service),
                    "Service retrieved successfully",
                    "تم استرجاع الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving service {ServiceId}", serviceId);
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
                    .Include(s => s.ServiceType)
                    .Include(s => s.ServiceProvider)
                        .ThenInclude(sp => sp.User)
                    .Where(s => s.IsActive && !s.IsDeleted)
                    .Select(s => MapToDTO(s))
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

        public async Task<ApiResponse<List<GetServiceDTO>>> GetMyServicesByProvider(int currentUserId)
        {
            try
            {
                var services = await _context.ServicesProvided
                    .Include(s => s.ServiceType)
                    .Include(s => s.ServiceProvider)
                        .ThenInclude(sp => sp.User)
                    .Where(s => s.ServiceProviderId == currentUserId && !s.IsDeleted)
                    .Select(s => MapToDTO(s))
                    .ToListAsync();

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Your services retrieved successfully",
                    "تم استرجاع خدماتك بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for provider {ProviderId}", currentUserId);
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
                var services = await _context.ServicesProvided
                    .Include(s => s.ServiceType)
                    .Include(s => s.ServiceProvider)
                        .ThenInclude(sp => sp.User)
                    .Where(s => s.ServiceProviderId == providerId && s.IsActive && !s.IsDeleted)
                    .Select(s => MapToDTO(s))
                    .ToListAsync();

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully",
                    "تم استرجاع الخدمات بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for provider {ProviderId}", providerId);
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
                var services = await _context.ServicesProvided
                    .Include(s => s.ServiceType)
                    .Include(s => s.ServiceProvider)
                    .ThenInclude(sp => sp.User)
                    .Where(s => s.ServiceTypeId == serviceTypeId && s.IsActive && !s.IsDeleted)
                    .Select(s => MapToDTO(s))
                    .ToListAsync();

                return ApiResponse<List<GetServiceDTO>>.SuccessResponse(
                    services,
                    "Services retrieved successfully",
                    "تم استرجاع الخدمات بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving services for type {ServiceTypeId}", serviceTypeId);
                return ApiResponse<List<GetServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving services by type",
                    "حدث خطأ أثناء استرجاع الخدمات حسب النوع"
                );
            }
        }

        public async Task<ApiResponse<bool>> SetServiceActive(int serviceId, int currentUserId)
        {
            try
            {
                var service = await _context.ServicesProvided
                    .Include(s => s.ServiceProvider)
                    .FirstOrDefaultAsync(s =>
                        s.Id == serviceId &&
                        s.ServiceProvider.UserId == currentUserId);

                if (service == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );
                }

                if (currentUserId != service.ServiceProvider.UserId)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to deactivate this service",
                        "غير مصرح لك بتعطيل هذه الخدمة"
                    );


                if (service.IsActive)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Service is already active",
                        "الخدمة مفعلة بالفعل"
                    );
                }

                service.IsActive = true;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service activated successfully",
                    "تم تفعيل الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Error activating service {ServiceId} for user {UserId}",
                    serviceId,
                    currentUserId);

                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while activating the service",
                    "حدث خطأ أثناء تفعيل الخدمة"
                );
            }
        }

        public async Task<ApiResponse<bool>> SetServiceInactive(int serviceId, int currentUserId)
        {
            try
            {
                var service = await _context.ServicesProvided
                    .Include(s => s.ServiceProvider)
                    .FirstOrDefaultAsync(s =>
                        s.Id == serviceId &&
                        s.ServiceProvider.UserId == currentUserId &&
                        !s.IsDeleted);

                if (service == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );



                if (currentUserId != service.ServiceProvider.UserId)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to deactivate this service",
                        "غير مصرح لك بتعطيل هذه الخدمة"
                    );
                if (!service.IsActive)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Service is already inactive",
                        "الخدمة معطلة بالفعل"
                    );
                }


                service.IsActive = false;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = currentUserId.ToString();

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Services deactivated successfully",
                    "تم تعطيل الخدمات بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating services for provider {ProviderId}", currentUserId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while deactivating services",
                    "حدث خطأ أثناء تعطيل الخدمات"
                );
            }
        }

        // ── Private Helper ────────────────────────────────────────────────────────

        private static GetServiceDTO MapToDTO(ServicesProvided service) => new()
        {
            Id = service.Id,
            ServiceName = service.ServiceName,
            Description = service.Description,
            Price = service.Price,
            ServiceTypeId = service.ServiceTypeId,
            ServiceTypeName = service.ServiceType?.Name ?? string.Empty,
            ServiceProviderId = service.ServiceProviderId,
            ServiceProviderName = service.ServiceProvider?.User?.Name ?? string.Empty,
            CreatedDate = service.CreatedAt
        };

        public async Task<ApiResponse<bool>> RestoreDeletedService(int serviceId, int currentUserId)
        {
            try
            {
                var service = await _context.ServicesProvided.Include(s => s.ServiceProvider)
                    .FirstOrDefaultAsync(s => s.Id == serviceId && !s.IsDeleted);

                if (service == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Service not found",
                        "الخدمة غير موجودة"
                    );

                if (service.ServiceProvider.UserId != currentUserId)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to delete this service",
                        "غير مصرح لك بحذف هذه الخدمة"
                    );

                // Soft delete
                service.IsDeleted = false;
                service.IsActive = true;
                service.UpdatedAt = DateTime.UtcNow;
                service.UpdatedBy = currentUserId.ToString();

                _context.ServicesProvided.Update(service);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Service deleted successfully",
                    "تم حذف الخدمة بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting service {ServiceId}", serviceId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while deleting the service",
                    "حدث خطأ أثناء حذف الخدمة"
                );
            }
        }
    }
}