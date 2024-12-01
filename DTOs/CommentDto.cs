namespace BlogApi.DTOs
{
    public class CommentDto
    {
        public Guid? ParentId { get; set; }  // Если комментарий является вложенным
        public string Content { get; set; }

    }
}
