namespace BlogApi.Models
{
    public class Post
    {
        public Guid Id { get; set; }  // Используем Guid вместо int
        public string Title { get; set; }
        public string Description { get; set; }
        public int ReadingTime { get; set; }
        public string? Image { get; set; }
        public Guid? AddressId { get; set; }
        public DateTime CreateTime { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Like> Likes { get; set; }
        public List<Comment> Comments { get; set; }  // Связь с комментариями

        public Guid AuthorId { get; set; }  // Используем Guid для связи с User
        public string Author { get; set; }  // Имя автора

    }
}
