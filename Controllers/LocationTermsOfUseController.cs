using FilmMaker.Attribute;
using FilmMaker.DTO.TermsOfUse;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [AllowAnonymous]
    public class LocationTermsOfUseController : ControllerBase
    {
        private readonly ILocationTermsOfUseService _locationTermsService;

        public LocationTermsOfUseController(ILocationTermsOfUseService locationTermsService)
        {
            _locationTermsService = locationTermsService;
        }

        [AuthorizeLocationOwner]
        [HttpPost("create-many")]
        public async Task<IActionResult> CreateManyTermsOfUse(
        [FromBody] List<LocationTermsOfUseDTO> terms, [FromQuery] int currentUserId)
        {
            var result = await _locationTermsService.CreateManyTermsOfUseAsync(terms, currentUserId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [AuthorizeLocationOwner]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateTermsOfUse(
        [FromBody] LocationTermsOfUseDTO term, [FromQuery] int currentUserId)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var result = await _locationTermsService.UpdateTermsOfUseAsync(term, currentUserId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [AuthorizeLocationOwner]
        [HttpPut("update-many")]
        public async Task<IActionResult> UpdateManyTermsOfUse(
            [FromBody] List<LocationTermsOfUseDTO> terms, [FromQuery] int currentUserId)
        {    
            var result = await _locationTermsService.UpdateManyTermsOfUseAsync(terms, currentUserId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [AuthorizeLocationOwner]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTermsOfUse(
           [FromBody] LocationTermsOfUseDTO term, [FromQuery] int currentUserId)
        {
 

            var result = await _locationTermsService.CreateTermsOfUseAsync(term, currentUserId);

            return result.Success ? Ok(result) : BadRequest(result);
        }

        [HttpGet("location/{locationId}")]
        public async Task<IActionResult> GetAllTermsOfUseByLocation(int locationId)
        {
            var result = await _locationTermsService.AllTermsOfUseByLocationAsync(locationId);

            return result.Success ? Ok(result) : NotFound(result);
        }

        [AuthorizeLocationOwner]
        [HttpDelete("delete")]
           public async Task<IActionResult> DeleteTermsOfUse(
          [FromQuery] int currentUserId,
          [FromQuery] int locationId,
          [FromQuery] int termId)
           {
               var result = await _locationTermsService.DeleteTermsOfUseAsync(locationId, currentUserId, termId);

               return result.Success ? Ok(result) : NotFound(result);
           }
    }
}
