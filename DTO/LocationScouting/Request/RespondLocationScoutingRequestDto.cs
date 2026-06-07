namespace FilmMaker.DTO.LocationScouting.Request
{
    public class RespondLocationScoutingRequestDto
    {
        public int RequestId { get; set; }
        public bool IsAccepted { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
