using FilmMaker.Common;
using FilmMaker.DTO.Location.Request;
using FilmMaker.DTO.Location.Response;
using FilmMaker.DTO.Media;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationService : ILocationService
    {
        private readonly FilmMakerDbContext _context;
        private readonly IMediaService _mediaService;
        private readonly ILogger<LocationService> _logger;

        public LocationService(FilmMakerDbContext context,IMediaService mediaService,ILogger<LocationService> logger)
        {
            _context = context;
            _mediaService = mediaService;
            _logger = logger;
        }



        public async Task<ApiResponse<LocationResponseDto>> CreateLocation(CreateLocationRequestDto request, int currentUserId)
        {
            if (request == null)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            var validationError = ValidateCreateLocationRequest(request);
            if (validationError != null)
                return validationError;

            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

            if (locationOwnerId == null)
            {
                _logger.LogWarning(
                    "Create location failed. User is not a location owner. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Location owner profile was not found.",
                    "لم يتم العثور على ملف صاحب الموقع."
                );
            }

            var activeStatusId = await GetStatus("LocationStatus", "Active");

            if (activeStatusId == null)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Active LocationStatus was not found in lookup data.",
                    "حالة الموقع النشط غير موجودة في بيانات النظام."
                );
            }

            var mediaValidationResult = await _mediaService.ValidateMediaOwnership(request.MediaIds,currentUserId);

            if (!mediaValidationResult.Success)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    mediaValidationResult.MessageEn,
                    mediaValidationResult.MessageAr
                );
            }

            var mediaItems = mediaValidationResult.Data ?? new List<Media>();

            var mediaBusinessValidation = await ValidateLocationMedia(
                mediaItems,
                request.MediaIds
            );

            if (mediaBusinessValidation != null)
                return mediaBusinessValidation;


            var isValidLocationType = await IsValidLocationType("LocationType",request.LocationTypeId);

            if (!isValidLocationType)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Invalid location type.",
                    "نوع الموقع غير صحيح."
                );
            }


            try
            {
                var location = new Location
                {
                    LocationName = request.LocationName.Trim(),
                    LocationDescription = request.LocationDescription.Trim(),
                    City = request.City.Trim(),
                    Address = string.IsNullOrWhiteSpace(request.Address)
                        ? null
                        : request.Address.Trim(),
                    DailyPrice = request.DailyPrice,
                    LocationOnGoogleMaps = string.IsNullOrWhiteSpace(request.LocationOnGoogleMaps)
                        ? null
                        : request.LocationOnGoogleMaps.Trim(),
                    Latitude = request.Latitude,
                    Longitude = request.Longitude,
                    Country = request.Country.Trim(),

                    HourlyPrice = request.HourlyPrice,

                    FacilitiesDescription = string.IsNullOrWhiteSpace(request.FacilitiesDescription)? null
                    : request.FacilitiesDescription.Trim(),
                    LocationOwnerId = locationOwnerId.Value,
                    LocationStatusId = activeStatusId.Value,
                    LocationTypeId = request.LocationTypeId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.Locations.Add(location);
                await _context.SaveChangesAsync();

                var terms = request.TermsOfUse
                    .Where(x => !string.IsNullOrWhiteSpace(x))
                    .Select(term => new LocationTermsOfUse
                    {
                        LocationId = location.Id,
                        TermText = term.Trim(),
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = currentUserId.ToString(),
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                _context.LocationTermsOfUse.AddRange(terms);

                var locationMediaLinks = mediaItems
                    .Select(media => new LocationMedia
                    {
                        LocationId = location.Id,
                        MediaId = media.Id,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = currentUserId.ToString(),
                        CreatedAt = DateTime.UtcNow
                    })
                    .ToList();

                _context.LocationMedia.AddRange(locationMediaLinks);

                await _context.SaveChangesAsync();

                var locationTypeName = await GetLookupItemNameById(request.LocationTypeId);

                var response = new LocationResponseDto
                {
                    LocationId = location.Id,
                    LocationName = location.LocationName,
                    LocationDescription = location.LocationDescription,
                    City = location.City,
                    Address = location.Address,
                    DailyPrice = location.DailyPrice,
                    LocationStatus = "Active",
                    LocationTypeId = request.LocationTypeId,
                    LocationType = locationTypeName,
                    Country = location.Country,
                    HourlyPrice = location.HourlyPrice,
                    FacilitiesDescription = location.FacilitiesDescription,
                    FacilitiesCount = GetFacilitiesCount(location.FacilitiesDescription),
                    LocationOnGoogleMaps = location.LocationOnGoogleMaps,
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    TermsOfUse = terms.Select(x => x.TermText).ToList(),
                    Media = mediaItems.Select(MapMediaResponse).ToList(),
                    CreatedAt = location.CreatedAt
                };

                _logger.LogInformation(
                    "Location created successfully. LocationId: {LocationId}, UserId: {UserId}",
                    location.Id,
                    currentUserId
                );

                return ApiResponse<LocationResponseDto>.SuccessResponse(
                    response,
                    "Location created successfully.",
                    "تم إنشاء الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Error while creating location. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "An unexpected error occurred while creating the location.",
                    "حدث خطأ غير متوقع أثناء إنشاء الموقع."
                );
            }
        }
        public async Task<ApiResponse<LocationResponseDto>> UpdateLocation(UpdateLocationRequestDto request, int currentUserId)
        {
            if (request == null)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            var validationError = ValidateUpdateLocationRequest(request);
            if (validationError != null)
                return validationError;

            var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

            if (locationOwnerId == null)
            {
                _logger.LogWarning(
                    "Update location failed. User is not a location owner. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Location owner profile was not found.",
                    "لم يتم العثور على ملف صاحب الموقع."
                );
            }

            var location = await _context.Locations
                .Include(x => x.TermsOfUse)
                .Include(x => x.Media)
                    .ThenInclude(x => x.Media)
                        .ThenInclude(x => x.MediaType)
                        .Where(x =>
                    x.Id == request.locationId &&
                    x.LocationOwnerId == locationOwnerId.Value &&
                    !x.IsDeleted).FirstOrDefaultAsync();

            if (location == null)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Location was not found.",
                    "لم يتم العثور على الموقع."
                );
            }

            LookupItem? locationType = null;

            if (request.LocationTypeId.HasValue)
            {
                locationType = await _context.LookupItems
                    .FirstOrDefaultAsync(x =>
                        x.Id == request.LocationTypeId.Value &&
                        x.LookupCategory.Name == "LocationType" &&
                        !x.IsDeleted);

                if (locationType == null)
                {
                    return ApiResponse<LocationResponseDto>.FailureResponse(
                        "Invalid location type.",
                        "نوع الموقع غير صحيح."
                    );
                }
            }

            List<Media>? mediaItems = null;

            if (request.MediaIds != null)
            {
                var mediaValidationResult = await _mediaService.ValidateMediaOwnership(
                    request.MediaIds,
                    currentUserId
                );

                if (!mediaValidationResult.Success)
                {
                    return ApiResponse<LocationResponseDto>.FailureResponse(
                        mediaValidationResult.MessageEn,
                        mediaValidationResult.MessageAr
                    );
                }

                mediaItems = mediaValidationResult.Data ?? new List<Media>();

                var mediaBusinessValidation = await ValidateLocationMediaForUpdate(
                    mediaItems,
                    request.MediaIds,
                    request.locationId
                );

                if (mediaBusinessValidation != null)
                    return mediaBusinessValidation;
            }


            try
            {
                if (!string.IsNullOrWhiteSpace(request.LocationName))
                {
                    location.LocationName = request.LocationName.Trim();
                }

                if (!string.IsNullOrWhiteSpace(request.LocationDescription))
                {
                    location.LocationDescription = request.LocationDescription.Trim();
                }

                if (request.LocationTypeId.HasValue)
                {
                    location.LocationTypeId = request.LocationTypeId.Value;
                }

                if (!string.IsNullOrWhiteSpace(request.City))
                {
                    location.City = request.City.Trim();
                }

                if (request.Address != null)
                {
                    location.Address = string.IsNullOrWhiteSpace(request.Address)
                        ? null
                        : request.Address.Trim();
                }
                if (!string.IsNullOrWhiteSpace(request.Country))
                {
                    location.Country = request.Country.Trim();
                }
                if (request.HourlyPrice.HasValue)
                {
                    location.HourlyPrice = request.HourlyPrice.Value;
                }
                if (request.FacilitiesDescription != null)
                {
                    location.FacilitiesDescription = string.IsNullOrWhiteSpace(request.FacilitiesDescription)
                        ? null
                        : request.FacilitiesDescription.Trim();
                }

                if (request.DailyPrice.HasValue)
                {
                    location.DailyPrice = request.DailyPrice.Value;
                }

                if (request.LocationOnGoogleMaps != null)
                {
                    location.LocationOnGoogleMaps = string.IsNullOrWhiteSpace(request.LocationOnGoogleMaps)
                        ? null
                        : request.LocationOnGoogleMaps.Trim();
                }

                if (request.Latitude.HasValue)
                {
                    location.Latitude = request.Latitude;
                }

                if (request.Longitude.HasValue)
                {
                    location.Longitude = request.Longitude;
                }

                location.UpdatedBy = currentUserId.ToString();
                location.UpdatedAt = DateTime.UtcNow;

                if (request.TermsOfUse != null)
                {
                    UpdateLocationTerms(location, request.TermsOfUse, currentUserId);
                }

                if (request.MediaIds != null && mediaItems != null)
                {
                    UpdateLocationMedia(location, mediaItems, currentUserId);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Location updated successfully. LocationId: {LocationId}, UserId: {UserId}",
                    request.locationId,
                    currentUserId
                );

                return await GetMyLocationById(request.locationId, currentUserId);
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Error while updating location. LocationId: {LocationId}, UserId: {UserId}",
                    request.locationId,
                    currentUserId
                );

                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "An unexpected error occurred while updating the location.",
                    "حدث خطأ غير متوقع أثناء تحديث الموقع."
                );
            }
        }
        public async Task<ApiResponse<bool>> ArchiveLocation(ArchiveLocationRequestDto request, int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    _logger.LogWarning(
                        "Archive location failed. User is not a location owner. UserId: {UserId}",
                        currentUserId
                    );

                    return ApiResponse<bool>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var archivedStatusId = await GetStatus("LocationStatus", "Archived");

                if (archivedStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Archived location status was not found in lookup data.",
                        "حالة الموقع المؤرشف غير موجودة في بيانات النظام."
                    );
                }

                var activeStatusId = await GetStatus("LocationStatus", "Active");

                if (activeStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Active location status was not found in lookup data.",
                        "حالة الموقع النشط غير موجودة في بيانات النظام."
                    );
                }

                var location = await _context.Locations.Where(x =>
                        x.Id == request.LocationId &&
                        x.LocationOwnerId == locationOwnerId.Value &&
                        !x.IsDeleted).FirstOrDefaultAsync();

                if (location == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Location was not found.",
                        "لم يتم العثور على الموقع."
                    );
                }

                if (location.LocationStatusId == archivedStatusId.Value)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Location is already archived.",
                        "الموقع مؤرشف مسبقًا."
                    );
                }

                if (location.LocationStatusId != activeStatusId.Value)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only active locations can be archived.",
                        "يمكن أرشفة المواقع النشطة فقط."
                    );
                }


                location.LocationStatusId = archivedStatusId.Value;
                location.UpdatedBy = currentUserId.ToString();
                location.UpdatedAt = DateTime.UtcNow;

                var archiveHistory = new LocationArchiveHistory
                {
                    LocationId = location.Id,
                    ArchivedByUserId = currentUserId,
                    ArchivedAt = DateTime.UtcNow,
                    Reason = string.IsNullOrWhiteSpace(request.Reason) ? null : request.Reason.Trim(),
                    IsRestored = false,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                };

                _context.LocationArchiveHistories.Add(archiveHistory);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Location archived successfully. LocationId: {LocationId}, UserId: {UserId}",
                    request.LocationId,
                    currentUserId
                );

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Location archived successfully.",
                    "تمت أرشفة الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while archiving location. LocationId: {LocationId}, UserId: {UserId}",
                    request.LocationId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An unexpected error occurred while archiving the location.",
                    "حدث خطأ غير متوقع أثناء أرشفة الموقع."
                );
            }
        }
        public async Task<ApiResponse<bool>> RestoreArchivedLocation(int locationId, int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    _logger.LogWarning(
                        "Restore archived location failed. User is not a location owner. UserId: {UserId}",
                        currentUserId
                    );

                    return ApiResponse<bool>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var archivedStatusId = await GetStatus("LocationStatus", "Archived");

                if (archivedStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Archived location status was not found in lookup data.",
                        "حالة الموقع المؤرشف غير موجودة في بيانات النظام."
                    );
                }

                var activeStatusId = await GetStatus("LocationStatus", "Active");

                if (activeStatusId == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Active location status was not found in lookup data.",
                        "حالة الموقع النشط غير موجودة في بيانات النظام."
                    );
                }

                var location = await _context.Locations.Where(x =>
                        x.Id == locationId &&
                        x.LocationOwnerId == locationOwnerId.Value &&
                        !x.IsDeleted).FirstOrDefaultAsync();

                if (location == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Location was not found.",
                        "لم يتم العثور على الموقع."
                    );
                }

                if (location.LocationStatusId != archivedStatusId.Value)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Only archived locations can be restored.",
                        "يمكن استرجاع المواقع المؤرشفة فقط."
                    );
                }


                location.LocationStatusId = activeStatusId.Value;
                location.UpdatedBy = currentUserId.ToString();
                location.UpdatedAt = DateTime.UtcNow;

                var archiveHistory = await _context.LocationArchiveHistories
                    .Where(x =>
                        x.LocationId == location.Id &&
                        !x.IsDeleted &&
                        !x.IsRestored)
                    .OrderByDescending(x => x.ArchivedAt)
                    .FirstOrDefaultAsync();

                if (archiveHistory != null)
                {
                    archiveHistory.IsRestored = true;
                    archiveHistory.RestoredAt = DateTime.UtcNow;
                    archiveHistory.RestoredByUserId = currentUserId;
                    archiveHistory.UpdatedBy = currentUserId.ToString();
                    archiveHistory.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Archived location restored successfully. LocationId: {LocationId}, UserId: {UserId}",
                    locationId,
                    currentUserId
                );

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Location restored successfully.",
                    "تم استرجاع الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while restoring archived location. LocationId: {LocationId}, UserId: {UserId}",
                    locationId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An unexpected error occurred while restoring the location.",
                    "حدث خطأ غير متوقع أثناء استرجاع الموقع."
                );
            }
        }
        public async Task<ApiResponse<List<LocationResponseDto>>> GetMyActiveLocations(int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    _logger.LogWarning(
                        "Get active locations failed. User is not a location owner. UserId: {UserId}",
                        currentUserId
                    );

                    return ApiResponse<List<LocationResponseDto>>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var activeStatusId = await GetStatus("LocationStatus", "Active");

                if (activeStatusId == null)
                {
                    return ApiResponse<List<LocationResponseDto>>.FailureResponse(
                        "Active location status was not found in lookup data.",
                        "حالة الموقع النشط غير موجودة في بيانات النظام."
                    );
                }

                var locations = await _context.Locations
                    .Where(x =>
                        x.LocationOwnerId == locationOwnerId.Value &&
                        x.LocationStatusId == activeStatusId.Value &&
                        !x.IsDeleted)
                    .Select(x => new LocationResponseDto
                    {
                        LocationId = x.Id,
                        LocationName = x.LocationName,
                        LocationDescription = x.LocationDescription,
                        LocationTypeId = x.LocationTypeId,
                        LocationType = x.LocationType.Name,
                        City = x.City,
                        Address = x.Address,
                        DailyPrice = x.DailyPrice,
                        LocationStatus = x.LocationStatus.Name,
                        LocationOnGoogleMaps = x.LocationOnGoogleMaps,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,
                        Country = x.Country,
                        HourlyPrice = x.HourlyPrice,
                        FacilitiesDescription = x.FacilitiesDescription,
                        FacilitiesCount = 0,
                        LocationOwnerName = x.LocationOwner.User.Name,
                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : null,

                        TermsOfUse = x.TermsOfUse
                            .Where(t => !t.IsDeleted)
                            .Select(t => t.TermText)
                            .ToList(),

                        Media = x.Media
                            .Where(m => !m.IsDeleted && !m.Media.IsDeleted)
                            .Select(m => new MediaResponseDto
                            {
                                MediaId = m.Media.Id,
                                FileName = m.Media.FileName,
                                OriginalFileName = m.Media.OriginalFileName,
                                FileUrl = m.Media.FileUrl,
                                ContentType = m.Media.ContentType,
                                SizeInBytes = m.Media.SizeInBytes,
                                MediaType = m.Media.MediaType.Name
                            })
                            .ToList(),

                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();
                foreach (var location in locations)
                {
                    location.FacilitiesCount = GetFacilitiesCount(location.FacilitiesDescription);
                }
                return ApiResponse<List<LocationResponseDto>>.SuccessResponse(
                    locations,
                    "Active locations fetched successfully.",
                    "تم جلب المواقع النشطة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching active locations. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<List<LocationResponseDto>>.FailureResponse(
                    "An unexpected error occurred while fetching active locations.",
                    "حدث خطأ غير متوقع أثناء جلب المواقع النشطة."
                );
            }
        }
        public async Task<ApiResponse<List<ArchivedLocationResponseDto>>> GetMyArchivedLocations(int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    return ApiResponse<List<ArchivedLocationResponseDto>>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var archivedStatusId = await GetStatus("LocationStatus", "Archived");

                if (archivedStatusId == null)
                {
                    return ApiResponse<List<ArchivedLocationResponseDto>>.FailureResponse(
                        "Archived location status was not found in lookup data.",
                        "حالة الموقع المؤرشف غير موجودة في بيانات النظام."
                    );
                }

                var locations = await _context.Locations
                    .Where(x =>
                        x.LocationOwnerId == locationOwnerId.Value &&
                        x.LocationStatusId == archivedStatusId.Value &&
                        !x.IsDeleted)
                    .Select(x => new ArchivedLocationResponseDto
                    {
                        LocationId = x.Id,
                        LocationName = x.LocationName,
                        LocationDescription = x.LocationDescription,

                        LocationTypeId = x.LocationTypeId,
                        LocationType = x.LocationType.Name,

                        Country = x.Country,
                        City = x.City,
                        Address = x.Address,

                        DailyPrice = x.DailyPrice,
                        HourlyPrice = x.HourlyPrice,

                        FacilitiesDescription = x.FacilitiesDescription,
                        FacilitiesCount = 0,

                        LocationStatus = x.LocationStatus.Name,

                        LocationOnGoogleMaps = x.LocationOnGoogleMaps,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,

                        LocationOwnerName = x.LocationOwner.User.Name,

                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : null,

                        TermsOfUse = x.TermsOfUse
                            .Where(t => !t.IsDeleted)
                            .Select(t => t.TermText)
                            .ToList(),

                        Media = x.Media
                            .Where(m => !m.IsDeleted && !m.Media.IsDeleted)
                            .Select(m => new MediaResponseDto
                            {
                                MediaId = m.Media.Id,
                                FileName = m.Media.FileName,
                                OriginalFileName = m.Media.OriginalFileName,
                                FileUrl = m.Media.FileUrl,
                                ContentType = m.Media.ContentType,
                                SizeInBytes = m.Media.SizeInBytes,
                                MediaType = m.Media.MediaType.Name
                            })
                            .ToList(),

                        CreatedAt = x.CreatedAt
                    }).ToListAsync();


                if (!locations.Any())
                {
                    return ApiResponse<List<ArchivedLocationResponseDto>>.SuccessResponse(
                        locations,
                        "No archived locations found.",
                        "لا توجد مواقع مؤرشفة."
                    );
                }

                var locationIds = locations
                    .Select(x => x.LocationId)
                    .ToList();

                var latestArchiveHistories = await _context.LocationArchiveHistories
                    .Where(h =>
                        locationIds.Contains(h.LocationId) &&
                        !h.IsDeleted &&
                        !h.IsRestored)
                    .GroupBy(h => h.LocationId)
                    .Select(g => g
                        .OrderByDescending(h => h.ArchivedAt)
                        .Select(h => new
                        {
                            h.LocationId,
                            h.Reason,
                            h.ArchivedAt,
                            h.ArchivedByUserId
                        })
                        .FirstOrDefault())
                    .ToListAsync();

                var archiveHistoryByLocationId = latestArchiveHistories
                    .Where(x => x != null)
                    .ToDictionary(x => x!.LocationId, x => x!);

                foreach (var location in locations)
                {
                    location.FacilitiesCount = GetFacilitiesCount(location.FacilitiesDescription);

                    if (archiveHistoryByLocationId.TryGetValue(location.LocationId, out var archiveHistory))
                    {
                        location.ArchiveReason = archiveHistory.Reason;
                        location.ArchivedAt = archiveHistory.ArchivedAt;
                    }
                }

                return ApiResponse<List<ArchivedLocationResponseDto>>.SuccessResponse(
                    locations,
                    "Archived locations fetched successfully.",
                    "تم جلب المواقع المؤرشفة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching archived locations. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<List<ArchivedLocationResponseDto>>.FailureResponse(
                    "An unexpected error occurred while fetching archived locations.",
                    "حدث خطأ غير متوقع أثناء جلب المواقع المؤرشفة."
                );
            }
        }
        public async Task<ApiResponse<LocationResponseDto>> GetMyLocationById(int locationId, int currentUserId)
        {
            try
            {
                var locationOwnerId = await GetLocationOwnerIdByUserId(currentUserId);

                if (locationOwnerId == null)
                {
                    _logger.LogWarning(
                        "Get location by id failed. User is not a location owner. UserId: {UserId}",
                        currentUserId
                    );

                    return ApiResponse<LocationResponseDto>.FailureResponse(
                        "Location owner profile was not found.",
                        "لم يتم العثور على ملف صاحب الموقع."
                    );
                }

                var location = await _context.Locations
                    .Where(x =>
                        x.Id == locationId &&
                        x.LocationOwnerId == locationOwnerId.Value &&
                        !x.IsDeleted)
                    .Select(x => new LocationResponseDto
                    {
                        LocationId = x.Id,
                        LocationName = x.LocationName,
                        LocationDescription = x.LocationDescription,

                        LocationTypeId = x.LocationTypeId,
                        LocationType = x.LocationType.Name,

                        City = x.City,
                        Address = x.Address,
                        DailyPrice = x.DailyPrice,

                        LocationStatus = x.LocationStatus.Name,
                        Country = x.Country,
                        HourlyPrice = x.HourlyPrice,
                        FacilitiesDescription = x.FacilitiesDescription,
                        FacilitiesCount = 0,
                        LocationOnGoogleMaps = x.LocationOnGoogleMaps,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,

                        LocationOwnerName = x.LocationOwner.User.Name,

                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : null,

                        TermsOfUse = x.TermsOfUse
                            .Where(t => !t.IsDeleted)
                            .Select(t => t.TermText)
                            .ToList(),

                        Media = x.Media
                            .Where(m => !m.IsDeleted && !m.Media.IsDeleted)
                            .Select(m => new MediaResponseDto
                            {
                                MediaId = m.Media.Id,
                                FileName = m.Media.FileName,
                                OriginalFileName = m.Media.OriginalFileName,
                                FileUrl = m.Media.FileUrl,
                                ContentType = m.Media.ContentType,
                                SizeInBytes = m.Media.SizeInBytes,
                                MediaType = m.Media.MediaType.Name
                            })
                            .ToList(),

                        CreatedAt = x.CreatedAt
                    })
                    .FirstOrDefaultAsync();
                if (location != null)
                {
                    location.FacilitiesCount = GetFacilitiesCount(location.FacilitiesDescription);
                }

                if (location == null)
                {
                    return ApiResponse<LocationResponseDto>.FailureResponse(
                        "Location was not found.",
                        "لم يتم العثور على الموقع."
                    );
                }

                return ApiResponse<LocationResponseDto>.SuccessResponse(
                    location,
                    "Location fetched successfully.",
                    "تم جلب الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching location by id. LocationId: {LocationId}, UserId: {UserId}",
                    locationId,
                    currentUserId
                );

                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "An unexpected error occurred while fetching the location.",
                    "حدث خطأ غير متوقع أثناء جلب الموقع."
                );
            }
        }
        public async Task<ApiResponse<List<LocationResponseDto>>> GetAllActiveLocations()
        {
            try
            {
                var activeStatusId = await GetStatus("LocationStatus", "Active");

                if (activeStatusId == null)
                {
                    return ApiResponse<List<LocationResponseDto>>.FailureResponse(
                        "Active location status was not found in lookup data.",
                        "حالة الموقع النشط غير موجودة في بيانات النظام."
                    );
                }

                var locations = await _context.Locations
                    .Where(x =>
                        x.LocationStatusId == activeStatusId.Value &&
                        !x.IsDeleted)
                    .Select(x => new LocationResponseDto
                    {
                        LocationId = x.Id,
                        LocationName = x.LocationName,
                        LocationDescription = x.LocationDescription,

                        LocationTypeId = x.LocationTypeId,
                        LocationType = x.LocationType.Name,

                        City = x.City,
                        Address = x.Address,
                        DailyPrice = x.DailyPrice,
                        Country = x.Country,
                        HourlyPrice = x.HourlyPrice,
                        FacilitiesDescription = x.FacilitiesDescription,
                        FacilitiesCount = 0,
                        LocationStatus = x.LocationStatus.Name,

                        LocationOnGoogleMaps = x.LocationOnGoogleMaps,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,

                        LocationOwnerName = x.LocationOwner.User.Name,

                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : null,

                        TermsOfUse = x.TermsOfUse
                            .Where(t => !t.IsDeleted)
                            .Select(t => t.TermText)
                            .ToList(),

                        Media = x.Media
                            .Where(m => !m.IsDeleted && !m.Media.IsDeleted)
                            .Select(m => new MediaResponseDto
                            {
                                MediaId = m.Media.Id,
                                FileName = m.Media.FileName,
                                OriginalFileName = m.Media.OriginalFileName,
                                FileUrl = m.Media.FileUrl,
                                ContentType = m.Media.ContentType,
                                SizeInBytes = m.Media.SizeInBytes,
                                MediaType = m.Media.MediaType.Name
                            })
                            .ToList(),

                        CreatedAt = x.CreatedAt
                    })
                    .ToListAsync();
                foreach (var location in locations)
                {
                    location.FacilitiesCount = GetFacilitiesCount(location.FacilitiesDescription);
                }

                return ApiResponse<List<LocationResponseDto>>.SuccessResponse(
                    locations,
                    "Active locations fetched successfully.",
                    "تم جلب المواقع النشطة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching all active locations."
                );

                return ApiResponse<List<LocationResponseDto>>.FailureResponse(
                    "An unexpected error occurred while fetching active locations.",
                    "حدث خطأ غير متوقع أثناء جلب المواقع النشطة."
                );
            }
        }
        public async Task<ApiResponse<List<ArchivedLocationResponseDto>>> GetAllArchivedLocations()
        {
            try
            {
                var archivedStatusId = await GetStatus("LocationStatus", "Archived");

                if (archivedStatusId == null)
                {
                    return ApiResponse<List<ArchivedLocationResponseDto>>.FailureResponse(
                        "Archived location status was not found in lookup data.",
                        "حالة الموقع المؤرشف غير موجودة في بيانات النظام."
                    );
                }

                var locations = await _context.Locations
                    .Where(x =>
                        x.LocationStatusId == archivedStatusId.Value &&
                        !x.IsDeleted)
                    .Select(x => new ArchivedLocationResponseDto
                    {
                        LocationId = x.Id,
                        LocationName = x.LocationName,
                        LocationDescription = x.LocationDescription,

                        LocationTypeId = x.LocationTypeId,
                        LocationType = x.LocationType.Name,

                        City = x.City,
                        Address = x.Address,
                        DailyPrice = x.DailyPrice,

                        LocationStatus = x.LocationStatus.Name,
                        Country = x.Country,
                        HourlyPrice = x.HourlyPrice,
                        FacilitiesDescription = x.FacilitiesDescription,
                        FacilitiesCount = 0,
                        LocationOnGoogleMaps = x.LocationOnGoogleMaps,
                        Latitude = x.Latitude,
                        Longitude = x.Longitude,

                        LocationOwnerName = x.LocationOwner.User.Name,

                        LocationManagerName = x.LocationManager != null
                            ? x.LocationManager.User.Name
                            : null,

                        TermsOfUse = x.TermsOfUse
                            .Where(t => !t.IsDeleted)
                            .Select(t => t.TermText)
                            .ToList(),

                        Media = x.Media
                            .Where(m => !m.IsDeleted && !m.Media.IsDeleted)
                            .Select(m => new MediaResponseDto
                            {
                                MediaId = m.Media.Id,
                                FileName = m.Media.FileName,
                                OriginalFileName = m.Media.OriginalFileName,
                                FileUrl = m.Media.FileUrl,
                                ContentType = m.Media.ContentType,
                                SizeInBytes = m.Media.SizeInBytes,
                                MediaType = m.Media.MediaType.Name
                            })
                            .ToList(),

                        CreatedAt = x.CreatedAt
                    }).ToListAsync();
                if (!locations.Any())
                {
                    return ApiResponse<List<ArchivedLocationResponseDto>>.SuccessResponse(
                        locations,
                        "No archived locations found.",
                        "لا توجد مواقع مؤرشفة."
                    );
                }
                foreach (var location in locations)
                {
                    location.FacilitiesCount = GetFacilitiesCount(location.FacilitiesDescription);
                }

                return ApiResponse<List<ArchivedLocationResponseDto>>.SuccessResponse(
                    locations,
                    "Archived locations fetched successfully.",
                    "تم جلب المواقع المؤرشفة بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching all archived locations."
                );

                return ApiResponse<List<ArchivedLocationResponseDto>>.FailureResponse(
                    "An unexpected error occurred while fetching archived locations.",
                    "حدث خطأ غير متوقع أثناء جلب المواقع المؤرشفة."
                );
            }
        }





        #region Create location private methods


        private ApiResponse<LocationResponseDto>? ValidateCreateLocationRequest(CreateLocationRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.LocationName))
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Location name is required.",
                    "اسم الموقع مطلوب."
                );
            }

            if (string.IsNullOrWhiteSpace(request.LocationDescription))
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Location description is required.",
                    "وصف الموقع مطلوب."
                );
            }

            if (string.IsNullOrWhiteSpace(request.City))
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "City is required.",
                    "المدينة مطلوبة."
                );
            }

            if (request.DailyPrice <= 0)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Daily price must be greater than zero.",
                    "السعر اليومي يجب أن يكون أكبر من صفر."
                );
            }

            if (request.HourlyPrice.HasValue && request.HourlyPrice.Value <= 0)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Hourly price must be greater than zero.",
                    "السعر بالساعة يجب أن يكون أكبر من صفر."
                );
            }

            if (string.IsNullOrWhiteSpace(request.Country))
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Country is required.",
                    "الدولة مطلوبة."
                );
            }

            if (request.TermsOfUse == null || !request.TermsOfUse.Any())
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "At least one term of use is required.",
                    "يجب إضافة شرط استخدام واحد على الأقل."
                );
            }

            if (request.TermsOfUse.Any(x => string.IsNullOrWhiteSpace(x)))
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Terms of use cannot contain empty values.",
                    "شروط الاستخدام لا يمكن أن تحتوي على قيم فارغة."
                );
            }

            if (request.MediaIds == null || !request.MediaIds.Any())
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "At least one image and one video are required.",
                    "يجب إضافة صورة واحدة على الأقل وفيديو واحد."
                );
            }

            if (request.LocationTypeId <= 0)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Location type is required.",
                    "نوع الموقع مطلوب."
                );
            }

            return null;
        }

        private async Task<ApiResponse<LocationResponseDto>?> ValidateLocationMedia(List<Media> mediaItems,List<int> requestedMediaIds)
        {
            var distinctMediaIds = requestedMediaIds.Distinct().ToList();

            var alreadyLinked = await _context.LocationMedia
                .AnyAsync(x =>
                    distinctMediaIds.Contains(x.MediaId) &&
                    !x.IsDeleted);

            if (alreadyLinked)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "One or more media files are already linked to another location.",
                    "واحد أو أكثر من الملفات مرتبط بموقع آخر."
                );
            }

            var hasImage = mediaItems.Any(x =>
                x.MediaType != null &&
                x.MediaType.Name == "Image");

            

            if (!hasImage)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "At least one image are required.",
                    "يجب إضافة صورة واحدة على الأقل ."
                );
            }

            return null;
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

        private async Task<int?> GetStatus(string categoryName,string itemName)
        {
            return await _context.LookupItems
                .Where(x =>
                    x.Name == itemName &&
                    x.LookupCategory.Name == categoryName &&
                    !x.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }

        private static MediaResponseDto MapMediaResponse(Media media)
        {
            return new MediaResponseDto
            {
                MediaId = media.Id,
                FileName = media.FileName,
                OriginalFileName = media.OriginalFileName,
                FileUrl = media.FileUrl,
                ContentType = media.ContentType,
                SizeInBytes = media.SizeInBytes,
                MediaType = media.MediaType?.Name ?? string.Empty
            };
        }

        private async Task<bool> IsValidLocationType(string categoryName, int lookupItemId)
        {
            return await _context.LookupItems.AnyAsync(x =>
                x.Id == lookupItemId &&
                x.LookupCategory.Name == categoryName &&
                !x.IsDeleted);
        }

        private async Task<string> GetLookupItemNameById(int lookupItemId)
        {
            return await _context.LookupItems
                .Where(x => x.Id == lookupItemId && !x.IsDeleted)
                .Select(x => x.Name)
                .FirstOrDefaultAsync() ?? string.Empty;
        }

        #endregion


        #region Update location private methods
        private ApiResponse<LocationResponseDto>? ValidateUpdateLocationRequest(UpdateLocationRequestDto request)
        {
            if (request.LocationTypeId.HasValue && request.LocationTypeId.Value <= 0)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Invalid location type.",
                    "نوع الموقع غير صحيح."
                );
            }

            if (request.DailyPrice.HasValue && request.DailyPrice.Value <= 0)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Daily price must be greater than zero.",
                    "السعر اليومي يجب أن يكون أكبر من صفر."
                );
            }

            if (request.TermsOfUse != null)
            {
                if (!request.TermsOfUse.Any())
                {
                    return ApiResponse<LocationResponseDto>.FailureResponse(
                        "At least one term of use is required.",
                        "يجب إضافة شرط استخدام واحد على الأقل."
                    );
                }

                if (request.TermsOfUse.Any(x => string.IsNullOrWhiteSpace(x)))
                {
                    return ApiResponse<LocationResponseDto>.FailureResponse(
                        "Terms of use cannot contain empty values.",
                        "شروط الاستخدام لا يمكن أن تحتوي على قيم فارغة."
                    );
                }
            }
            if (request.HourlyPrice.HasValue && request.HourlyPrice.Value <= 0)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Hourly price must be greater than zero.",
                    "السعر بالساعة يجب أن يكون أكبر من صفر."
                );
            }
            if (request.Country != null && string.IsNullOrWhiteSpace(request.Country))
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "Country cannot be empty.",
                    "الدولة لا يمكن أن تكون فارغة."
                );
            }

            if (request.MediaIds != null && !request.MediaIds.Any())
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "At least one image and one video are required.",
                    "يجب إضافة صورة واحدة على الأقل وفيديو واحد."
                );
            }

            return null;
        }

        private static void UpdateLocationTerms(Location location,List<string> termsOfUse,int currentUserId)
        {
            var cleanTerms = termsOfUse
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var activeTerms = location.TermsOfUse
                .Where(x => !x.IsDeleted)
                .ToList();

            foreach (var existingTerm in activeTerms)
            {
                existingTerm.IsDeleted = true;
                existingTerm.IsActive = false;
                existingTerm.UpdatedBy = currentUserId.ToString();
                existingTerm.UpdatedAt = DateTime.UtcNow;
            }

            foreach (var term in cleanTerms)
            {
                location.TermsOfUse.Add(new LocationTermsOfUse
                {
                    LocationId = location.Id,
                    TermText = term,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private static void UpdateLocationMedia(Location location,List<Media> mediaItems,int currentUserId)
        {
            var requestedMediaIds = mediaItems
                .Select(x => x.Id)
                .Distinct()
                .ToList();

            var activeMediaLinks = location.Media
                .Where(x => !x.IsDeleted)
                .ToList();

            foreach (var existingMediaLink in activeMediaLinks)
            {
                if (!requestedMediaIds.Contains(existingMediaLink.MediaId))
                {
                    existingMediaLink.IsDeleted = true;
                    existingMediaLink.IsActive = false;
                    existingMediaLink.UpdatedBy = currentUserId.ToString();
                    existingMediaLink.UpdatedAt = DateTime.UtcNow;
                }
            }

            var existingActiveMediaIds = location.Media
                .Where(x => !x.IsDeleted)
                .Select(x => x.MediaId)
                .ToHashSet();

            var newMediaItems = mediaItems
                .Where(media => !existingActiveMediaIds.Contains(media.Id))
                .ToList();

            foreach (var media in newMediaItems)
            {
                location.Media.Add(new LocationMedia
                {
                    LocationId = location.Id,
                    MediaId = media.Id,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }

        private async Task<ApiResponse<LocationResponseDto>?> ValidateLocationMediaForUpdate(List<Media> mediaItems,List<int> requestedMediaIds,int currentLocationId)
        {
            var distinctMediaIds = requestedMediaIds.Distinct().ToList();

            var linkedToOtherLocation = await _context.LocationMedia
                .AnyAsync(x =>
                    distinctMediaIds.Contains(x.MediaId) &&
                    x.LocationId != currentLocationId &&
                    !x.IsDeleted);

            if (linkedToOtherLocation)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "One or more media files are already linked to another location.",
                    "واحد أو أكثر من الملفات مرتبط بموقع آخر."
                );
            }

            var hasImage = mediaItems.Any(x =>
                x.MediaType != null &&
                x.MediaType.Name == "Image");

            var hasVideo = mediaItems.Any(x =>
                x.MediaType != null &&
                x.MediaType.Name == "Video");

            if (!hasImage || !hasVideo)
            {
                return ApiResponse<LocationResponseDto>.FailureResponse(
                    "At least one image and one video are required.",
                    "يجب إضافة صورة واحدة على الأقل وفيديو واحد."
                );
            }

            return null;
        }
        #endregion


        private static int GetFacilitiesCount(string? facilitiesDescription)
        {
            if (string.IsNullOrWhiteSpace(facilitiesDescription))
                return 0;

            return facilitiesDescription
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .Count(x => !string.IsNullOrWhiteSpace(x));
        }
    }
}
