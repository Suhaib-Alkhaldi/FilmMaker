namespace FilmMaker.DTO.LocationVisit
{
    public class VisitRequestResponseDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime RequestedVisitDateUtc { get; set; }
        public string Status { get; set; } = string.Empty; 
        public int LocationOwnerId { get; set; }
        public string LocationOwnerName { get; set; }
        public int LocationManagerId { get; set; }
        public string? RequestMessage { get; set; }
        public string? OwnerResponseMessage { get; set; }
        public DateTime? RespondedAtUtc { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}