using FilmMaker.DTO.TermsOfUse;
using FilmMaker.Entities;

namespace FilmMaker.DTO.Location
{
    public class UpdateLocationDTO
    {
        public int Id { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string LocationDescription { get; set; } = string.Empty;
        public decimal DailyPrice { get; set; }
        public int LocationStatusId { get; set; }
        public string LocationOnGoogleMaps { get; set; } = string.Empty;
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<LocationTermsOfUseDTO>? TermsOfUse { get; set; }

    }
}
