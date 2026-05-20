using FilmMaker.Entities;
using FilmMaker.Services.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Cryptography;

namespace FilmMaker.Services.Service
{
    public class OtpService : IOtpService
    {
        private readonly FilmMakerDbContext _context;

        public OtpService(FilmMakerDbContext context)
        {
            _context = context;
        }

        public async Task<string> GenerateAndSaveOtpAsync(int userId, OtpPurpose purpose)
        {
            
            var existing = await _context.OtpCodes
                .Where(o => o.UserId == userId && o.Purpose == purpose && !o.IsUsed)
                .ToListAsync();

            _context.OtpCodes.RemoveRange(existing);

            
            var code = RandomNumberGenerator.GetInt32(100000, 999999).ToString();

            _context.OtpCodes.Add(new OtpCode
            {
                UserId = userId,
                Code = code,
                Purpose = purpose,
                ExpiresAt = DateTime.UtcNow.AddMinutes(10)
            });

            await _context.SaveChangesAsync();
            return code;
        }

        public async Task<OtpCode?> ValidateOtpAsync(int userId, string code, OtpPurpose purpose)
        {
            var otp = await _context.OtpCodes.FirstOrDefaultAsync(o =>
                o.UserId == userId &&
                o.Code == code &&
                o.Purpose == purpose &&
                !o.IsUsed &&
                o.ExpiresAt > DateTime.UtcNow);

            if (otp is null) return null;

            
            otp.IsUsed = true;
            await _context.SaveChangesAsync();

            return otp;
        }
    }
}
