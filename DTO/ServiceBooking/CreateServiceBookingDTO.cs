namespace FilmMaker.DTO.ServiceBooking
{
    public class CreateServiceBookingDTO
    {
        public int ServiceId { get; set; }

        public int? LocationId { get; set; }

        public string? Notes { get; set; }

        public DateTime BookingStartDate { get; set; }

        public DateTime BookingEndDate { get; set; }

    }
}
