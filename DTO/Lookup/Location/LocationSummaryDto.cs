namespace FilmMaker.DTO.Lookup.Location
{
    public class LocationSummaryDto
    {
        public int Id { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string? Address { get; set; }
        public decimal DailyPrice { get; set; }
        public string LocationStatus { get; set; } = string.Empty;
        public string? MediaUrl { get; set; }
    }
}