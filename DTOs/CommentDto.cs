using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    public class CommentDto
    {
        public Guid? ParentId { get; set; }  // Если комментарий является вложенным
        [Required]
        public string Content { get; set; }

    }
}
