namespace FilmMaker.DTO.ServiceBooking
{
    public class CreateServiceBookingDTO
    {
        public int ServiceId { get; set; }

        public int? LocationId { get; set; }

        public string? Notes { get; set; }

        public DateTime BookingStartDate { get; set; }

        public DateTime BookingEndDate { get; set; }

        public decimal Latitude { get; set; }
        public decimal Longitude { get; set; }

        public string? LocationOnGoogleMaps { get; set; }


    }
}
