using FilmMaker.Common;
using FilmMaker.DTO.Location;
using FilmMaker.DTO.TermsOfUse;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LocationTermsOfUseService : ILocationTermsOfUseService
    {
        private readonly FilmMakerDbContext _context;

        public LocationTermsOfUseService(FilmMakerDbContext context)
        {
            _context = context;
        }

        public async Task<ApiResponse<List<LocationTermsOfUseDTO>>> UpdateManyTermsOfUseAsync(
    List<LocationTermsOfUseDTO> updateTermsDTOs, int UserId)
        {
            try
            {
                var ownerId = await GetLocationOwnerIdByUserId(UserId, _context);
                if (ownerId == null)
                    return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                        "Owner profile not found.",
                        "لم يتم العثور على ملف المالك.");

                // Extract all requested IDs and fetch them in one round-trip
                var termIds = updateTermsDTOs.Select(d => d.Id).ToList();

                var existingTerms = await _context.locationTermsOfUse
                    .Include(t => t.Location)
                    .Where(t => termIds.Contains(t.Id)
                             && t.Location.LocationOwnerId == ownerId
                             && !t.IsDeleted)
                    .ToListAsync();

                // Check every requested ID was actually found and is owned by this user
                var foundIds = existingTerms.Select(t => t.Id).ToHashSet();
                var missingIds = termIds.Where(id => !foundIds.Contains(id)).ToList();

                if (missingIds.Any())
                    return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                        $"Terms with IDs [{string.Join(", ", missingIds)}] were not found or unauthorized.",
                        $"الشروط ذات المعرفات [{string.Join(", ", missingIds)}] غير موجودة أو غير مصرح بها.");

                // Build a lookup for O(1) access
                var updateLookup = updateTermsDTOs.ToDictionary(d => d.Id);

                foreach (var term in existingTerms)
                {
                    term.Term = updateLookup[term.Id].Term;
                    term.UpdatedBy = UserId.ToString();
                    term.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();

                var resultDTOs = existingTerms.Select(t => new LocationTermsOfUseDTO
                {
                    Id = t.Id,
                    LocationId = t.LocationId,
                    Term = t.Term
                }).ToList();

                return ApiResponse<List<LocationTermsOfUseDTO>>.SuccessResponse(
                    resultDTOs,
                    $"{resultDTOs.Count} terms updated successfully.",
                    $"تم تحديث {resultDTOs.Count} شروط بنجاح.");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                    $"An error occurred: {ex.Message}",
                    "حدث خطأ أثناء تحديث الشروط.");
            }
        }

        public async Task<ApiResponse<List<LocationTermsOfUseDTO>>> CreateManyTermsOfUseAsync(
    List<LocationTermsOfUseDTO> createTermsDTOs, int UserId)
        {
            try
            {
                var ownerId = await GetLocationOwnerIdByUserId(UserId, _context);
                if (ownerId == null)
                    return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                        "Owner profile not found.",
                        "لم يتم العثور على ملف المالك.");

                // All terms must belong to the same location — validate it once
                var locationId = createTermsDTOs.Select(t => t.LocationId).Distinct().ToList();
                if (locationId.Count > 1)
                    return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                        "All terms must belong to the same location.",
                        "يجب أن تنتمي جميع الشروط إلى نفس الموقع.");

                var locationExists = await _context.locations
                    .AnyAsync(l => l.Id == locationId.First()
                                && l.LocationOwnerId == ownerId
                                && !l.IsDeleted);

                if (!locationExists)
                    return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                        "Location not found or unauthorized.",
                        "الموقع غير موجود أو غير مصرح.");

                var terms = createTermsDTOs.Select(dto => new LocationTermsOfUse
                {
                    LocationId = dto.LocationId,
                    Term = dto.Term,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = UserId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = UserId.ToString(),
                    UpdatedAt = DateTime.UtcNow
                }).ToList();

                await _context.locationTermsOfUse.AddRangeAsync(terms);
                await _context.SaveChangesAsync();

                var resultDTOs = terms.Select(t => new LocationTermsOfUseDTO
                {
                    Id = t.Id,
                    LocationId = t.LocationId,
                    Term = t.Term
                }).ToList();

                return ApiResponse<List<LocationTermsOfUseDTO>>.SuccessResponse(
                    resultDTOs,
                    $"{resultDTOs.Count} terms created successfully.",
                    $"تم إنشاء {resultDTOs.Count} شروط بنجاح.");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                    $"An error occurred: {ex.Message}",
                    "حدث خطأ أثناء إنشاء الشروط.");
            }
        }

        public async Task<ApiResponse<LocationTermsOfUseDTO>> CreateTermsOfUseAsync(
            LocationTermsOfUseDTO createLocationTermOfUseDTO, int UserId)
        {
            try
            {
                var ownerId = await GetLocationOwnerIdByUserId(UserId, _context);
                if (ownerId == null)
                    return ApiResponse<LocationTermsOfUseDTO>.FailureResponse(
                        "Owner profile not found.",
                        "لم يتم العثور على ملف المالك.");

                var locationExists = await _context.locations
                    .AnyAsync(l => l.Id == createLocationTermOfUseDTO.LocationId
                                && l.LocationOwnerId == ownerId
                                && !l.IsDeleted);

                if (!locationExists)
                    return ApiResponse<LocationTermsOfUseDTO>.FailureResponse(
                        "Location not found or unauthorized.",
                        "الموقع غير موجود أو غير مصرح.");

                var term = new LocationTermsOfUse
                {
                    LocationId = createLocationTermOfUseDTO.LocationId,
                    Term = createLocationTermOfUseDTO.Term,
                    IsActive = true,
                    IsDeleted = false,
                    CreatedBy = UserId.ToString(),
                    CreatedAt = DateTime.UtcNow,
                    UpdatedBy = UserId.ToString(),
                    UpdatedAt = DateTime.UtcNow
                };

                await _context.locationTermsOfUse.AddAsync(term);
                await _context.SaveChangesAsync();

                createLocationTermOfUseDTO.Id = term.Id;

                return ApiResponse<LocationTermsOfUseDTO>.SuccessResponse(
                    createLocationTermOfUseDTO,
                    "Term created successfully.",
                    "تم إنشاء الشرط بنجاح.");
            }
            catch (Exception ex)
            {
                return ApiResponse<LocationTermsOfUseDTO>.FailureResponse(
                    $"An error occurred: {ex.Message}",
                    "حدث خطأ أثناء إنشاء الشرط.");
            }
        }

        public async Task<ApiResponse<LocationTermsOfUseDTO>> UpdateTermsOfUseAsync(
            LocationTermsOfUseDTO updateLocationTermOfUseDTO, int UserId)
        {
            try
            {
                var ownerId = await GetLocationOwnerIdByUserId(UserId, _context);
                if (ownerId == null)
                    return ApiResponse<LocationTermsOfUseDTO>.FailureResponse(
                        "Owner profile not found.",
                        "لم يتم العثور على ملف المالك.");

                // Make sure the term belongs to a location owned by this user
                var term = await _context.locationTermsOfUse
                    .Include(t => t.Location)
                    .FirstOrDefaultAsync(t => t.Id == updateLocationTermOfUseDTO.Id
                                           && t.LocationId == updateLocationTermOfUseDTO.LocationId
                                           && t.Location.LocationOwnerId == ownerId
                                           && !t.IsDeleted);

                if (term == null)
                    return ApiResponse<LocationTermsOfUseDTO>.FailureResponse(
                        "Term not found or unauthorized.",
                        "الشرط غير موجود أو غير مصرح.");

                term.Term = updateLocationTermOfUseDTO.Term;
                term.UpdatedBy = UserId.ToString();
                term.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<LocationTermsOfUseDTO>.SuccessResponse(
                    updateLocationTermOfUseDTO,
                    "Term updated successfully.",
                    "تم تحديث الشرط بنجاح.");
            }
            catch (Exception ex)
            {
                return ApiResponse<LocationTermsOfUseDTO>.FailureResponse(
                    $"An error occurred: {ex.Message}",
                    "حدث خطأ أثناء تحديث الشرط.");
            }
        }

        public async Task<ApiResponse<bool>> DeleteTermsOfUseAsync(
            int locationId, int UserId, int TermId)
        {
            try
            {
                var ownerId = await GetLocationOwnerIdByUserId(UserId, _context);
                if (ownerId == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Owner profile not found.",
                        "لم يتم العثور على ملف المالك.");

                var term = await _context.locationTermsOfUse
                    .Include(t => t.Location)
                    .FirstOrDefaultAsync(t => t.Id == TermId
                                           && t.LocationId == locationId
                                           && t.Location.LocationOwnerId == ownerId
                                           && !t.IsDeleted);

                if (term == null)
                    return ApiResponse<bool>.FailureResponse(
                        "Term not found or unauthorized.",
                        "الشرط غير موجود أو غير مصرح.");

                // Soft delete
                term.IsDeleted = true;
                term.IsActive = false;
                term.UpdatedBy = UserId.ToString();
                term.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Term deleted successfully.",
                    "تم حذف الشرط بنجاح.");
            }
            catch (Exception ex)
            {
                return ApiResponse<bool>.FailureResponse(
                    $"An error occurred: {ex.Message}",
                    "حدث خطأ أثناء حذف الشرط.");
            }
        }

        public async Task<ApiResponse<List<LocationTermsOfUseDTO>>> AllTermsOfUseByLocationAsync(int locationId)
        {
            try
            {
                var terms = await _context.locationTermsOfUse
                    .Where(t => t.LocationId == locationId && !t.IsDeleted)
                    .Select(t => new LocationTermsOfUseDTO
                    {
                        Id = t.Id,
                        LocationId = t.LocationId,
                        Term = t.Term
                    })
                    .ToListAsync();

                return ApiResponse<List<LocationTermsOfUseDTO>>.SuccessResponse(
                    terms,
                    "Terms retrieved successfully.",
                    "تم جلب الشروط بنجاح.");
            }
            catch (Exception ex)
            {
                return ApiResponse<List<LocationTermsOfUseDTO>>.FailureResponse(
                    $"An error occurred: {ex.Message}",
                    "حدث خطأ أثناء جلب الشروط.");
            }
        }

        // ── Private helper ────────────────────────────────────────────────────
        private static async Task<int?> GetLocationOwnerIdByUserId(
            int userId, FilmMakerDbContext context)
        {
            var locationOwner = await context.locationOwnerProfiles
                .FirstOrDefaultAsync(x => x.UserId == userId);

            return locationOwner?.Id;
        }

  
    }
}