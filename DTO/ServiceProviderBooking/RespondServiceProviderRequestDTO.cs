namespace FilmMaker.DTO.ServiceProviderBooking
{
    public class RespondServiceProviderRequestDTO
    {
        public int RequestId { get; set; }

        public bool IsAccepted { get; set; }

        public string? ResponseMessage { get; set; }
    }
}
