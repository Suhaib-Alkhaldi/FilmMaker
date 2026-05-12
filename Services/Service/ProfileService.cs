using FilmMaker.Common;
using FilmMaker.DTO.Profile.Request;
using FilmMaker.DTO.Profile.Response;
using FilmMaker.Entities;
using FilmMaker.Helper.Validation;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class ProfileService : IProfileService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<AuthService> _logger;

        public ProfileService(FilmMakerDbContext context, ILogger<AuthService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<LocationManagerProfileResponseDto>> UpdateLocationManagerProfile(UpdateLocationManagerProfileRequestDto request, int currentUserId)
        {
            if (request == null)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            var validationError = await ValidateUpdateLocationManagerProfileRequest(request);
            if (validationError != null)
                return validationError;

            var profile = await _context.LocationManagerProfiles
                .Include(x => x.User)
                .Include(x => x.Cities)
                .Include(x => x.PreviousProjects)
                .Where(x => x.UserId == currentUserId &&!x.IsDeleted)
                .FirstOrDefaultAsync();

            if (profile == null)
            {
                _logger.LogWarning(
                    "Location manager profile was not found for update. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Location manager profile was not found.",
                    "لم يتم العثور على ملف مدير الموقع."
                );
            }


            try
            {
                profile.User.Name = request.Name.Trim();
                profile.User.PhoneNumber = request.PhoneNumber.Trim();
                profile.User.IBAN = string.IsNullOrWhiteSpace(request.IBAN)
                    ? null
                    : request.IBAN.Trim();

                profile.User.UpdatedBy = currentUserId.ToString();
                profile.User.UpdatedAt = DateTime.UtcNow;

                profile.YearsOfExperience = request.YearsOfExperience;
                profile.Description = request.Description?.Trim();
                profile.CommissionRate = request.CommissionRate;
                profile.UpdatedBy = currentUserId.ToString();
                profile.UpdatedAt = DateTime.UtcNow;

                UpdateLocationManagerCities(profile, request.CityId, currentUserId);
                UpdatePreviousProjects(profile, request.PreviousProjects, currentUserId);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Location manager profile updated successfully. UserId: {UserId}, ProfileId: {ProfileId}",
                    currentUserId,
                    profile.Id
                );

                return await GetMyLocationManagerProfile(currentUserId);
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Error while updating location manager profile. UserId: {UserId}, ProfileId: {ProfileId}",
                    currentUserId,
                    profile.Id
                );

                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "An unexpected error occurred while updating the profile.",
                    "حدث خطأ غير متوقع أثناء تحديث الملف الشخصي."
                );
            }
        }
        
        public async Task<ApiResponse<LocationManagerProfileResponseDto>> GetMyLocationManagerProfile(int currentUserId)
        {
            try
            {
                var profile = await _context.LocationManagerProfiles
                    .Where(x => x.UserId == currentUserId && !x.IsDeleted)
                    .Select(x => new LocationManagerProfileResponseDto
                    {
                        ProfileId = x.Id,
                        UserId = x.UserId,

                        Name = x.User.Name,
                        Email = x.User.Email,
                        PhoneNumber = x.User.PhoneNumber,
                        IBAN = x.User.IBAN,

                        YearsOfExperience = x.YearsOfExperience,
                        Description = x.Description,
                        CommissionRate = x.CommissionRate,
                        Rate = x.Rate,

                        Cities = x.Cities
                            .Where(c => !c.IsDeleted)
                            .Select(c => c.City.Name)
                            .ToList(),

                        PreviousProjects = x.PreviousProjects
                            .Where(p => !p.IsDeleted)
                            .Select(p => p.ProjectName)
                            .ToList()
                    })
                    .FirstOrDefaultAsync();

                if (profile == null)
                {
                    _logger.LogWarning(
                        "Location manager profile was not found. UserId: {UserId}",
                        currentUserId
                    );

                    return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                        "Location manager profile was not found.",
                        "لم يتم العثور على ملف مدير الموقع."
                    );
                }

                profile.IsProfileCompleted = IsLocationManagerProfileCompleted(profile);

                return ApiResponse<LocationManagerProfileResponseDto>.SuccessResponse(
                    profile,
                    "Location manager profile fetched successfully.",
                    "تم جلب ملف مدير الموقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while fetching location manager profile. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "An unexpected error occurred while fetching the profile.",
                    "حدث خطأ غير متوقع أثناء جلب الملف الشخصي."
                );
            }
        }
        public async Task<ApiResponse<LocationManagerProfileResponseDto>> CompleteLocationManagerProfile(
            CompleteLocationManagerProfileRequestDto request,int currentUserId)
        {
            if (request == null)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Invalid request.",
                    "الطلب غير صحيح."
                );
            }

            var validationError = await ValidateCompleteLocationManagerProfileRequest(request);
            if (validationError != null)
                return validationError;

            var profile = await _context.LocationManagerProfiles
                .Include(x => x.Cities)
                .Include(x => x.PreviousProjects)
                .FirstOrDefaultAsync(x =>
                    x.UserId == currentUserId &&
                    !x.IsDeleted);

            if (profile == null)
            {
                _logger.LogWarning(
                    "Location manager profile was not found for complete profile. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Location manager profile was not found.",
                    "لم يتم العثور على ملف مدير الموقع."
                );
            }

            if (IsLocationManagerProfileAlreadyCompleted(profile))
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Location manager profile is already completed.",
                    "ملف مدير الموقع مكتمل مسبقًا."
                );
            }


            try
            {
                profile.YearsOfExperience = request.YearsOfExperience;
                profile.Description = request.Description?.Trim();
                profile.CommissionRate = request.CommissionRate;
                profile.UpdatedBy = currentUserId.ToString();
                profile.UpdatedAt = DateTime.UtcNow;

                UpdateLocationManagerCities(profile, request.CityIds, currentUserId);
                UpdatePreviousProjects(profile, request.PreviousProjects, currentUserId);

                await _context.SaveChangesAsync();

                _logger.LogInformation(
                    "Location manager profile completed successfully. UserId: {UserId}, ProfileId: {ProfileId}",
                    currentUserId,
                    profile.Id
                );

                return await GetMyLocationManagerProfile(currentUserId);
            }
            catch (Exception ex)
            {

                _logger.LogError(
                    ex,
                    "Error while completing location manager profile. UserId: {UserId}, ProfileId: {ProfileId}",
                    currentUserId,
                    profile.Id
                );

                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "An unexpected error occurred while completing the profile.",
                    "حدث خطأ غير متوقع أثناء إكمال الملف الشخصي."
                );
            }
        }



        private async Task<ApiResponse<LocationManagerProfileResponseDto>?> ValidateUpdateLocationManagerProfileRequest(UpdateLocationManagerProfileRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Name is required.",
                    "الاسم مطلوب."
                );
            }

            if (string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Phone number is required.",
                    "رقم الهاتف مطلوب."
                );
            }

            if (!PhoneNumberValidation.IsValidPhoneNumber(request.PhoneNumber))
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Invalid phone number format.",
                    "صيغة رقم الهاتف غير صحيحة."
                );
            }

            if (request.YearsOfExperience.HasValue && request.YearsOfExperience.Value < 0)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Years of experience cannot be negative.",
                    "سنوات الخبرة لا يمكن أن تكون قيمة سالبة."
                );
            }

            if (request.CommissionRate.HasValue && request.CommissionRate.Value < 0)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Commission rate cannot be negative.",
                    "نسبة العمولة لا يمكن أن تكون قيمة سالبة."
                );
            }

            if (request.CityId != null && !request.CityId.Any())
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "At least one city is required.",
                    "يجب اختيار مدينة واحدة على الأقل."
                );
            }

            var cityValidationError = await ValidateCity(request.CityId);
            if (cityValidationError != null)
                return cityValidationError;

            return null;
        }
        private static bool IsLocationManagerProfileAlreadyCompleted(LocationManagerProfile profile)
        {
            return profile.YearsOfExperience.HasValue &&
                   profile.YearsOfExperience.Value >= 0 &&
                   profile.Cities.Any(x => !x.IsDeleted);
        }
        private static void UpdateLocationManagerCities(LocationManagerProfile profile,List<int> cityIds,int currentUserId)
        {
            var requestedCityIds = cityIds
                .Distinct()
                .ToList();

            var activeCities = profile.Cities
                .Where(x => !x.IsDeleted)
                .ToList();

            foreach (var existingCity in activeCities)
            {
                if (!requestedCityIds.Contains(existingCity.CityId))
                {
                    existingCity.IsDeleted = true;
                    existingCity.IsActive = false;
                    existingCity.UpdatedBy = currentUserId.ToString();
                    existingCity.UpdatedAt = DateTime.UtcNow;
                }
            }

            var existingActiveCityIds = profile.Cities
                .Where(x => !x.IsDeleted)
                .Select(x => x.CityId)
                .ToHashSet();

            var newCityIds = requestedCityIds
                .Where(cityId => !existingActiveCityIds.Contains(cityId))
                .ToList();

            foreach (var cityId in newCityIds)
            {
                profile.Cities.Add(new LocationManagerCity
                {
                    LocationManagerProfileId = profile.Id,
                    CityId = cityId,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        private static void UpdatePreviousProjects(LocationManagerProfile profile,List<string> previousProjects,int currentUserId)
        {
            var requestedProjects = previousProjects
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .Select(x => x.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var activeProjects = profile.PreviousProjects
                .Where(x => !x.IsDeleted)
                .ToList();

            foreach (var existingProject in activeProjects)
            {
                var stillExists = requestedProjects.Any(x =>
                    string.Equals(
                        x,
                        existingProject.ProjectName,
                        StringComparison.OrdinalIgnoreCase));

                if (!stillExists)
                {
                    existingProject.IsDeleted = true;
                    existingProject.IsActive = false;
                    existingProject.UpdatedBy = currentUserId.ToString();
                    existingProject.UpdatedAt = DateTime.UtcNow;
                }
            }

            var existingActiveProjectNames = profile.PreviousProjects
                .Where(x => !x.IsDeleted)
                .Select(x => x.ProjectName)
                .ToHashSet(StringComparer.OrdinalIgnoreCase);

            var newProjects = requestedProjects
                .Where(projectName => !existingActiveProjectNames.Contains(projectName))
                .ToList();

            foreach (var projectName in newProjects)
            {
                profile.PreviousProjects.Add(new PreviousProject
                {
                    LocationManagerProfileId = profile.Id,
                    ProjectName = projectName,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = currentUserId.ToString(),
                    CreatedAt = DateTime.UtcNow
                });
            }
        }
        private static bool IsLocationManagerProfileCompleted(LocationManagerProfileResponseDto profile)
        {
            return profile.YearsOfExperience.HasValue &&
                   profile.YearsOfExperience.Value >= 0 &&
                   profile.Cities.Any();
        }
        private async Task<ApiResponse<LocationManagerProfileResponseDto>?> ValidateCompleteLocationManagerProfileRequest(
            CompleteLocationManagerProfileRequestDto request)
        {
            if (!request.YearsOfExperience.HasValue)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Years of experience is required.",
                    "سنوات الخبرة مطلوبة."
                );
            }

            if (request.YearsOfExperience.Value < 0)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Years of experience cannot be negative.",
                    "سنوات الخبرة لا يمكن أن تكون قيمة سالبة."
                );
            }

            if (request.CommissionRate.HasValue && request.CommissionRate.Value < 0)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "Commission rate cannot be negative.",
                    "نسبة العمولة لا يمكن أن تكون قيمة سالبة."
                );
            }

            if (request.CityIds == null || !request.CityIds.Any())
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "At least one city is required.",
                    "يجب اختيار مدينة واحدة على الأقل."
                );
            }

            var cityValidationError = await ValidateCity(request.CityIds);
            if (cityValidationError != null)
                return cityValidationError;

            return null;
        }

        private async Task<ApiResponse<LocationManagerProfileResponseDto>?> ValidateCity(List<int> cityIds)
        {
            if (cityIds == null || !cityIds.Any())
                return null;

            var distinctCityIds = cityIds.Distinct().ToList();

            var validCityIds = await _context.LookupItems
                .Where(x =>
                    distinctCityIds.Contains(x.Id) &&
                    x.LookupCategory.Name == "City" &&
                    !x.IsDeleted)
                .Select(x => x.Id)
                .ToListAsync();

            var hasInvalidCity = distinctCityIds.Any(id => !validCityIds.Contains(id));

            if (hasInvalidCity)
            {
                return ApiResponse<LocationManagerProfileResponseDto>.FailureResponse(
                    "One or more selected cities are invalid.",
                    "واحدة أو أكثر من المدن المحددة غير صحيحة."
                );
            }

            return null;
        }
    }
}
