using EduTechBlogsApi.Models.DTO;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EduTechBlogsApi.Models
{
    public class BlogItem
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public required string Title { get; set; }
        [Required]
        public required string Description { get; set; }

        [Required]
        public required string Body { get; set; }

        // the reference to the author
        public required string AuthorId { get; set; }
        [ForeignKey(nameof(AuthorId))]
        [JsonIgnore]
        public  ApplicationUser? User { get; set; }
        [NotMapped] //means that this property is not mapped to the database
        public UserDto? UserDto { get; set; }

    }
}
