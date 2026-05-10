using FilmMaker.DTOs.Media;

namespace FilmMaker.Services.Media
{
    public interface IMediaService
    {
       
        Task<(bool Success, string? Error, List<MediaUploadResultDto> Results)> UploadLocationMediaAsync(
            int locationId,
            int uploadedByUserId,
            List<IFormFile> files);

        
        // Task<(bool Success, string? Error, List<MediaUploadResultDto> Results)> UploadServiceMediaAsync(
        //     int serviceId,
        //     int uploadedByUserId,
        //     List<IFormFile> files);

        
        Task<(bool Success, string? Error)> DeleteLocationMediaAsync(int mediaId);

       
        Task<(bool Success, string? Error)> DeleteServiceMediaAsync(int mediaId);
    }
}