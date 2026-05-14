using FilmMaker.Common;
using FilmMaker.DTO.Lookup.Location;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupController : ControllerBase
    {
        private readonly ILookupService _locationLookupService;

        public LookupController(ILookupService locationLookupService)
        {
            _locationLookupService = locationLookupService;
        }

        [HttpGet("locations")]
        public async Task<ActionResult<ApiResponse<LocationListResponseDto>>> GetLocations(LocationFilterDto filter)
        {
            var response = await _locationLookupService.GetLocationsAsync(filter);
            if (!response.Success)
                return BadRequest(response);
            return Ok(response);
        }
    }
}
