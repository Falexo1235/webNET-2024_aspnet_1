namespace BlogApi.Models
{
    public class Post
    {
        public List<Comment> Comments { get; set; }  // Связь с комментариями
        public string Title { get; set; }
        public string Description { get; set; }
        public int ReadingTime { get; set; }
        public string? Image { get; set; }
        public Guid AuthorId { get; set; }
        public string? Author { get; set; }
        public Guid? CommunityId { get; set; }
        public string? CommunityName { get; set; }
        public Guid? AddressId { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Like> Likes { get; set; }
        public Guid Id { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
