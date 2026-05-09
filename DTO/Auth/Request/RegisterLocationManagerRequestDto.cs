namespace FilmMaker.DTO.Auth.Request
{
    public class RegisterLocationManagerRequestDto : BaseRegisterDto
    {
        public int? YearsOfExperience { get; set; }
        public List<string>? PreviousProjects { get; set; }
        public List<int>? Cities { get; set; }
        public string? Description { get; set; }
        public decimal? CommissionRate { get; set; }
    }
}
