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

        private async Task<int?> GetProductionCompanyIdAsync(int userId)
        {
            var profile = await _context.ProductionCompanyProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            return profile?.Id;
        }

        private async Task<int?> GetLocationManagerIdAsync(int userId)
        {
            var profile = await _context.LocationManagerProfiles
                .FirstOrDefaultAsync(p => p.UserId == userId && !p.IsDeleted);
            return profile?.Id;
        }

        private IQueryable<RequestToLocationManagerToBookService> RequestsWithIncludes() =>
            _context.RequestToLocationManagerToBookService
                .Include(r => r.ProductionCompany)
                    .ThenInclude(pc => pc.User)
                .Include(r => r.ServiceType)
                .Include(r => r.LocationBooking)
                    .ThenInclude(lb => lb.LocationManager)
                        .ThenInclude(lm => lm.User);

        private static ReadRequestToLocationManagerToBookServiceDTO MapToReadDTO(
            RequestToLocationManagerToBookService entity) => new()
            {
                Id = entity.Id,
                ProductionCompany = entity.ProductionCompany?.User?.Name ?? string.Empty,

                ServiceType = entity.ServiceType?.Name ?? string.Empty,

                LocationManager = entity.LocationBooking?.LocationManager?.User?.Name ?? string.Empty,

                StartDate = entity.StartDate,
                EndDate = entity.EndDate,
                Notes = entity.Notes,
                Latitude = entity.Latitude,
                Longitude = entity.Longitude,
                LocationOnGoogleMaps = entity.LocationOnGoogleMaps,

            };


        public async Task<ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>> CreateServiceRequestToLocationManager(
            CreateRequestToLocationManagerToBookServiceDTO request, int currentUserId)
        {
            try
            {
                var productionCompanyId = await GetProductionCompanyIdAsync(currentUserId);
                if (productionCompanyId == null)
                    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Production company profile not found",
                        "لم يتم العثور على ملف شركة الإنتاج"
                    );



                if (request.StartDate <= DateTime.Now)
                    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Start date must be in the future",
                        "يجب أن يكون تاريخ البداية في المستقبل"
                    );

                if (request.StartDate >= request.EndDate)
                    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "End date must be after start date",
                        "يجب أن يكون تاريخ الانتهاء بعد تاريخ البداية"
                    );

                var locationBookingExists = await _context.LocationBookingRequests
                    .AnyAsync(lb => lb.Id == request.LocationBookingId && !lb.IsDeleted);

                if (!locationBookingExists)
                    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Location booking not found",
                        "حجز الموقع غير موجود"
                    );

                var serviceTypeExists = await _context.LookupItems
                    .AnyAsync(l => l.Id == request.ServiceTypeId && !l.IsDeleted);

                if (!serviceTypeExists)
                    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Service type not found",
                        "نوع الخدمة غير موجود"
                    );

                //var duplicateExists = await _context.RequestToLocationManagerToBookServices
                //    .AnyAsync(r =>
                //        r.ProductionCompanyId == request.ProductionCompanyId &&
                //        r.LocationBookingId == request.LocationBookingId &&
                //        r.ServiceTypeId == request.ServiceTypeId &&
                //        !r.IsDeleted);

                //if (duplicateExists)
                //    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                //        "A request for this service type already exists for this booking",
                //        "يوجد طلب بالفعل لنوع الخدمة هذا في هذا الحجز"
                //    );

                var entity = new RequestToLocationManagerToBookService
                {
                    ProductionCompanyId = productionCompanyId.Value,
                    ServiceTypeId = request.ServiceTypeId,
                    LocationBookingId = request.LocationBookingId,
                    StartDate = request.StartDate,
                    EndDate = request.EndDate,
                    Notes = request.Notes,
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsDeleted = false
                };

                if (request.Latitude != 0 && request.Longitude != 0)
                {
                    entity.Latitude = request.Latitude;
                    entity.Longitude = request.Longitude;
                    if (request.LocationOnGoogleMaps.IsNullOrEmpty())
                    {
                        entity.LocationOnGoogleMaps = $"https://www.google.com/maps/search/?api=1&query={request.Latitude},{request.Longitude}";
                    }
                    else
                    {
                        entity.LocationOnGoogleMaps = request.LocationOnGoogleMaps;
                    }
                }
                else if (!request.LocationOnGoogleMaps.IsNullOrEmpty())
                {
                    entity.LocationOnGoogleMaps = request.LocationOnGoogleMaps;
                }
                else
                {
                    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                   "location or latitude and longitude is required",
                   "الموقع أو خط العرض وخط الطول مطلوبان"
                );
                }

                    await _context.RequestToLocationManagerToBookService.AddAsync(entity);
                    await _context.SaveChangesAsync();

                    return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                        request,
                        "Request created successfully",
                        "تم إنشاء الطلب بنجاح"
                    );
                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating service request for user {UserId}", currentUserId);
                return ApiResponse<CreateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while creating the request",
                    "حدث خطأ أثناء إنشاء الطلب"
                );
            }
        }


        public async Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> ReadServiceRequestToLocationManager(
            int currentUserId, bool IsDeleted)
        {
            try
            {
                int? locationManagerId = -1;
                var productionCompanyId = await GetProductionCompanyIdAsync(currentUserId);
                if (productionCompanyId == null)
                {
                    locationManagerId = await GetLocationManagerIdAsync(currentUserId);

                    if (locationManagerId == null)
                    {
                        return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                            "User not Found",
                            "لم يتم العثور على المستخدم"
                        );
                    }
                }

                if (productionCompanyId != null)
                {
                    var requests = await RequestsWithIncludes()
                    .Where(r => r.ProductionCompanyId == productionCompanyId.Value && r.IsDeleted == IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => MapToReadDTO(r))
                    .ToListAsync();

                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                    requests,
                    "Requests retrieved successfully",
                    "تم استرجاع الطلبات بنجاح"
                );
                }
                else
                {
                    var requests = await RequestsWithIncludes()
                    .Where(r => r.LocationBooking.LocationManagerId == locationManagerId.Value && r.IsDeleted == IsDeleted)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => MapToReadDTO(r))
                    .ToListAsync();

                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                    requests,
                    "Requests retrieved successfully",
                    "تم استرجاع الطلبات بنجاح"
                );
                }

                
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading requests for user {UserId}", currentUserId);
                return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>   >.FailureResponse(
                    "An error occurred while retrieving the requests",
                    "حدث خطأ أثناء استرجاع الطلبات"
                );
            }
        }

        public async Task<ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>> ReadServiceRequestToLocationManagerByLocationBookingId(
            int currentUserId, int locationBookingId, bool IsDeleted)
        {
            try
            {
                int? locationManagerId = -1;
                var productionCompanyId = await GetProductionCompanyIdAsync(currentUserId);
                if (productionCompanyId == null)
                {
                    locationManagerId = await GetLocationManagerIdAsync(currentUserId);

                    if (locationManagerId == null)
                    {
                        return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                            "User not Found",
                            "لم يتم العثور على المستخدم"
                        );
                    }
                }
                if (productionCompanyId != null)
                {
                    var requests = await RequestsWithIncludes()
                        .Where(r =>
                            r.ProductionCompanyId == productionCompanyId.Value &&
                            r.LocationBookingId == locationBookingId &&
                            r.IsDeleted == IsDeleted)
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => MapToReadDTO(r))
                        .ToListAsync();

                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                    requests,
                    "Requests retrieved successfully",
                    "تم استرجاع الطلبات بنجاح");
                }
                else
                {
                    var requests = await RequestsWithIncludes()
                        .Where(r =>
                            r.LocationBooking.LocationManagerId == locationManagerId.Value &&
                            r.LocationBookingId == locationBookingId &&
                            r.IsDeleted == IsDeleted)
                        .OrderByDescending(r => r.CreatedAt)
                        .Select(r => MapToReadDTO(r))
                        .ToListAsync();

                    return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.SuccessResponse(
                    requests,
                    "Requests retrieved successfully",
                    "تم استرجاع الطلبات بنجاح"
                );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading requests by location booking {LocationBookingId}", locationBookingId);
                return ApiResponse<List<ReadRequestToLocationManagerToBookServiceDTO>>.FailureResponse(
                    "An error occurred while retrieving the requests",
                    "حدث خطأ أثناء استرجاع الطلبات"
                );
            }
        }


        public async Task<ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>> UpdateServiceRequestToLocationManager(
            UpdateRequestToLocationManagerToBookServiceDTO request, int currentUserId)
        {
            try
            {
                var productionCompanyId = await GetProductionCompanyIdAsync(currentUserId);
                if (productionCompanyId == null)
                    return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Production company profile not found",
                        "لم يتم العثور على ملف شركة الإنتاج"
                    );

                var entity = await _context.RequestToLocationManagerToBookService
                    .FirstOrDefaultAsync(lb => lb.Id == request.Id && !lb.IsDeleted);

                if (entity == null)
                    return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Request not found",
                        "الطلب غير موجود"
                    );

                if (entity.ProductionCompanyId != productionCompanyId.Value)
                    return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "You are not authorized to update a request for this production company",
                        "غير مصرح لك بتحديث طلب لشركة الإنتاج هذه"
                    );

                if (request.StartDate <= DateTime.Now)
                    return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "Start date must be in the future",
                        "يجب أن يكون تاريخ البداية في المستقبل"
                    );

                if (request.StartDate >= request.EndDate)
                    return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                        "End date must be after start date",
                        "يجب أن يكون تاريخ الانتهاء بعد تاريخ البداية"
                    );

  

                entity.ServiceTypeId = request.ServiceTypeId;
                entity.StartDate = request.StartDate;
                entity.EndDate = request.EndDate;
                entity.Notes = request.Notes;
                entity.UpdatedAt = DateTime.UtcNow;


                if (request.Latitude != 0 && request.Longitude != 0)
                {
                    entity.Latitude = request.Latitude;
                    entity.Longitude = request.Longitude;
                    if (request.LocationOnGoogleMaps.IsNullOrEmpty())
                    {
                        entity.LocationOnGoogleMaps = $"https://www.google.com/maps/search/?api=1&query={request.Latitude},{request.Longitude}";
                    }
                    else
                    {
                        entity.LocationOnGoogleMaps = request.LocationOnGoogleMaps;
                    }
                }
                else if (!request.LocationOnGoogleMaps.IsNullOrEmpty())
                {
                    entity.LocationOnGoogleMaps = request.LocationOnGoogleMaps;
                }
                else
                {
                    return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                   "location or latitude and longitude is required",
                   "الموقع أو خط العرض وخط الطول مطلوبان"
                );
                }

                _context.RequestToLocationManagerToBookService.Update(entity);
                await _context.SaveChangesAsync();

                return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    request,
                    "Request updated successfully",
                    "تم تحديث الطلب بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating request for user {UserId}", currentUserId);
                return ApiResponse<UpdateRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while updating the request",
                    "حدث خطأ أثناء تحديث الطلب"
                );
            }
        }

        // ── Delete & Restore ──────────────────────────────────────────────────────

        public async Task<ApiResponse<bool>> DeleteServiceRequestToLocationManager(
            int currentUserId, int id)
        {
            try
            {
                var productionCompanyId = await GetProductionCompanyIdAsync(currentUserId);
                if (productionCompanyId == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Production company profile not found",
                        "لم يتم العثور على ملف شركة الإنتاج"
                    );

                var entity = await _context.RequestToLocationManagerToBookService
                    .FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted);

                if (entity == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Request not found",
                        "الطلب غير موجود"
                    );

                if (entity.ProductionCompanyId != productionCompanyId.Value)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to delete this request",
                        "غير مصرح لك بحذف هذا الطلب"
                    );

                entity.IsDeleted = true;
                entity.IsActive = false;
                entity.UpdatedAt = DateTime.UtcNow;

                _context.RequestToLocationManagerToBookService.Update(entity);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Request deleted successfully",
                    "تم حذف الطلب بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting request {Id} for user {UserId}", id, currentUserId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while deleting the request",
                    "حدث خطأ أثناء حذف الطلب"
                );
            }
        }

        public async Task<ApiResponse<bool>> RestoreDeletedServiceRequestToLocationManager(
            int currentUserId, int id)
        {
            try
            {
                var productionCompanyId = await GetProductionCompanyIdAsync(currentUserId);
                if (productionCompanyId == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Production company profile not found",
                        "لم يتم العثور على ملف شركة الإنتاج"
                    );

                var entity = await _context.RequestToLocationManagerToBookService
                    .FirstOrDefaultAsync(r => r.Id == id && r.IsDeleted);

                if (entity == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Deleted request not found",
                        "الطلب المحذوف غير موجود"
                    );

                if (entity.ProductionCompanyId != productionCompanyId.Value)
                    return ApiResponse<bool>.FailureResponse(
                        "You are not authorized to restore this request",
                        "غير مصرح لك باستعادة هذا الطلب"
                    );

                entity.IsDeleted = false;
                entity.IsActive = true;
                entity.UpdatedAt = DateTime.UtcNow;

                _context.RequestToLocationManagerToBookService.Update(entity);
                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Request restored successfully",
                    "تم استعادة الطلب بنجاح"
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error restoring request {Id} for user {UserId}", id, currentUserId);
                return ApiResponse<bool>.FailureResponse(
                    "An error occurred while restoring the request",
                    "حدث خطأ أثناء استعادة الطلب"
                );
            }
        }

        public async Task<ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>> ReadServiceRequestToLocationManagerById(int currentUserId, bool IsDeleted, int Id)
        {
            try
            {
                int? locationManagerId = -1;
                var productionCompanyId = await GetProductionCompanyIdAsync(currentUserId);
                if (productionCompanyId == null)
                {
                    locationManagerId = await GetLocationManagerIdAsync(currentUserId);

                    if (locationManagerId == null)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "User not Found",
                            "لم يتم العثور على المستخدم"
                        );
                    }
                }

                if (productionCompanyId != null)
                {
                    var requests = await RequestsWithIncludes()
                    .Where(r => r.ProductionCompanyId == productionCompanyId.Value && r.IsDeleted == IsDeleted && r.Id == Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => MapToReadDTO(r))
                    .FirstOrDefaultAsync();

                    if (requests == null)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Request not found",
                            "الطلب غير موجود"
                        );
                    }

                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    requests,
                    "Requests retrieved successfully",
                    "تم استرجاع الطلبات بنجاح"
                );
                }
                else
                {
                    var requests = await RequestsWithIncludes()
                    .Where(r => r.LocationBooking.LocationManagerId == locationManagerId.Value && r.IsDeleted == IsDeleted && r.Id == Id)
                    .OrderByDescending(r => r.CreatedAt)
                    .Select(r => MapToReadDTO(r))
                    .FirstOrDefaultAsync();

                    if (requests == null)
                    {
                        return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                            "Request not found",
                            "الطلب غير موجود"
                        );
                    }   
                    return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.SuccessResponse(
                    requests,
                    "Requests retrieved successfully",
                    "تم استرجاع الطلبات بنجاح"
                );
                }


            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reading requests for user {UserId}", currentUserId);
                return ApiResponse<ReadRequestToLocationManagerToBookServiceDTO>.FailureResponse(
                    "An error occurred while retrieving the requests",
                    "حدث خطأ أثناء استرجاع الطلبات"
                );
            }
        }
    }
}