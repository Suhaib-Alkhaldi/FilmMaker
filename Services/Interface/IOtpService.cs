using FilmMaker.Entities;

namespace FilmMaker.Services.Interface
{
    public interface IOtpService
    {
        Task<string> GenerateAndSaveOtpAsync(int userId, OtpPurpose purpose);
        Task<OtpCode?> ValidateOtpAsync(int userId, string code, OtpPurpose purpose);

    }
}
