using FilmMaker.Attribute;
using FilmMaker.DTO.Location;
using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        [AuthorizeLocationOwner]
        [HttpPost("create")]
        public async Task<IActionResult> CreateLocation([FromBody] LocationDTO location, [FromQuery] int currentUserId)
        {
       

            if (currentUserId <= 0)
            {
                return BadRequest("Invalid user ID.");
            }
            var result = await _locationService.CreateLocation(location, currentUserId);

                return Ok(result);
        }

        [AuthorizeLocationOwner]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateLocation([FromBody] UpdateLocationDTO location, [FromQuery] int currentUserId)
        {

            var result = await _locationService.UpdateLocation(location, currentUserId);

           return Ok(result);
        }

        [AuthorizeLocationOwner]
        [HttpPatch("archive/{locationId}")]
        public async Task<IActionResult> ToggleArchive(int locationId, [FromQuery] int currentUserId)
        {
            var result = await _locationService.ArchiveLocation(locationId,currentUserId);

            return Ok(result);
        }

        [AuthorizeLocationOwner]
        [HttpPatch("restore-archive/{locationId}")]
        public async Task<IActionResult> ToggleUnArchive(int locationId, [FromQuery] int currentUserId)
        {
            var result = await _locationService.RestoreArchivedLocation(locationId, currentUserId);

            return Ok(result);
        }


        [AuthorizeAdmin]
        [HttpGet("all-archived")]
        public async Task<IActionResult> GetAllArchivedLocations()
        {
            var locations = await _locationService.GetAllArchivedLocations();

            return Ok(locations);
        }

        [HttpGet("all-unarchived")]
        public async Task<IActionResult> GetAllUnArchivedLocations()
        {
            var locations = await _locationService.GetAllUnArchivedLocations();
            return Ok(locations);
        }


        [HttpGet("{locationId}")]
        public async Task<IActionResult> GetLocationById(int locationId)
        {
            var location = await _locationService.GetLocationById(locationId);

            return Ok(location);
        }

        [HttpGet("owner/{ownerId}")]
        public async Task<IActionResult> GetLocationsByOwnerId(int ownerId)
        {
            var locations = await _locationService.GetLocationsByOwnerId(ownerId);

            return Ok(locations);
        }

        [Authorize(Roles = "Admin , LocationOwner")]
        [HttpGet("owner/{ownerId}/active")]
        public async Task<IActionResult> GetOwnerActiveLocations(int ownerId)
        {
            var locations = await _locationService.GetOwnerActiveLocations(ownerId);

            return Ok(locations);
        }

        [Authorize(Roles = "Admin , LocationOwner")]
        [HttpGet("owner/{ownerId}/archived")]
        public async Task<IActionResult> GetOwnerArchivedLocations(int ownerId)
        {
            var locations = await _locationService.GetOwnerArchivedLocations(ownerId);

            return Ok(locations);

        }
    }
}