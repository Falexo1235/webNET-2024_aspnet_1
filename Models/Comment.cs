namespace BlogApi.Models
{
    public class Comment
    {
        public int Id { get; set; }
        public string Content { get; set; }
        public DateTime CreatedAt { get; set; }

        // Связь с постом
        public int PostId { get; set; }
        public Post Post { get; set; }

        // Связь с пользователем
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
