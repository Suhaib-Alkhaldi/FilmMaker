namespace FilmMaker.DTO.Auth.Response
{
    public class LoginResponseDTO
    {
        public string AccessToken { get; set; }

        public string RefreshToken { get; set; }
        public DateTime Expiration { get; set; }
    }
}
