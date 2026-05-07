using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace FilmMaker.Helper.Token
{
    public static class TokenHelper
    {
        public static string GenerateJWTToken(int userId, string FirstName, string role, IConfiguration config)
        {
            var jwtToken = new JwtSecurityTokenHandler();
            var secret = config["Jwt:Key"]; // Use the key from appsettings
            var tokenByteKey = Encoding.UTF8.GetBytes(secret);

            var descriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new Claim[]
                {
            new Claim("UserId", userId.ToString()),
            new Claim("FirstName" , FirstName),
            new Claim(ClaimTypes.Role, role),
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = config["Jwt:Issuer"],     // Add this
                Audience = config["Jwt:Audience"], // Add this
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(tokenByteKey), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = jwtToken.CreateToken(descriptor);
            return jwtToken.WriteToken(token);
        }
        public static string IsValidToken(string token)
        {
            try
            {
                var jwtToken = new JwtSecurityTokenHandler();
                var secret = "LongPrimarySecretForTrainingManagementSystemProjectWithAspNetCoreFinalProject";
                var tokenByteKey = Encoding.UTF8.GetBytes(secret);
                var tokenValidatorParams = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(tokenByteKey),
                    ValidateLifetime = true,
                    ValidateAudience = false,
                    ValidateIssuer = false,
                    ClockSkew = TimeSpan.Zero
                };
                var tokenBase = jwtToken.ValidateToken(token, tokenValidatorParams, out SecurityToken valid);
                return "valid";
            }
            catch (Exception ex)
            {
                return $"InValid {ex.Message}";
            }
        }
    }
}
