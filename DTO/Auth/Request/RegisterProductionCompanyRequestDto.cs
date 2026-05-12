namespace FilmMaker.DTO.Auth.Request
{
    public class RegisterProductionCompanyRequestDto:BaseRegisterDto
    {

        public string Country { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string? Bio { get; set; }

        public List<int>? ProductionTypeId { get; set; }
    }
}
