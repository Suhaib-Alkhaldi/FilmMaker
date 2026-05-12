namespace FilmMaker.DTO.Profile.Request
{
    public class UpdateProductionCompanyProfileRequestDto
    {
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? IBAN { get; set; }
        public string? Country { get; set; }
        public string? City { get; set; }
        public string? Bio { get; set; }
        public List<int>? ProductionTypeIds { get; set; }
    }
}
