using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace eShoes.Authentication
{
    public class JwtService
    {
        private readonly SymmetricSecurityKey _signingKey;

        public JwtService(string secretKey, ILogger<JwtService> logger)
        {
            if (string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentException("Secret key cannot be empty or null", nameof(secretKey));
            }

            _signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        }

        /// JWT token for a given username.
        public string GenerateToken(string username)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username)
            };
            
            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddHours(12),
                SigningCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var jwtString = tokenHandler.WriteToken(token);
            return jwtString;
        }
    }
}
