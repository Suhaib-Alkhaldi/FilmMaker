namespace FilmMaker.DTO.Profile.Response
{
    public class ProductionCompanyProfileResponseDto
    {
        public int ProfileId { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? IBAN { get; set; }

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public List<string> ProductionTypes { get; set; }
    }
}
