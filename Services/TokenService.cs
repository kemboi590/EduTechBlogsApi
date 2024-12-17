using EduTechBlogsApi.Models;
using EduTechBlogsApi.Models.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Data;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace EduTechBlogsApi.Services
{
    public class TokenService: ITokenService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly TokenValidationParameters _tokenValidationParameters;

        public TokenService(UserManager<ApplicationUser> userManager, // 
                        AppDbContext context, // used to access the RefreshTokens table
                        IConfiguration configuration, // used to access appsettings.json
                        TokenValidationParameters tokenValidationParameters)
        {
            _userManager = userManager;
            _context = context;
            _configuration = configuration;
            _tokenValidationParameters = tokenValidationParameters;
        }

        public async Task<AuthResultVM> GenerateJWTToken(ApplicationUser user, RefreshToken? rToken)
        {
            var authClaims = new List<Claim> // at the end the list will look like this: ["Name" = "user.UserName", "NameIdentifier" = "user.Id", "Email" = "user.Email", "Sub" = "user.Email", "Jti" = "Guid.NewGuid().ToString()"]
           // [Name, NameIdentifier, Email, Sub, Jti, Role]
        {
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Sub, user.Email ?? string.Empty),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

            // Add role claims
            var userRoles = await _userManager.GetRolesAsync(user);
            foreach (var role in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, role));
            }

            var jwtSecret = _configuration["JWT:Secret"] ?? throw new InvalidOperationException("JWT:Secret is not configured.");
            var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

            var token = new JwtSecurityToken(
                issuer: _configuration["JWT:Issuer"],
                audience: _configuration["JWT:Audience"],
                expires: DateTime.UtcNow.AddHours(1),
                claims: authClaims,
                signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
            );

            var jwtToken = new JwtSecurityTokenHandler().WriteToken(token);

            if (rToken != null)
            {
                return new AuthResultVM
                {
                    Token = jwtToken,
                    RefreshToken = rToken.Token,
                    ExpiresAt = token.ValidTo
                };
            }

            var refreshToken = new RefreshToken()
            {
                JwtId = token.Id,
                UserId = user.Id,
                Token = Guid.NewGuid().ToString() + "-" + Guid.NewGuid().ToString(),
                DateAdded = DateTime.UtcNow,
                DateExipire = DateTime.UtcNow.AddMonths(6),
                User = user
            };

            await _context.RefreshTokens.AddAsync(refreshToken);
            await _context.SaveChangesAsync();

            return new AuthResultVM
            {
                Token = jwtToken,
                RefreshToken = refreshToken.Token,
                ExpiresAt = token.ValidTo
            };
        }

        public async Task<AuthResultVM> VerifyAndGenerateTokenAsync(TokenRequestVM tokenRequestVM)
        {
            var jwtTokenHandler = new JwtSecurityTokenHandler();
            var storedToken = await _context.RefreshTokens.FirstOrDefaultAsync(x => x.Token == tokenRequestVM.RefreshToken);

            if (storedToken == null)
                throw new InvalidOperationException("Refresh token not found.");

            var dbUser = await _userManager.FindByIdAsync(storedToken.UserId);
            if (dbUser == null)
                throw new InvalidOperationException("User not found.");

            try
            {
                jwtTokenHandler.ValidateToken(tokenRequestVM.Token, _tokenValidationParameters, out _);
                return await GenerateJWTToken(dbUser, storedToken);
            }
            catch (SecurityTokenExpiredException)
            {
                if (storedToken.DateExipire >= DateTime.UtcNow)
                {
                    return await GenerateJWTToken(dbUser, storedToken);
                }
                return await GenerateJWTToken(dbUser, null);
            }
        }
    }
}