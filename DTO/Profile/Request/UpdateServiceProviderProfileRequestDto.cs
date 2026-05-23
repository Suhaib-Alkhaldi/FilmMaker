namespace FilmMaker.DTO.Profile.Request
{
    public class UpdateServiceProviderProfileRequestDto
    {
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public string? IBAN { get; set; }

        public List<int>? ServiceTypeIds { get; set; }

        public List<string>? CustomServiceTypes { get; set; }

        public List<int>? CitiesIds { get; set; }
    }
}
