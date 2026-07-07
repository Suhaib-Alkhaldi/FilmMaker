using FilmMaker.Common;
using FilmMaker.DTO.Lookup;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;

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

        public async Task<ApiResponse<List<LookupItemDto>>> GetLookupByCategory(string category)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(category))
                {
                    return ApiResponse<List<LookupItemDto>>.FailureResponse(
                        "Category is required.",
                        "اسم التصنيف مطلوب."
                    );
                }

                var normalizedCategory = Normalize(category);

                var data = await _context.LookupItems
                    .AsNoTracking()
                    .Where(x => x.LookupCategory.Name.ToLower() == normalizedCategory)
                    .Select(x => new LookupItemDto
                    {
                        Id = x.Id,
                        Name = x.Name
                    })
                    .ToListAsync();
                if(data == null || !data.Any())
                {
                    return ApiResponse<List<LookupItemDto>>.FailureResponse(
                        "No lookup items found for the specified category.",
                        "لم يتم العثور على عناصر للبحث عن التصنيف المحدد."
                    );
                }

                return ApiResponse<List<LookupItemDto>>.SuccessResponse(
                    data,
                    "Lookup retrieved successfully.",
                    "تم جلب البيانات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting lookup by category {Category}", category);

                return ApiResponse<List<LookupItemDto>>.FailureResponse(
                    "An error occurred while retrieving lookup.",
                    "حدث خطأ أثناء جلب البيانات."
                );
            }
        }

        private string Normalize(string input)
        {
            return input
                .Trim()
                .Replace(" ", "")
                .ToLower();
        }
    }
}
