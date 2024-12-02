using System.ComponentModel.DataAnnotations;

namespace BlogApi.Models
{
    public class CreatePostDto
    {
        [Required]
        public string Title { get; set; }  // Обязательное
        [Required]
        public string Description { get; set; }
        public int ReadingTime { get; set; }
        public string? Image { get; set; }  // Необязательное
        public Guid? AddressId { get; set; }  // Необязательное
        [Required]
        public List<Guid> Tags { get; set; }  // Обязательное
    }
}
