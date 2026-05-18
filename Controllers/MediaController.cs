using FilmMaker.Attribute;
using FilmMaker.Common;
using FilmMaker.DTO.Media;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MediaController : ControllerBase
    {
        private readonly IMediaService _mediaService;

        public MediaController(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }


        [AuthorizeLocationOwner]
        [HttpPost("UploadMedia")]
        public async Task<ActionResult<ApiResponse<List<MediaResponseDto>>>> UploadMedia([FromForm] List<IFormFile> files)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _mediaService.UploadMedia(files, currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        [AuthorizeLocationOwner]
        [HttpDelete("DeleteMedia/{mediaId}")]
        public async Task<ActionResult<ApiResponse<bool>>> DeleteMedia(int mediaId)
        {
            var currentUserId = GetCurrentUserId();

            if (currentUserId == null)
                return Unauthorized();

            var result = await _mediaService.DeleteMedia(mediaId, currentUserId.Value);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim))
            {
                return null;
            }

            if (!int.TryParse(userIdClaim, out var userId))
            {
                return null;
            }

            return userId;
        }
    }
}

