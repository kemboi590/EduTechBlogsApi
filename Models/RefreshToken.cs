using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace EduTechBlogsApi.Models
{
    public class RefreshToken
    {
        [Key]
        public int Id { get; set; }

        public required string Token { get; set; }
        public required string JwtId { get; set; }
        public bool IsRevoked { get; set; }
        public DateTime DateAdded { get; set; }
        public DateTime DateExipire { get; set; }

        // Foreign Key
        public required string UserId { get; set; } 
        [ForeignKey(nameof(UserId))]
        public required ApplicationUser User { get; set; }
    }
}
