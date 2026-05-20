using FilmMaker.Entities;

namespace FilmMaker.Services.Interface
{
    public interface IEmailService
    {
        Task SendOtpAsync(string toEmail, string otp, OtpPurpose purpose);
    }
}
