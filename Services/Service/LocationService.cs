using FilmMaker.Common;
using FilmMaker.DTO.Location;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationService : ILocationService
    {
        private readonly FilmMakerDbContext _filmMakerDbContext;
        private readonly ILogger<LocationService> _logger;

        public LocationService(FilmMakerDbContext filmMakerDbContext, ILogger<LocationService> logger)
        {
            _filmMakerDbContext = filmMakerDbContext;
            _logger = logger;
        }

        public async Task<ApiResponse<LocationDTO>> CreateLocation(LocationDTO location, int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            if (locationOwnerId == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to create a location but is not a location owner.", currentUserId);
                return new Common.ApiResponse<LocationDTO>
                {
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };
            }
            var validationResult = ValidateLocation(location);
            if (validationResult != null)
            {
                _logger.LogWarning("Validation failed for location creation by user with ID {UserId}.", currentUserId);
                return validationResult; 
            }

            if(location.TermsOfUse.Count == 0)
            {
                _logger.LogWarning("Validation failed for location creation due to empty terms of use by user with ID {UserId}.", currentUserId);
                return new Common.ApiResponse<LocationDTO>
                {
                    MessageAr = "شروط الاستخدام لا يمكن أن تحتوي على نص فارغ.",
                    MessageEn = "Terms of use cannot contain empty text.",
                    Success = false,
                };
            }

            try
            {
                var newLocation = new Location
                {
                    LocationName = location.LocationName,
                    LocationDescription = location.LocationDescription,
                    DailyPrice = location.DailyPrice,
                    LocationStatusId = location.LocationStatusId,
                    LocationOnGoogleMaps = location.LocationOnGoogleMaps,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    LocationOwnerId = locationOwnerId ?? 0,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = currentUserId.ToString(),
                    UpdatedAt = DateTime.UtcNow,
                    TermsOfUse = location.TermsOfUse?
                    .Select(term => new LocationTermsOfUse
                    {
                        TermText = term,
                        CreatedBy = currentUserId.ToString(),
                        CreatedAt = DateTime.UtcNow
                    }).ToList() ?? new List<LocationTermsOfUse>()
                };
                await _filmMakerDbContext.Locations.AddAsync(newLocation);

                await _filmMakerDbContext.SaveChangesAsync();

                return new Common.ApiResponse<LocationDTO>
                {
                    MessageAr = "تم انشاء الموقع",
                    MessageEn = "Location Has been created",
                    Success = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while creating a location for user with ID {UserId}.", currentUserId);
                return new Common.ApiResponse<LocationDTO>
                {
                    MessageAr = "حدث خطأ أثناء انشاء الموقع.",
                    MessageEn = "An error occurred while creating the location.",
                    Success = false,
                };
            }
        }

        public async Task<ApiResponse<UpdateLocationDTO>> UpdateLocation(UpdateLocationDTO location, int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            var isOwnerOfLocation = await _filmMakerDbContext.Locations
                .AnyAsync(l => l.Id == location.Id && l.LocationOwnerId == locationOwnerId);

            if (locationOwnerId == null || !isOwnerOfLocation)
            {
                _logger.LogWarning("User with ID {UserId} attempted to update a location but is not a location owner.", currentUserId);
                return new ApiResponse<UpdateLocationDTO>
                {
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };
            }

            var validationResult = ValidateLocation(location);
            if (validationResult != null)
            {
                 _logger.LogWarning("Validation failed for location update by user with ID {UserId}.", currentUserId);
                return validationResult;
            }


            try
            {
                var existingLocation = await _filmMakerDbContext.Locations
                    .Include(l => l.TermsOfUse)
                    .FirstOrDefaultAsync(l => l.Id == location.Id);

                if (existingLocation == null)
                    return new ApiResponse<UpdateLocationDTO>
                    {
                        MessageAr = "الموقع غير موجود.",
                        MessageEn = "Location not found.",
                        Success = false,
                    };

                existingLocation.LocationName = location.LocationName;
                existingLocation.LocationDescription = location.LocationDescription;
                existingLocation.DailyPrice = location.DailyPrice;
                existingLocation.LocationStatusId = location.LocationStatusId;
                existingLocation.LocationOnGoogleMaps = location.LocationOnGoogleMaps;
                existingLocation.Latitude = location.Latitude;
                existingLocation.Longitude = location.Longitude;
                existingLocation.UpdatedBy = currentUserId.ToString();
                existingLocation.UpdatedAt = DateTime.UtcNow;

                foreach (var termDto in location.TermsOfUse ?? [])
                {
                    var existingTerm = existingLocation.TermsOfUse
                        .FirstOrDefault(t => t.Id == termDto.Id);

                    if (existingTerm != null)
                    {
                       
                        existingTerm.TermText = termDto.Term;
                    }
                    else
                    {
                        
                        existingLocation.TermsOfUse.Add(new LocationTermsOfUse
                        {
                            TermText = termDto.Term,
                            LocationId = existingLocation.Id
                        });
                    }
                }

                var dtoTermIds = (location.TermsOfUse ?? [])
                  .Select(t => t.Id)
                  .ToList();

                var termsToRemove = existingLocation.TermsOfUse
                    .Where(t => !dtoTermIds.Contains(t.Id))
                    .ToList();

                _filmMakerDbContext.LocationTermsOfUse.RemoveRange(termsToRemove);




                await _filmMakerDbContext.SaveChangesAsync();

                return new ApiResponse<UpdateLocationDTO>
                {
                    Data = location,
                    MessageAr = "تم تحديث الموقع.",
                    MessageEn = "Location updated.",
                    Success = true,
                };
            }
            catch
            {
                _logger.LogError("An error occurred while updating the location with ID {LocationId} for user with ID {UserId}.", location.Id, currentUserId);
                return new ApiResponse<UpdateLocationDTO>
                {
                    MessageAr = "حدث خطأ أثناء تحديث الموقع.",
                    MessageEn = "An error occurred while updating the location.",
                    Success = false,
                };
            }
        }



        public async Task<ApiResponse<GetLocationDTO>> GetLocationById(int locationId)
        {
            try
            {
                var location = await _filmMakerDbContext.Locations
                 .Where(l => l.Id == locationId && !l.IsDeleted)
                 .Select(l => new GetLocationDTO
                 {
                     LocationName = l.LocationName,
                     LocationDescription = l.LocationDescription,
                     DailyPrice = l.DailyPrice,
                     LocationOwnerId = l.LocationOwnerId,

                     LocationOwnerName = l.LocationOwner != null
                         ? l.LocationOwner.User.Name
                         : string.Empty,

                     LocationManagerName = l.LocationManager != null
                         ? l.LocationManager.User.Name
                         : string.Empty,

                     IsActive = l.IsActive,

                     LocationStatusName = l.LocationStatus != null
                         ? l.LocationStatus.Name
                         : string.Empty,

                     LocationOnGoogleMaps = l.LocationOnGoogleMaps,
                     Latitude = l.Latitude,
                     Longitude = l.Longitude
                 })
                 .FirstOrDefaultAsync();

                if (location == null)
                {
                    _logger.LogWarning("Location with ID {LocationId} not found.", locationId);
                    return new ApiResponse<GetLocationDTO>
                    {
                        MessageAr = "الموقع غير موجود.",
                        MessageEn = "Location not found.",
                        Success = false,
                    };
                }

                return new ApiResponse<GetLocationDTO>
                {
                    Data = location,
                    MessageAr = "تم جلب الموقع.",
                    MessageEn = "Location fetched.",
                    Success = true,
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while fetching the location with ID {LocationId}.", locationId);
                return new ApiResponse<GetLocationDTO>
                {
                    MessageAr = "حدث خطأ أثناء جلب الموقع.",
                    MessageEn = "An error occurred while fetching the location.",
                    Success = false,
                };

            }
        }

        public async Task<ApiResponse<List<GetLocationDTO>>> GetAllArchivedLocations()
        {
            var statusId = await GetStatusIdByName("Archived");
            var locations = await _filmMakerDbContext.Locations
         .Where(l =>  !l.IsDeleted && l.LocationStatusId == statusId )
         .Select(l => new GetLocationDTO
         {
             LocationName = l.LocationName,
             LocationDescription = l.LocationDescription,
             DailyPrice = l.DailyPrice,
             LocationOwnerId = l.LocationOwnerId,

             LocationOwnerName = l.LocationOwner != null
                 ? l.LocationOwner.User.Name
                 : string.Empty,

             LocationManagerName = l.LocationManager != null
                 ? l.LocationManager.User.Name
                 : string.Empty,

             IsActive = l.IsActive,

             LocationStatusName = l.LocationStatus != null
                 ? l.LocationStatus.Name
                 : string.Empty,

             LocationOnGoogleMaps = l.LocationOnGoogleMaps,
             Latitude = l.Latitude,
             Longitude = l.Longitude
         })
         .ToListAsync();

            return new ApiResponse<List<GetLocationDTO>>
            {
                Data = locations,
                MessageAr = "تم جلب جميع المواقع.",
                MessageEn = "All locations fetched.",
                Success = true,
            };
        }

        public async Task<ApiResponse<List<GetLocationDTO>>> GetLocationsByOwnerId(int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            if (locationOwnerId == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to fetch locations but is not a location owner.", currentUserId);

                return new ApiResponse<List<GetLocationDTO>>
                {
                    Data = new List<GetLocationDTO>(),
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };
            }



                var locations = await _filmMakerDbContext.Locations
                .Where(l => l.LocationOwnerId == locationOwnerId && !l.IsDeleted)
                .Select(l => new GetLocationDTO
                {
                 LocationName = l.LocationName,
                 LocationDescription = l.LocationDescription,
                 DailyPrice = l.DailyPrice,
                 LocationOwnerId = l.LocationOwnerId,
                
                 LocationOwnerName = l.LocationOwner != null
                     ? l.LocationOwner.User.Name
                     : string.Empty,
                
                 LocationManagerName = l.LocationManager != null
                     ? l.LocationManager.User.Name
                     : string.Empty,
                
                 IsActive = l.IsActive,
                
                 LocationStatusName = l.LocationStatus != null
                     ? l.LocationStatus.Name
                     : string.Empty,
                
                 LocationOnGoogleMaps = l.LocationOnGoogleMaps,
                 Latitude = l.Latitude,
                 Longitude = l.Longitude
                })
                .ToListAsync();

            return new ApiResponse<List<GetLocationDTO>>
            {
                Data = locations,
                MessageAr = "تم جلب المواقع.",
                MessageEn = "Locations fetched.",
                Success = true,
            };
        }

        public async Task<ApiResponse<List<GetLocationDTO>>> GetOwnerActiveLocations(int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            if (locationOwnerId == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to fetch locations but is not a location owner.", currentUserId);

                return new ApiResponse<List<GetLocationDTO>>
                {
                    Data = new List<GetLocationDTO>(),
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };
            }


                  var locations = await _filmMakerDbContext.Locations
                 .Where(l => l.LocationOwnerId == locationOwnerId
                         && !l.IsDeleted
                         && l.IsActive)
                 .Select(l => new GetLocationDTO
                 {
                     LocationName = l.LocationName,
                     LocationDescription = l.LocationDescription,
                     DailyPrice = l.DailyPrice,
                     LocationOwnerId = l.LocationOwnerId,
                
                     LocationOwnerName = l.LocationOwner != null
                         ? l.LocationOwner.User.Name
                         : string.Empty,
                
                     LocationManagerName = l.LocationManager != null
                         ? l.LocationManager.User.Name
                         : string.Empty,           
                     IsActive = l.IsActive,            
                     LocationStatusName = l.LocationStatus != null
                         ? l.LocationStatus.Name
                         : string.Empty,
                     LocationOnGoogleMaps = l.LocationOnGoogleMaps,
                     Latitude = l.Latitude,
                     Longitude = l.Longitude
                 })
                 .ToListAsync();

            return new ApiResponse<List<GetLocationDTO>>
            {
                Data = locations,
                MessageAr = "تم جلب المواقع النشطة.",
                MessageEn = "Active locations fetched.",
                Success = true,
            };
        }

        public async Task<ApiResponse<List<GetLocationDTO>>> GetOwnerArchivedLocations(int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);


            if (locationOwnerId == null)
            {
                _logger.LogWarning("User with ID {UserId} attempted to fetch locations but is not a location owner.", currentUserId);

                return new ApiResponse<List<GetLocationDTO>>
                {
                    Data = new List<GetLocationDTO>(),
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };
            }
      
            int statusId = await GetStatusIdByName("Archived");
            var locations = await _filmMakerDbContext.Locations
           .Where(l => l.LocationOwnerId == locationOwnerId
                   && l.IsDeleted
                   && !l.IsActive && l.LocationStatusId == statusId)
           .Select(l => new GetLocationDTO
           {
               LocationName = l.LocationName,
               LocationDescription = l.LocationDescription,
               DailyPrice = l.DailyPrice,
               LocationOwnerId = l.LocationOwnerId,

               LocationOwnerName = l.LocationOwner != null
                   ? l.LocationOwner.User.Name
                   : string.Empty,

               LocationManagerName = l.LocationManager != null
                   ? l.LocationManager.User.Name
                   : string.Empty,
               IsActive = l.IsActive,
               LocationStatusName = l.LocationStatus != null
                   ? l.LocationStatus.Name
                   : string.Empty,
               LocationOnGoogleMaps = l.LocationOnGoogleMaps,
               Latitude = l.Latitude,
               Longitude = l.Longitude
           })
           .ToListAsync();


            return new ApiResponse<List<GetLocationDTO>>
            {
                Data = locations,
                MessageAr = "تم جلب المواقع المؤرشفة.",
                MessageEn = "Archived locations fetched.",
                Success = true,
            };
        }

        public async Task<ApiResponse<List<GetLocationDTO>>> GetAllUnArchivedLocations()
        {
   
            int statusId = await GetStatusIdByName("Archived");

            var locations = await _filmMakerDbContext.Locations
           .Where(l => !l.IsDeleted && l.LocationStatusId != statusId)
           .Select(l => new GetLocationDTO
           {
               LocationName = l.LocationName,
               LocationDescription = l.LocationDescription,
               DailyPrice = l.DailyPrice,
               LocationOwnerId = l.LocationOwnerId,

               LocationOwnerName = l.LocationOwner != null
                   ? l.LocationOwner.User.Name
                   : string.Empty,

               LocationManagerName = l.LocationManager != null
                   ? l.LocationManager.User.Name
                   : string.Empty,
               IsActive = l.IsActive,
               LocationStatusName = l.LocationStatus != null
                   ? l.LocationStatus.Name
                   : string.Empty,
               LocationOnGoogleMaps = l.LocationOnGoogleMaps,
               Latitude = l.Latitude,
               Longitude = l.Longitude
           })
           .ToListAsync();

            return new ApiResponse<List<GetLocationDTO>>
            {
                Data = locations,
                MessageAr = "تم جلب جميع المواقع.",
                MessageEn = "All locations fetched.",
                Success = true,
            };
        }
        private static async Task<int?> GetLocationOwnerIdByUserId(int userId, FilmMakerDbContext context)
        {
            var locationOwner = await context.LocationOwnerProfiles
                                .FirstOrDefaultAsync(x => x.UserId == userId);
            return locationOwner.Id;
        }

        public async Task<ApiResponse<bool>> ArchiveLocation(int locationId, int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            var isOwnerOfLocation = await _filmMakerDbContext.Locations
                .AnyAsync(l => l.Id == locationId && l.LocationOwnerId == locationOwnerId);

            if (locationOwnerId == null || !isOwnerOfLocation)
            {
                _logger.LogWarning(
                    "User with ID {UserId} attempted to archive a location but is not a location owner.",
                    currentUserId);

                return new ApiResponse<bool>
                {
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };
            }

            try
            {
                var existingLocation = await _filmMakerDbContext.Locations
                    .FirstOrDefaultAsync(l => l.Id == locationId);

                if (existingLocation == null)
                {
                    return new ApiResponse<bool>
                    {
                        MessageAr = "الموقع غير موجود.",
                        MessageEn = "Location not found.",
                        Success = false,
                    };
                }

                existingLocation.LocationStatusId = await GetStatusIdByName("Archived");
                existingLocation.IsDeleted = true;
                existingLocation.IsActive = false;

                _filmMakerDbContext.LocationArchiveHistories.Add(new LocationArchiveHistory
                {
                    LocationId = locationId,
                    ArchivedByUserId = locationOwnerId.Value,
                    ArchivedAt = DateTime.UtcNow,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });

                await _filmMakerDbContext.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Data = true,
                    MessageAr = "تم أرشفة الموقع.",
                    MessageEn = "Location archived.",
                    Success = true,
                };
            }
            catch
            {
                _logger.LogError(
                    "An error occurred while archiving location with ID {LocationId} for user with ID {UserId}.",
                    locationId,
                    currentUserId);

                return new ApiResponse<bool>
                {
                    MessageAr = "حدث خطأ أثناء أرشفة الموقع.",
                    MessageEn = "An error occurred while archiving the location.",
                    Success = false,
                };
            }
        }
        public async Task<ApiResponse<bool>> RestoreArchivedLocation(int locationId, int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            var isOwnerOfLocation = await _filmMakerDbContext.Locations
                .AnyAsync(l => l.Id == locationId && l.LocationOwnerId == locationOwnerId);

            if (locationOwnerId == null || !isOwnerOfLocation)
            {
                _logger.LogWarning(
                    "User with ID {UserId} attempted to restore a location but is not a location owner.",
                    currentUserId);

                return new ApiResponse<bool>
                {
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };
            }

            try
            {
                var existingLocation = await _filmMakerDbContext.Locations
                    .FirstOrDefaultAsync(l => l.Id == locationId);

                if (existingLocation == null)
                {
                    return new ApiResponse<bool>
                    {
                        MessageAr = "الموقع غير موجود.",
                        MessageEn = "Location not found.",
                        Success = false,
                    };
                }

                existingLocation.LocationStatusId = await GetStatusIdByName("Active");
                existingLocation.IsDeleted = false;
                existingLocation.IsActive = true;

                var archiveEntry = await _filmMakerDbContext.LocationArchiveHistories
                    .Where(h => h.LocationId == locationId && h.IsRestored != true)
                    .OrderByDescending(h => h.ArchivedAt)
                    .FirstOrDefaultAsync();

                if (archiveEntry != null)
                {
                    archiveEntry.IsRestored = true;
                    archiveEntry.RestoredAt = DateTime.UtcNow;
                    archiveEntry.RestoredByUserId = locationOwnerId.Value;
                }

                await _filmMakerDbContext.SaveChangesAsync();

                return new ApiResponse<bool>
                {
                    Data = true,
                    MessageAr = "تم تفعيل الموقع.",
                    MessageEn = "Location restored.",
                    Success = true,
                };
            }
            catch
            {
                _logger.LogError(
                    "An error occurred while restoring archived location with ID {LocationId} for user with ID {UserId}.",
                    locationId,
                    currentUserId);

                return new ApiResponse<bool>
                {
                    MessageAr = "حدث خطأ أثناء استعادة الموقع.",
                    MessageEn = "An error occurred while restoring the location.",
                    Success = false,
                };
            }
        }

        // ── Private helper ────────────────────────────────────────────────────

        private Common.ApiResponse<UpdateLocationDTO>? ValidateLocation(UpdateLocationDTO location)
        {
            if (string.IsNullOrEmpty(location.LocationName))
            {
                return new Common.ApiResponse<UpdateLocationDTO>
                {
                    MessageAr = "اسم الموقع مطلوب.",
                    MessageEn = "Location name is required.",
                    Success = false,
                };
            }

            if (string.IsNullOrEmpty(location.LocationDescription))
            {
                return new Common.ApiResponse<UpdateLocationDTO>
                {
                    MessageAr = "وصف الموقع مطلوب.",
                    MessageEn = "Location description is required.",
                    Success = false,
                };
            }

            if (location.DailyPrice <= 0)
            {
                return new Common.ApiResponse<UpdateLocationDTO>
                {
                    MessageAr = "يجب أن يكون السعر اليومي أكبر من صفر.",
                    MessageEn = "Daily price must be greater than zero.",
                    Success = false,
                };
            }

            return null;
        }
        private Common.ApiResponse<LocationDTO>? ValidateLocation(LocationDTO location )
        {
            if (string.IsNullOrEmpty(location.LocationName))
            {
                return new Common.ApiResponse<LocationDTO>
                {
                    MessageAr = "اسم الموقع مطلوب.",
                    MessageEn = "Location name is required.",
                    Success = false,
                };
            }

            if (string.IsNullOrEmpty(location.LocationDescription))
            {
                return new Common.ApiResponse<LocationDTO>
                {
                    MessageAr = "وصف الموقع مطلوب.",
                    MessageEn = "Location description is required.",
                    Success = false,
                };
            }

            if (location.DailyPrice <= 0)
            {
                return new Common.ApiResponse<LocationDTO>
                {
                    MessageAr = "يجب أن يكون السعر اليومي أكبر من صفر.",
                    MessageEn = "Daily price must be greater than zero.",
                    Success = false,
                };
            }

            return null; 
        }
        
        private async Task<int> GetStatusIdByName(string name)
        {
          var status = await _filmMakerDbContext.LockupItems
                .Where(s => s.Name == name)
                .Select(s => s.Id)
                .FirstOrDefaultAsync();
            return status;
        }
    }
}