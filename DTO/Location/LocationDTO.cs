using FilmMaker.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.DTO.Location
{
    public class LocationDTO
    {
        public string LocationName { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
        public int LocationStatusId { get; set; }
        public string LocationOnGoogleMaps { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<string>? TermsOfUse { get; set; }

    }
}
