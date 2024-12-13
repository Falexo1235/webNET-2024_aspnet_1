using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models
{
    public class CreatePostDto
    {
        [Required]
        public string Title { get; set; }
        [Required]
        public string Description { get; set; }
        public int ReadingTime { get; set; }
        public string? Image { get; set; }
        public Guid? AddressId { get; set; }
        [Required]
        public List<Guid> Tags { get; set; }
    }
}
