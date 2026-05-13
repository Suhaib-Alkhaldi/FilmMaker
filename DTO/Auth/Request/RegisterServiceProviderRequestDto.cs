namespace FilmMaker.DTO.Auth.Request
{
    public class RegisterServiceProviderRequestDto:BaseRegisterDto
    {
        public List<int> ServiceTypeIds { get; set; } 

        public List<int> CitiesIds { get; set; }
        public List<string> CustomServiceTypes { get; set; } 
    }
}
