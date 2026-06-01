using FilmMaker.Entities;
using FilmMaker.Helper.Token;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;

namespace FilmMaker.Services.Service
{
    public class TokenService : ITokenService
    {
        private readonly FilmMakerDbContext _context;
        private readonly IConfiguration _config;

        public TokenService(FilmMakerDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        public string GenerateAccessToken(int userId, string firstName, string role)
        {
            return TokenHelper.GenerateJWTToken(userId, firstName, role, _config);
        }

        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }

        public async Task<RefreshToken> SaveRefreshTokenAsync(int userId, string token)
        {
            var refreshToken = new RefreshToken
            {
                UserId = userId,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddDays(7), 
                CreatedAt = DateTime.UtcNow,
                IsRevoked = false
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync();
            return refreshToken;
        }

        public async Task<RefreshToken?> GetRefreshTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .Include(r => r.User).ThenInclude(u => u.Roles)
                .FirstOrDefaultAsync(r => r.Token == token && !r.IsRevoked);
        }

        public async Task RevokeRefreshTokenAsync(string token)
        {
            var refreshToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(r => r.Token == token);

            if (refreshToken is not null)
            {
                refreshToken.IsRevoked = true;
                await _context.SaveChangesAsync();
            }
        }
    }
}
