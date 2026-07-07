using FilmMaker.Common;
using FilmMaker.DTO.Lookup;

namespace FilmMaker.Services.Interface
{
    public interface ILookupService
    {
        Task<ApiResponse<List<LookupItemDto>>> GetLookupByCategory(string category);
    }
}
