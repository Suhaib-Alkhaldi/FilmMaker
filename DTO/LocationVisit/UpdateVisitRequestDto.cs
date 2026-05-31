namespace FilmMaker.DTO.LocationVisit
{
    public class UpdateVisitRequestDto
    {
        public int RequestId { get; set; }
        public DateTime? RequestedVisitDateUtc { get; set; }
        public string? RequestMessage { get; set; }
    }
}