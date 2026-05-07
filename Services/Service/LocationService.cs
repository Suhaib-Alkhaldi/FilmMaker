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

        public LocationService(FilmMakerDbContext filmMakerDbContext)
        {
            _filmMakerDbContext = filmMakerDbContext;
        }

        public async Task<ApiResponse<LocationDTO>> CreateLocation(LocationDTO location, int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            if (locationOwnerId == null)
                return new Common.ApiResponse<LocationDTO>
                {
                   MessageAr = "صاحب الموقع غير موجود.",
                   MessageEn = "Location owner not found.",
                    Success = false,
                };

            var validationResult = ValidateLocation(location);
            if (validationResult != null)
            {
                return validationResult; 
            }


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
                    IsArchived = false,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = currentUserId.ToString(),
                    UpdatedAt = DateTime.UtcNow
                };
            await _filmMakerDbContext.locations.AddAsync(newLocation);


            if (location.TermsOfUse != null && location.TermsOfUse.Any())
            {
                var termsList = location.TermsOfUse.Select(term => new LocationTermsOfUse
                {
                    Term = term, // Assuming 'TermContent' is your column name
                    Location = newLocation, // EF handles the Foreign Key mapping automatically
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                }).ToList();

                await _filmMakerDbContext.locationTermsOfUse.AddRangeAsync(termsList);
            }

            await _filmMakerDbContext.SaveChangesAsync();

            return new Common.ApiResponse<LocationDTO>
            {
                MessageAr = "تم انشاء الموقع",
                MessageEn = "Location Has been created",
                Success = true,
            };

        }

        public async Task<ApiResponse<LocationDTO>> UpdateLocation(LocationDTO location, int currentUserId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            if (locationOwnerId == null)
                return new ApiResponse<LocationDTO>
                {
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };

            var validationResult = ValidateLocation(location);
            if (validationResult != null)
            {
                return validationResult;
            }


            try
            {
                var existingLocation = await _filmMakerDbContext.locations
                    .FirstOrDefaultAsync(l => l.Id == location.Id);

                if (existingLocation == null)
                    return new ApiResponse<LocationDTO>
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



                await _filmMakerDbContext.SaveChangesAsync();

                return new ApiResponse<LocationDTO>
                {
                    Data = location,
                    MessageAr = "تم تحديث الموقع.",
                    MessageEn = "Location updated.",
                    Success = true,
                };
            }
            catch
            {
                return new ApiResponse<LocationDTO>
                {
                    MessageAr = "حدث خطأ أثناء تحديث الموقع.",
                    MessageEn = "An error occurred while updating the location.",
                    Success = false,
                };
            }
        }

        public async Task<ApiResponse<LocationDTO>> ToggleArchive(int currentUserId, int locationId)
        {
            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId, _filmMakerDbContext);

            if (locationOwnerId == null)
                return new ApiResponse<LocationDTO>
                {
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };

            try
            {
                var location = await _filmMakerDbContext.locations
                    .FirstOrDefaultAsync(l => l.Id == locationId
                                           && l.LocationOwnerId == locationOwnerId
                                           && !l.IsDeleted);

                if (location == null)
                    return new ApiResponse<LocationDTO>
                    {
                        MessageAr = "الموقع غير موجود.",
                        MessageEn = "Location not found.",
                        Success = false,
                    };

                location.IsArchived = !location.IsArchived;
                location.UpdatedBy = currentUserId.ToString();
                location.UpdatedAt = DateTime.UtcNow;

                await _filmMakerDbContext.SaveChangesAsync();

                return new ApiResponse<LocationDTO>
                {
                    Data = new LocationDTO
                    {
                        LocationName = location.LocationName,
                        LocationDescription = location.LocationDescription,
                        DailyPrice = location.DailyPrice,
                        LocationStatusId = location.LocationStatusId,
                        LocationOnGoogleMaps = location.LocationOnGoogleMaps,
                        Latitude = location.Latitude,
                        Longitude = location.Longitude
                    },
                    MessageAr = "تم تغيير حالة الأرشفة.",
                    MessageEn = "Archive status toggled.",
                    Success = true,
                };
            }
            catch
            {
                return new ApiResponse<LocationDTO>
                {
                    MessageAr = "حدث خطأ أثناء تغيير حالة الأرشفة.",
                    MessageEn = "An error occurred while toggling archive status.",
                    Success = false,
                };
            }
        }

        public async Task<ApiResponse<GetLocationDTO>> GetLocationById(int locationId)
        {
            var location = await _filmMakerDbContext.locations
                .Include(l => l.LocationOwner).ThenInclude(o => o.User)
                .Include(l => l.LocationManager).ThenInclude(m => m.User)
                .Include(l => l.LocationStatus)
                .FirstOrDefaultAsync(l => l.Id == locationId && !l.IsDeleted);

            if (location == null)
            {
                return new ApiResponse<GetLocationDTO>
                {
                    MessageAr = "الموقع غير موجود.",
                    MessageEn = "Location not found.",
                    Success = false,
                };
            }

            return new ApiResponse<GetLocationDTO>
            {
                Data = MapToGetLocationDTO(location),
                MessageAr = "تم جلب الموقع.",
                MessageEn = "Location fetched.",
                Success = true,
            };
        }

        public async Task<ApiResponse<List<GetLocationDTO>>> GetAllLocations()
        {
            var locations = await _filmMakerDbContext.locations
                .Include(l => l.LocationOwner).ThenInclude(o => o.User)
                .Include(l => l.LocationManager).ThenInclude(m => m.User)
                .Include(l => l.LocationStatus)
                .Where(l => !l.IsDeleted)
                .Select(l => MapToGetLocationDTO(l))
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
                return new ApiResponse<List<GetLocationDTO>>
                {
                    Data = new List<GetLocationDTO>(),
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };

            var locations = await _filmMakerDbContext.locations
                .Include(l => l.LocationOwner).ThenInclude(o => o.User)
                .Include(l => l.LocationManager).ThenInclude(m => m.User)
                .Include(l => l.LocationStatus)
                .Where(l => l.LocationOwnerId == locationOwnerId && !l.IsDeleted)
                .Select(l => MapToGetLocationDTO(l))
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
                return new ApiResponse<List<GetLocationDTO>>
                {
                    Data = new List<GetLocationDTO>(),
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };

            var locations = await _filmMakerDbContext.locations
                .Include(l => l.LocationOwner).ThenInclude(o => o.User)
                .Include(l => l.LocationManager).ThenInclude(m => m.User)
                .Include(l => l.LocationStatus)
                .Where(l => l.LocationOwnerId == locationOwnerId
                         && !l.IsDeleted
                         && !l.IsArchived
                         && l.IsActive)
                .Select(l => MapToGetLocationDTO(l))
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
                return new ApiResponse<List<GetLocationDTO>>
                {
                    Data = new List<GetLocationDTO>(),
                    MessageAr = "صاحب الموقع غير موجود.",
                    MessageEn = "Location owner not found.",
                    Success = false,
                };

            var locations = await _filmMakerDbContext.locations
                .Include(l => l.LocationOwner).ThenInclude(o => o.User)
                .Include(l => l.LocationManager).ThenInclude(m => m.User)
                .Include(l => l.LocationStatus)
                .Where(l => l.LocationOwnerId == locationOwnerId
                 && !l.IsDeleted
                 && l.IsArchived)
                .Select(l => MapToGetLocationDTO(l))
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
            var locations = await _filmMakerDbContext.locations
             .Include(l => l.LocationOwner).ThenInclude(o => o.User)
             .Include(l => l.LocationManager).ThenInclude(m => m.User)
             .Include(l => l.LocationStatus)
             .Where(l => !l.IsDeleted && !l.IsArchived)
             .Select(l => MapToGetLocationDTO(l))
             .ToListAsync();

            return new ApiResponse<List<GetLocationDTO>>
            {
                Data = locations,
                MessageAr = "تم جلب جميع المواقع.",
                MessageEn = "All locations fetched.",
                Success = true,
            };
        }


        // ── Private helper ────────────────────────────────────────────────────

        private Common.ApiResponse<LocationDTO>? ValidateLocation(LocationDTO location)
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

        private static async Task<int?> GetLocationOwnerIdByUserId(int userId, FilmMakerDbContext context)
        {
            var locationOwner = await context.locationOwnerProfiles
                                .FirstOrDefaultAsync(x => x.UserId == userId);
            return locationOwner.Id;
        }

        private static GetLocationDTO MapToGetLocationDTO(Location l) => new()
        {
            LocationName = l.LocationName,
            LocationDescription = l.LocationDescription,
            DailyPrice = l.DailyPrice,
            LocationOwnerId = l.LocationOwnerId,
            LocationOwnerName = l.LocationOwner != null
                                    ? $"{l.LocationOwner.User.Name} "
                                    : string.Empty,
            LocationManagerName = l.LocationManager != null
                                    ? $"{l.LocationManager.User.Name}  "
                                    : string.Empty,
            IsActive = l.IsActive,
            IsArchived = l.IsArchived,
            LocationStatusName = l.LocationStatus?.Name ?? string.Empty,
            LocationOnGoogleMaps = l.LocationOnGoogleMaps,
            Latitude = l.Latitude,
            Longitude = l.Longitude
        };

    }
}