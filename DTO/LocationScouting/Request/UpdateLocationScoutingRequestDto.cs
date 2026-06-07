namespace FilmMaker.DTO.LocationScouting.Request
{
    public class UpdateLocationScoutingRequestDto
    {
        public int RequestId { get; set; }
        public int LocationManagerId { get; set; }
        public int? CityId { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Requirements { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
    }
}
