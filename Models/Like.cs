namespace BlogApi.Models
{
    public class Like
    {
        public int Id { get; set; }
        public int PostId { get; set; }  // Связь с постом
        public Post Post { get; set; }

        public int UserId { get; set; }  // Связь с пользователем
        public User User { get; set; }
    }
}
