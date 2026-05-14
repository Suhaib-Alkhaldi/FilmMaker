namespace FilmMaker.DTO.Booking
{
    public class BookingRequestResponseDto
    {
        public int BookingRequestId { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public int LocationOwnerId { get; set; }
        public string LocationOwnerName { get; set; } = string.Empty;
        public int? ProductionCompanyId { get; set; }
        public string? ProductionCompanyName { get; set; }
        public int? LocationManagerId { get; set; }
        public string? LocationManagerName { get; set; }
        public string RequestedByType { get; set; } = string.Empty;
        public string RequestedByName { get; set; } = string.Empty;
        public int BookingStatusId { get; set; }
        public string BookingStatus { get; set; } = string.Empty;
        public DateTime ShootingDate { get; set; }
        public string RequestDetails { get; set; } = string.Empty;
        public decimal TotalPrice { get; set; }
        public DateTime RequestedAtUtc { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
