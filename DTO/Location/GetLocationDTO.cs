using FilmMaker.Entities;
using System.ComponentModel.DataAnnotations.Schema;

namespace FilmMaker.DTO.Location
{
    public class GetLocationDTO
    {
        public string LocationName { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
        public int LocationOwnerId { get; set; }
        public string LocationOwnerName { get; set; }
        public string LocationManagerName { get; set; }
        public bool IsActive { get; set; }

        public string StatusName { get; set; }

        public string LocationStatusName { get; set; }
        public string LocationOnGoogleMaps { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<string>? TermsOfUse { get; set; }
    }
}
