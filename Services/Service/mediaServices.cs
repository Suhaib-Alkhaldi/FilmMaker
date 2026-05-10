using FilmMaker.DTOs.Media;
using FilmMaker.Entities;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Media
{
    public class MediaService : IMediaService
    {
        private const long MaxImageSizeBytes = 5 * 1024 * 1024;   // 5 MB
        private const long MaxVideoSizeBytes = 200 * 1024 * 1024;  // 200 MB

        private static readonly HashSet<string> AllowedImageTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "image/jpeg", "image/png", "image/webp", "image/gif"
        };

        private static readonly HashSet<string> AllowedVideoTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            "video/mp4", "video/quicktime", "video/x-msvideo", "video/webm"
        };

        private readonly FilmMakerDbContext _context;
        private readonly IWebHostEnvironment _env;

        public MediaService(FilmMakerDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        

        public async Task<(bool Success, string? Error, List<MediaUploadResultDto> Results)>
            UploadLocationMediaAsync(int locationId, int uploadedByUserId, List<IFormFile> files)
        {
            var locationExists = await _context.Locations.AnyAsync(l => l.Id == locationId);
            if (!locationExists)
                return (false, $"Location with ID {locationId} was not found.", new());

            var (success, error, savedFiles) = await ValidateAndSaveFilesAsync(files, "Locations");
            if (!success)
                return (false, error, new());

            var results = new List<MediaUploadResultDto>();
            foreach (var saved in savedFiles)
            {
                var mediaTypeName = saved.IsVideo ? "Video" : "Photo";
                var mediaType = await _context.LockupItems
                    .FirstOrDefaultAsync(l => l.Name == mediaTypeName);

                if (mediaType is null)
                    return (false, $"LookupItem '{mediaTypeName}' was not found. Make sure it is seeded in the database.", new());

                var entity = new LocationMedia
                {
                    LocationId       = locationId,
                    UploadedByUserId = uploadedByUserId,
                    FileName         = saved.OriginalFileName,
                    FileUrl          = saved.RelativePath,
                    ContentType      = saved.ContentType,
                    SizeInBytes      = saved.SizeInBytes,
                    MediaTypeId      = mediaType.Id
                };

                _context.LocationMedia.Add(entity);
                await _context.SaveChangesAsync();

                results.Add(new MediaUploadResultDto
                {
                    Id          = entity.Id,
                    FileName    = entity.FileName,
                    FileUrl     = entity.FileUrl,
                    ContentType = entity.ContentType,
                    SizeInBytes = entity.SizeInBytes
                });
            }

            return (true, null, results);
        }

        public async Task<(bool Success, string? Error)> DeleteLocationMediaAsync(int mediaId)
        {
            var media = await _context.LocationMedia.FindAsync(mediaId);
            if (media is null)
                return (false, "Media record not found.");

            DeleteFileFromDisk(media.FileUrl);
            _context.LocationMedia.Remove(media);
            await _context.SaveChangesAsync();

            return (true, null);
        }

        
        //     //you need to add the services entity to add this here 

        // public async Task<(bool Success, string? Error, List<MediaUploadResultDto> Results)>
        //     UploadServiceMediaAsync(int serviceId, int uploadedByUserId, List<IFormFile> files)
        // {
        //     var serviceExists = await _context.ser.AnyAsync(s => s.Id == serviceId);
        //     if (!serviceExists)
        //         return (false, $"Service with ID {serviceId} was not found.", new());
        //
        //     var (success, error, savedFiles) = await ValidateAndSaveFilesAsync(files, "Services");
        //     if (!success)
        //         return (false, error, new());
        //
        //     var results = new List<MediaUploadResultDto>();
        //     foreach (var saved in savedFiles)
        //     {
        //         var mediaTypeName = saved.IsVideo ? "Video" : "Photo";
        //         var mediaType = await _context.LookupItems
        //             .FirstOrDefaultAsync(l => l.Name == mediaTypeName);
        //
        //         if (mediaType is null)
        //             return (false, $"LookupItem '{mediaTypeName}' was not found. Make sure it is seeded in the database.", new());
        //
        //         var entity = new ServiceMedia
        //         {
        //             ServiceId        = serviceId,
        //             UploadedByUserId = uploadedByUserId,
        //             FileName         = saved.OriginalFileName,
        //             FileUrl          = saved.RelativePath,
        //             ContentType      = saved.ContentType,
        //             SizeInBytes      = saved.SizeInBytes,
        //             MediaTypeId      = mediaType.Id
        //         };
        //
        //         _context.ServiceMedia.Add(entity);
        //         await _context.SaveChangesAsync();
        //
        //         results.Add(new MediaUploadResultDto
        //         {
        //             Id          = entity.Id,
        //             FileName    = entity.FileName,
        //             FileUrl     = entity.FileUrl,
        //             ContentType = entity.ContentType,
        //             SizeInBytes = entity.SizeInBytes
        //         });
        //     }
        //
        //     return (true, null, results);
        // }

        public async Task<(bool Success, string? Error)> DeleteServiceMediaAsync(int mediaId)
        {
            var media = await _context.ServiceMedias.FindAsync(mediaId);
            if (media is null)
                return (false, "Media record not found.");

            DeleteFileFromDisk(media.FileUrl);
            _context.ServiceMedias.Remove(media);
            await _context.SaveChangesAsync();

            return (true, null);
        }

       

        private async Task<(bool Success, string? Error, List<SavedFileInfo> Files)>
            ValidateAndSaveFilesAsync(List<IFormFile> files, string subFolder)
        {
            if (files == null || files.Count == 0)
                return (false, "No files were provided.", new());

            var folder = Path.Combine(_env.ContentRootPath, "Uploads", subFolder);
            if (!Directory.Exists(folder))
                Directory.CreateDirectory(folder);

            var saved = new List<SavedFileInfo>();

            foreach (var file in files)
            {
                bool isImage = AllowedImageTypes.Contains(file.ContentType);
                bool isVideo = AllowedVideoTypes.Contains(file.ContentType);

                if (!isImage && !isVideo)
                    return (false,
                        $"File '{file.FileName}' has an unsupported type ({file.ContentType}). " +
                        "Only images (JPEG, PNG, WEBP, GIF) and videos (MP4, MOV, AVI, WEBM) are allowed.",
                        new());

                long maxSize = isImage ? MaxImageSizeBytes : MaxVideoSizeBytes;
                if (file.Length > maxSize)
                {
                    string limit = isImage ? "5 MB" : "200 MB";
                    return (false,
                        $"File '{file.FileName}' exceeds the maximum allowed size of {limit}.",
                        new());
                }

                var uniqueName   = Guid.NewGuid() + Path.GetExtension(file.FileName);
                var fullPath     = Path.Combine(folder, uniqueName);
                var relativePath = $"Uploads/{subFolder}/{uniqueName}";

                await using var stream = new FileStream(fullPath, FileMode.Create);
                await file.CopyToAsync(stream);

                saved.Add(new SavedFileInfo
                {
                    OriginalFileName = file.FileName,
                    RelativePath     = relativePath,
                    ContentType      = file.ContentType,
                    SizeInBytes      = file.Length,
                    IsVideo          = isVideo
                });
            }

            return (true, null, saved);
        }

        private void DeleteFileFromDisk(string relativeUrl)
        {
            var fullPath = Path.Combine(_env.ContentRootPath, relativeUrl.Replace('/', Path.DirectorySeparatorChar));
            if (File.Exists(fullPath))
                File.Delete(fullPath);
        }

        private record SavedFileInfo
        {
            public string OriginalFileName { get; init; } = string.Empty;
            public string RelativePath     { get; init; } = string.Empty;
            public string ContentType      { get; init; } = string.Empty;
            public long   SizeInBytes      { get; init; }
            public bool   IsVideo          { get; init; }  // ← drives auto mediaType lookup
        }
    }
}