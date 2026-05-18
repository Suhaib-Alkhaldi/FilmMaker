using FilmMaker.Common;
using FilmMaker.DTO.Media;
using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;

namespace FilmMaker.Services.Service
{
    public class MediaService : IMediaService
    {
        private readonly FilmMakerDbContext _context;
        private readonly ILogger<MediaService> _logger;
        private readonly IWebHostEnvironment _environment;

        public MediaService(FilmMakerDbContext context, ILogger<MediaService> logger , IWebHostEnvironment environment)
        {
            _context = context;
            _logger = logger;
            _environment = environment;
        }

        private const long MaxImageSizeInBytes = 5 * 1024 * 1024; // 5 MB
        private const long MaxVideoSizeInBytes = 100 * 1024 * 1024; // 100 MB

        private static readonly string[] AllowedImageContentTypes =
        {
            "image/jpeg",
            "image/png",
            "image/webp"
        };

        private static readonly string[] AllowedVideoContentTypes =
        {
            "video/mp4",
            "video/quicktime",
            "video/webm"
        };


        public async Task<ApiResponse<List<MediaResponseDto>>> UploadMedia(List<IFormFile> files, int currentUserId)
        {
            if (files == null || !files.Any())
            {
                return ApiResponse<List<MediaResponseDto>>.FailureResponse(
                    "At least one file is required.",
                    "يجب رفع ملف واحد على الأقل."
                );
            }

            var response = new List<MediaResponseDto>();

            try
            {
                var uploadFolderPath = Path.Combine(
                    _environment.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "uploads",
                    "media"
                );

                if (!Directory.Exists(uploadFolderPath))
                    Directory.CreateDirectory(uploadFolderPath);

                foreach (var file in files)
                {
                    var validationError = ValidateFile(file);
                    if (validationError != null)
                        return validationError;

                    var mediaTypeName = GetMediaTypeName(file.ContentType);

                    var mediaTypeId = await GetMediaTypeId(mediaTypeName);
                    if (mediaTypeId == null)
                    {
                        return ApiResponse<List<MediaResponseDto>>.FailureResponse(
                            $"{mediaTypeName} MediaType was not found in lookup data.",
                            $"نوع الملف {mediaTypeName} غير موجود في بيانات النظام."
                        );
                    }

                    var originalFileName = Path.GetFileName(file.FileName);
                    var fileExtension = Path.GetExtension(originalFileName);
                    var storedFileName = $"{Guid.NewGuid():N}{fileExtension}";

                    var fullFilePath = Path.Combine(uploadFolderPath, storedFileName);

                    await using (var stream = new FileStream(fullFilePath, FileMode.Create))
                    {
                        await file.CopyToAsync(stream);
                    }

                    var fileUrl = $"/uploads/media/{storedFileName}";

                    var media = new Media
                    {
                        UploadedByUserId = currentUserId,
                        FileName = storedFileName,
                        OriginalFileName = originalFileName,
                        FileUrl = fileUrl,
                        ContentType = file.ContentType,
                        SizeInBytes = file.Length,
                        MediaTypeId = mediaTypeId.Value,
                        IsActive = true,
                        IsDeleted = false,
                        CreatedBy = currentUserId.ToString(),
                        CreatedAt = DateTime.UtcNow
                    };

                    _context.Media.Add(media);
                    await _context.SaveChangesAsync();

                    response.Add(new MediaResponseDto
                    {
                        MediaId = media.Id,
                        FileName = media.FileName,
                        OriginalFileName = media.OriginalFileName,
                        FileUrl = media.FileUrl,
                        ContentType = media.ContentType,
                        SizeInBytes = media.SizeInBytes,
                        MediaType = mediaTypeName
                    });
                }

                return ApiResponse<List<MediaResponseDto>>.SuccessResponse(
                    response,
                    "Media uploaded successfully.",
                    "تم رفع الملفات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while uploading media. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<List<MediaResponseDto>>.FailureResponse(
                    "An unexpected error occurred while uploading media.",
                    "حدث خطأ غير متوقع أثناء رفع الملفات."
                );
            }
        }

