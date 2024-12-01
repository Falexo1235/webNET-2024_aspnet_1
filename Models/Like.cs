namespace BlogApi.Models
{
    public class Like
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PostId { get; set; } // Связь с постом
        public Guid UserId { get; set; } // Связь с пользователем
    }
}