namespace FilmMaker.DTO.Visit
{
    public class LocationVisitRequestResponseDto
    {
        public int VisitRequestId { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int LocationOwnerId { get; set; }
        public string LocationOwnerName { get; set; } = string.Empty;
        public int LocationManagerId { get; set; }
        public string LocationManagerName { get; set; } = string.Empty;
        public DateTime RequestedVisitDateUtc { get; set; }
        public string? RequestMessage { get; set; }
        public int VisitStatusId { get; set; }
        public string VisitStatus { get; set; } = string.Empty;
        public string? OwnerResponseMessage { get; set; }
        public DateTime? RespondedAtUtc { get; set; }
        public int? RespondedByUserId { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
