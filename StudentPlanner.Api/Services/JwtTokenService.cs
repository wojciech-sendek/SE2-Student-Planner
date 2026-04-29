using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using StudentPlanner.Api.Configurations;
using StudentPlanner.Api.Dtos.Auth;
using StudentPlanner.Api.Entities;
using StudentPlanner.Api.Services.Interfaces;

namespace StudentPlanner.Api.Services
{
    public class JwtTokenService : IJwtTokenService
    {
        private readonly JwtOptions _jwtOptions;
        private readonly UserManager<ApplicationUser> _userManager;

        public JwtTokenService(
            IOptions<JwtOptions> jwtOptions,
            UserManager<ApplicationUser> userManager)
        {
            _jwtOptions = jwtOptions.Value;
            _userManager = userManager;
        }

        public async Task<AuthResponseDto> CreateTokenAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new(JwtRegisteredClaimNames.Sub, user.Id),
                new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new(ClaimTypes.NameIdentifier, user.Id),
                new(ClaimTypes.Email, user.Email ?? string.Empty)
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtOptions.SecretKey));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(_jwtOptions.ExpiryMinutes);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expires,
                signingCredentials: credentials);

            return new AuthResponseDto
            {
                Token = new JwtSecurityTokenHandler().WriteToken(token),
                ExpiresAtUtc = expires,
                Email = user.Email ?? string.Empty,
                Roles = roles,
                UsosScheduleSyncedAtUtc = user.UsosScheduleSyncedAtUtc
            };
        }
    }
}