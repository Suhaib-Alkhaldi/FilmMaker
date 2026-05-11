namespace FilmMaker.DTO.Profile.Request
{
    public class UpdateLocationManagerProfileRequestDto
    {
        public string? Name { get; set; }

        public string? PhoneNumber { get; set; }

        public string? IBAN { get; set; }

        public int? YearsOfExperience { get; set; }

        public string? Description { get; set; }

        public decimal? CommissionRate { get; set; }

        public List<int> CityId { get; set; } = new();

        public List<string> PreviousProjects { get; set; }
    }
}
