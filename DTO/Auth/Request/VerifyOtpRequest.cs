namespace FilmMaker.DTO.Auth.Request
{
    public class VerifyOtpRequest
    {
        public string Code { get;set; } 

        public string Email { get; set; }
    }
}
