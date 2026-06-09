namespace FilmMaker.DTO.RequestToLocationManagerToBookService
{
    public class RespondRequestToLocationManagerToBookServiceDTO
    {
        public int RequestId { get; set; }

        public bool IsAccepted { get; set; }

        public string? ResponseMessage { get; set; }
    }
}
