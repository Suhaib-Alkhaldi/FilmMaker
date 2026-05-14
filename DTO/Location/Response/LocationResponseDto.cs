using FilmMaker.DTO.Media;

namespace FilmMaker.DTO.Location.Response
{
    public class LocationResponseDto
    {
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;

        public string LocationDescription { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;

        public decimal? HourlyPrice { get; set; }

        public string? FacilitiesDescription { get; set; }

        public int FacilitiesCount { get; set; }

        public string City { get; set; } = string.Empty;

        public string? Address { get; set; }

        public decimal DailyPrice { get; set; }

        public string LocationStatus { get; set; } = string.Empty;
        public int LocationTypeId { get; set; }
        public string LocationType { get; set; } = string.Empty;

        public string? LocationOnGoogleMaps { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public string? LocationOwnerName { get; set; }

        public string? LocationManagerName { get; set; }

        public List<string> TermsOfUse { get; set; } = new();

        public List<MediaResponseDto> Media { get; set; } = new();

        public DateTime CreatedAt { get; set; }
    }
}
