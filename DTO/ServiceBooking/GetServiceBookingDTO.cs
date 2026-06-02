namespace FilmMaker.DTO.ServiceBooking
{
    public class GetServiceBookingDTO
    {
        // ── Booking Info ──────────────────────────────────────────────────────────
        public int Id { get; set; }

        public string? Notes { get; set; }

        public DateTime BookingStartDate { get; set; }

        public DateTime BookingEndDate { get; set; }

        public int TotalDays { get; set; }

        public decimal TotalPrice { get; set; }

        public DateTime CreatedAt { get; set; }

        public int StatusId { get; set; }

        public string StatusName { get; set; } = string.Empty;

        public int ServiceId { get; set; }

        public string ServiceName { get; set; } = string.Empty;

        public decimal ServiceDailyPrice { get; set; }

        public int ServiceTypeId { get; set; }

        public string ServiceTypeName { get; set; } = string.Empty;

        public int ServiceProviderId { get; set; }

        public string ServiceProviderName { get; set; } = string.Empty;

        public int RequesterId { get; set; }

        public string RequesterName { get; set; } = string.Empty;

        public int? LocationId { get; set; }

        public string? LocationName { get; set; }

        public string? LocationCity { get; set; }

        public string? LocationAddress { get; set; }
        public decimal? Latitude { get; set; }
        public decimal? Longitude { get; set; }

        public string? LocationOnGoogleMaps { get; set; }


    }
}
