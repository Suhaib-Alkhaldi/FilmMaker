namespace FilmMaker.DTO.Booking
{
    public class RespondBookingRequestDto
    {
        public int BookingRequestId { get; set; }
        public bool IsAccepted { get; set; }
        public string? ResponseMessage { get; set; }
    }
}
