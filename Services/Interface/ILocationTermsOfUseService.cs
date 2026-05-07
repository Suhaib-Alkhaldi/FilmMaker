using FilmMaker.Common;
using FilmMaker.DTO.TermsOfUse;

namespace FilmMaker.Services.Interface
{
    public interface ILocationTermsOfUseService
    {
        Task<ApiResponse<List<LocationTermsOfUseDTO>>> AllTermsOfUseByLocationAsync(int locationId);
        Task<ApiResponse<LocationTermsOfUseDTO>> UpdateTermsOfUseAsync(LocationTermsOfUseDTO updateLocationTermOfUseDTO, int UserId);
        Task<ApiResponse<LocationTermsOfUseDTO>> CreateTermsOfUseAsync(LocationTermsOfUseDTO createLocationTermOfUseDTO, int UserId);
        Task<ApiResponse<bool>> DeleteTermsOfUseAsync(int locationId, int UserId, int TermId);

        Task<ApiResponse<List<LocationTermsOfUseDTO>>> CreateManyTermsOfUseAsync(
        List<LocationTermsOfUseDTO> createTermsDTOs, int UserId);

        Task<ApiResponse<List<LocationTermsOfUseDTO>>> UpdateManyTermsOfUseAsync(
        List<LocationTermsOfUseDTO> updateTermsDTOs, int UserId);
    }
}
