namespace FilmMaker.DTO.LocationScouting.Response
{
    public class LocationScoutingRequestResponseDto
    {
        public int Id { get; set; }
        public int ProductionCompanyId { get; set; }
        public string ProductionCompanyName { get; set; } = string.Empty;
        public int LocationManagerId { get; set; }
        public string LocationManagerName { get; set; } = string.Empty;
        public int? CityId { get; set; }
        public string? CityName { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string Requirements { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public decimal? MinBudget { get; set; }
        public decimal? MaxBudget { get; set; }
        public int StatusId { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public string? LocationManagerResponse { get; set; }
        public DateTime? RespondedAtUtc { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
    }
}
