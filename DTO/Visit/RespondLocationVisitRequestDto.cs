namespace FilmMaker.DTO.Visit
{
    public class RespondLocationVisitRequestDto
    {
        public int VisitRequestId { get; set; } 
        public bool IsAccepted { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
