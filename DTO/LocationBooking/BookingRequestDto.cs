namespace FilmMaker.DTO.LocationManager
{
    public class BookingRequestDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime StartDateTime { get; set; }
        public DateTime EndDateTime { get; set; }
        public bool IsFullDay { get; set; }
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public decimal TotalPrice { get; set; }
        public int LocationOwnerId { get; set; }
        public string LocationOwnerName { get; set; } = null!;
        public int? LocationManagerId { get; set; }
        public string? LocationManagerName { get; set; }
        public int? ProductionCompanyId { get; set; }    
        public string? ProductionCompanyName { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
