using FilmMaker.Common;
using FilmMaker.DTO.Lookup;
using FilmMaker.DTO.Lookup.Location;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class LookupService : ILookupService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<LookupService> _logger;

        public LookupService(FilmMakerDbContext context, ILogger<LookupService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<ApiResponse<LocationListResponseDto>> GetLocationsAsync(LocationFilterDto filter)
        {
            try
            {
                var query = _context.Locations
                    .Include(l => l.LocationStatus)
                    .Include(l => l.Media)
                    .Where(l => l.IsActive && !l.IsDeleted)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(filter.City))
                {
                    query = query.Where(l =>
                        l.City.Contains(filter.City));
                }

                if (filter.MinPrice.HasValue)
                {
                    query = query.Where(l => l.DailyPrice >= filter.MinPrice.Value);
                }

                if (filter.MaxPrice.HasValue)
                {
                    query = query.Where(l => l.DailyPrice <= filter.MaxPrice.Value);
                }

                var totalCount = await query.CountAsync();

                var locations = await query
                    .OrderBy(l => l.Id)
                    .Skip((filter.Page - 1) * filter.PageSize)
                    .Take(filter.PageSize)
                    .Select(l => new LocationSummaryDto
                    {
                        Id = l.Id,
                        LocationName = l.LocationName,
                        City = l.City,
                        Address = l.Address,
                        DailyPrice = l.DailyPrice,
                        LocationStatus = l.LocationStatus.Name,
                        MediaUrl = l.Media.FirstOrDefault() != null
                            ? l.Media.First().FileUrl
                            : null
                    })
                    .ToListAsync();

                var response = new LocationListResponseDto
                {
                    Locations = locations,
                    TotalCount = totalCount,
                    Page = filter.Page,
                    PageSize = filter.PageSize,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)filter.PageSize)
                };

                return ApiResponse<LocationListResponseDto>.SuccessResponse(
                    response,
                    "Locations retrieved successfully.",
                    "تم جلب المواقع بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting locations with filter");

                return ApiResponse<LocationListResponseDto>.FailureResponse(
                    "An error occurred while retrieving locations.",
                    "حدث خطأ أثناء جلب المواقع."
                );
            }
        }
    }
}
