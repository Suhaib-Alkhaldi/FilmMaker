namespace FilmMaker.DTO.LocationVisit
{
    public class UpdateVisitRequestDto
    {
        public DateTime? RequestedVisitDateUtc { get; set; }
        public string? RequestMessage { get; set; }
    }
}