using FilmMaker.Services.Interface;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace FilmMaker.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LookupsController : ControllerBase
    {
        private readonly ILookupService _lookupService;

        public LookupsController(ILookupService lookupService)
        {
            _lookupService = lookupService;
        }

        [HttpGet("GetLookupByCategory")]
        public async Task<IActionResult> GetLookupByCategory(string category)
        {
            var result = await _lookupService.GetLookupByCategory(category);
            return Ok(result);
        }
    }
}
