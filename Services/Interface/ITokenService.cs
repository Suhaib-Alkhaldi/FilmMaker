using FilmMaker.Entities;

namespace FilmMaker.Services.Interface
{
    public interface ITokenService
    {
        string GenerateAccessToken(int userId, string firstName, string role);
        string GenerateRefreshToken();
        Task<RefreshToken> SaveRefreshTokenAsync(int userId, string token);
        Task<RefreshToken?> GetRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token);

    }
}
