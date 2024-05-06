using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using mozgovichok.Models.Users;
using System.Security.Claims;
using System.Text;

namespace mozgovichok.Services.Authorization
{
    public class JWTHandler
    {
        private readonly IConfiguration _configuration;
        private readonly IConfigurationSection _jwtSettings;
        //private readonly UserManager<User> _userManager;

        public JWTHandler(IConfiguration configuration, UserManager<User> userManager)
        {
            _configuration = configuration;
            _jwtSettings = _configuration.GetSection("JwtSettings");
            //_userManager = userManager;
        }

        public SigningCredentials GetSigningCredentials()
        {
            var key = Encoding.UTF8.GetBytes(_jwtSettings.GetSection("securityKey").Value);
            var secret = new SymmetricSecurityKey(key);

            return new SigningCredentials(secret, SecurityAlgorithms.HmacSha256);
        }

        public async Task<List<Claim>> GetClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.Email)
            };

            //var roles = await _userManager.GetRolesAsync(user);
            //foreach (var role in roles)
            //{
            //    claims.Add(new Claim(ClaimTypes.Role, role));
            //}

            return claims;
        }

    }
}
