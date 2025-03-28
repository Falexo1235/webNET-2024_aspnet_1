using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlogApi.Models
{
    public class Post
    {
        public List<Comment> Comments { get; set; }  // Связь с комментариями
        [MaxLength(100)]
        public string Title { get; set; }
        public string Description { get; set; }
        public int? ReadingTime { get; set; }
        public string? Image { get; set; }
        public Guid AuthorId { get; set; }
        public string? Author { get; set; }
        public Guid? CommunityId { get; set; }
        public string? CommunityName { get; set; }
        public Guid? AddressId { get; set; }
        [JsonIgnore]
        public List<PostTag> PostTags { get; set; }
        [JsonIgnore]
        public List<Tag> Tags { get; set; } = new List<Tag>();
        public List<Like> Likes { get; set; }
        public Guid Id { get; set; }
        public DateTime CreateTime { get; set; }
        
    }
}
