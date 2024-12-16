using EduTechBlogsApi.Models;
using EduTechBlogsApi.Models.ViewModels;

namespace EduTechBlogsApi.Services
{
    public interface ITokenService
    {
        Task<AuthResultVM> GenerateJWTToken(ApplicationUser user, RefreshToken? refreshToken = null);
        Task<AuthResultVM> VerifyAndGenerateTokenAsync(TokenRequestVM tokenRequestVM);
    }
}
