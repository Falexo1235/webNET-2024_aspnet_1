namespace BlogApi.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // Связь с автором
        public int AuthorId { get; set; }
        public User Author { get; set; }
    }
}
