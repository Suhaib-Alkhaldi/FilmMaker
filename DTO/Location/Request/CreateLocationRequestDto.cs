namespace FilmMaker.DTO.Location.Request
{
    public class CreateLocationRequestDto
    {
        public string LocationName { get; set; } = string.Empty;

        public string LocationDescription { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public int LocationTypeId { get; set; }

        public string? Address { get; set; }

        public decimal DailyPrice { get; set; }
        public string Country { get; set; } = string.Empty;

        public decimal? HourlyPrice { get; set; }

        public string? FacilitiesDescription { get; set; }
        public string? LocationOnGoogleMaps { get; set; }

        public decimal? Latitude { get; set; }

        public decimal? Longitude { get; set; }

        public List<string> TermsOfUse { get; set; } = new();

        public List<int> MediaIds { get; set; } = new();
    }
}
