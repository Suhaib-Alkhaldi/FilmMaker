namespace FilmMaker.DTO.Location.Request
{
    public class UpdateLocationRequestDto
    {
        public int locationId {  get; set; }
        public string? LocationName { get; set; }
        public string? LocationDescription { get; set; }
        public string? Country { get; set; } 
        public decimal? HourlyPrice { get; set; }
        public string? FacilitiesDescription { get; set; }
        public int? LocationTypeId { get; set; }
        public string? City { get; set; }
        public string? Address { get; set; }
        public decimal? DailyPrice { get; set; }
        public string? LocationOnGoogleMaps { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }
        public List<string>? TermsOfUse { get; set; }
        public List<int>? MediaIds { get; set; }
    }
}
