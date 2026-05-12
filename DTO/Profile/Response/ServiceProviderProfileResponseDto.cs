namespace FilmMaker.DTO.Profile.Response
{
    public class ServiceProviderProfileResponseDto
    {
        public int ProfileId { get; set; }
        public int UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string? IBAN { get; set; }
        public List<string> ServiceTypes { get; set; } = new();
        public List<string> CustomServiceTypes { get; set; } = new();
    }
}
