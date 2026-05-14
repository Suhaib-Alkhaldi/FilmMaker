using FilmMaker.Common;
using FilmMaker.DTO.Lookup.Location;

namespace FilmMaker.Services.Interface
{
    public interface ILookupService
    {
        Task<ApiResponse<LocationListResponseDto>> GetLocationsAsync(LocationFilterDto filter);
    }
}
