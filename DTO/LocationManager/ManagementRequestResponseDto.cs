namespace FilmMaker.DTO.LocationManager
{
    public class ManagementRequestResponseDto
    {
        public int Id { get; set; }
        public int LocationId { get; set; }
        public string LocationName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
