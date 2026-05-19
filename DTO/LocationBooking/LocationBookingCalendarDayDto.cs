namespace FilmMaker.DTO.LocationBooking
{
    public class LocationBookingCalendarDayDto
    {
        public DateTime Date { get; set; }

        public bool IsAvailable { get; set; }

        public bool HasPendingRequest { get; set; }

        public bool IsBooked { get; set; }

        public bool CanRequest { get; set; }

        public int PendingCount { get; set; }

        public int BookedCount { get; set; }

        public string Status { get; set; } = string.Empty;
    }
}
