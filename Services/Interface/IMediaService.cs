using FilmMaker.Common;
using FilmMaker.DTO.Media;
using FilmMaker.Entities;

namespace FilmMaker.Services.Interface
{
    public interface IMediaService
    {
        Task<ApiResponse<List<MediaResponseDto>>> UploadMedia(List<IFormFile> files,int currentUserId);
        Task<ApiResponse<List<Media>>> ValidateMediaOwnership(List<int> mediaIds,int currentUserId);
        Task<ApiResponse<bool>> DeleteMedia(int mediaId,int currentUserId);
    }
}
