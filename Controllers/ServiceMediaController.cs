// using FilmMaker.DTOs.Media;
// using FilmMaker.Services.Media;
// using Microsoft.AspNetCore.Authorization;
// using Microsoft.AspNetCore.Mvc;
//
// namespace FilmMaker.Controllers
// {
//     [ApiController]
//     [Route("api/[controller]")]
//     [Authorize]
//     public class ServiceMediaController : ControllerBase
//     {
//         private readonly IMediaService _mediaService;
//
//         public ServiceMediaController(IMediaService mediaService)
//         {
//             _mediaService = mediaService;
//         }
//
//         [HttpPost("{serviceId:int}/upload")]
//         [Consumes("multipart/form-data")]
//         public async Task<IActionResult> Upload(int serviceId, [FromForm] UploadServiceMediaDto dto)
//         {
//             var userIdClaim = User.FindFirst("userId")?.Value
//                            ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
//
//             if (userIdClaim is null || !int.TryParse(userIdClaim, out int userId))
//                 return Unauthorized("User identity could not be determined.");
//
//             var (success, error, results) = await _mediaService.UploadServiceMediaAsync(
//                 serviceId,
//                 userId,
//                 dto.Files);
//
//             if (!success)
//                 return BadRequest(new { message = error });
//
//             return Ok(new
//             {
//                 message = $"{results.Count} file(s) uploaded successfully.",
//                 files   = results
//             });
//         }
//         
//         [HttpDelete("{mediaId:int}")]
//         public async Task<IActionResult> Delete(int mediaId)
//         {
//             var (success, error) = await _mediaService.DeleteServiceMediaAsync(mediaId);
//
//             if (!success)
//                 return NotFound(new { message = error });
//
//             return Ok(new { message = "Media deleted successfully." });
//         }
//     }
// }