namespace FilmMaker.DTO.Profile.Request
{
    public class CompleteLocationManagerProfileRequestDto
    {
        public int? YearsOfExperience { get; set; }

        public string? Description { get; set; }

        public decimal? CommissionRate { get; set; }

        public List<int> CityIds { get; set; } = new();

        public List<string> PreviousProjects { get; set; } = new();
    }
}
