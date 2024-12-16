using System.ComponentModel.DataAnnotations;

namespace EduTechBlogsApi.Models.ViewModels
{
    public class TokenRequestVM
    {
        [Required]
        public required string Token { get; set; }
        [Required]
        public required string RefreshToken { get; set; }
    }
}
