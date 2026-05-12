namespace FilmMaker.DTO.Profile.Response
{
    public class LocationManagerProfileResponseDto
    {
        public int ProfileId { get; set; }

        public int UserId { get; set; }

        public string Name { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string? IBAN { get; set; }

        public int? YearsOfExperience { get; set; }

        public string? Description { get; set; }

        public decimal? CommissionRate { get; set; }

        public int Rate { get; set; }

        public bool IsProfileCompleted { get; set; }

        public List<string> Cities { get; set; } = new();

        public List<string> PreviousProjects { get; set; } = new();
    }
}