        public async Task<ApiResponse<List<Media>>> ValidateMediaOwnership(List<int> mediaIds, int currentUserId)
        {
            if (mediaIds == null || !mediaIds.Any())
            {
                return ApiResponse<List<Media>>.FailureResponse(
                    "Media files are required.",
                    "الملفات مطلوبة."
                );
            }

            var distinctMediaIds = mediaIds
                .Distinct()
                .ToList();

            try
            {
                var mediaItems = await _context.Media
                    .Include(x => x.MediaType)
                    .Where(x =>
                        distinctMediaIds.Contains(x.Id) &&
                        x.UploadedByUserId == currentUserId &&
                        !x.IsDeleted)
                    .ToListAsync();

                if (mediaItems.Count != distinctMediaIds.Count)
                {
                    _logger.LogWarning(
                        "Media ownership validation failed. UserId: {UserId}, MediaIds: {MediaIds}",
                        currentUserId,
                        string.Join(",", distinctMediaIds)
                    );

                    return ApiResponse<List<Media>>.FailureResponse(
                        "One or more media files are invalid or do not belong to the current user.",
                        "واحد أو أكثر من الملفات غير صحيح أو لا يخص المستخدم الحالي."
                    );
                }

                return ApiResponse<List<Media>>.SuccessResponse(
                    mediaItems,
                    "Media files validated successfully.",
                    "تم التحقق من الملفات بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while validating media ownership. UserId: {UserId}",
                    currentUserId
                );

                return ApiResponse<List<Media>>.FailureResponse(
                    "An unexpected error occurred while validating media files.",
                    "حدث خطأ غير متوقع أثناء التحقق من الملفات."
                );
            }
        }
        public async Task<ApiResponse<bool>> DeleteMedia(int mediaId, int currentUserId)
        {
            try
            {
                var media = await _context.Media.Where(x =>
                        x.Id == mediaId &&
                        x.UploadedByUserId == currentUserId &&
                        !x.IsDeleted)
                    .FirstOrDefaultAsync();

                if (media == null)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "Media file was not found.",
                        "لم يتم العثور على الملف."
                    );
                }

                var isLinkedToLocation = await _context.LocationMedia
                    .AnyAsync(x =>x.MediaId == mediaId &&!x.IsDeleted);

                if (isLinkedToLocation)
                {
                    return ApiResponse<bool>.FailureResponse(
                        "This media file is already linked and cannot be deleted from here.",
                        "هذا الملف مرتبط بالفعل ولا يمكن حذفه من هنا."
                    );
                }

                media.IsDeleted = true;
                media.IsActive = false;
                media.UpdatedBy = currentUserId.ToString();
                media.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return ApiResponse<bool>.SuccessResponse(
                    true,
                    "Media file deleted successfully.",
                    "تم حذف الملف بنجاح."
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error while deleting unlinked media. MediaId: {MediaId}, UserId: {UserId}",
                    mediaId,
                    currentUserId
                );

                return ApiResponse<bool>.FailureResponse(
                    "An unexpected error occurred while deleting the media file.",
                    "حدث خطأ غير متوقع أثناء حذف الملف."
                );
            }
        }




        private ApiResponse<List<MediaResponseDto>>? ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return ApiResponse<List<MediaResponseDto>>.FailureResponse(
                    "File cannot be empty.",
                    "لا يمكن أن يكون الملف فارغًا."
                );
            }

            var isImage = AllowedImageContentTypes.Contains(file.ContentType);
            var isVideo = AllowedVideoContentTypes.Contains(file.ContentType);

            if (!isImage && !isVideo)
            {
                return ApiResponse<List<MediaResponseDto>>.FailureResponse(
                    "Only image and video files are allowed.",
                    "يسمح فقط برفع الصور والفيديوهات."
                );
            }

            if (isImage && file.Length > MaxImageSizeInBytes)
            {
                return ApiResponse<List<MediaResponseDto>>.FailureResponse(
                    "Image file size cannot exceed 5 MB.",
                    "حجم الصورة لا يمكن أن يتجاوز 5 ميجابايت."
                );
            }

            if (isVideo && file.Length > MaxVideoSizeInBytes)
            {
                return ApiResponse<List<MediaResponseDto>>.FailureResponse(
                    "Video file size cannot exceed 100 MB.",
                    "حجم الفيديو لا يمكن أن يتجاوز 100 ميجابايت."
                );
            }

            return null;
        }

        private static string GetMediaTypeName(string contentType)
        {
            if (AllowedImageContentTypes.Contains(contentType))
                return "Image";

            if (AllowedVideoContentTypes.Contains(contentType))
                return "Video";

            return string.Empty;
        }

        private async Task<int?> GetMediaTypeId(string mediaTypeName)
        {
            return await _context.LookupItems
                .Where(x =>
                    x.Name == mediaTypeName &&
                    x.LookupCategory.Name == "MediaType" &&
                    !x.IsDeleted)
                .Select(x => (int?)x.Id)
                .FirstOrDefaultAsync();
        }
    }
}
