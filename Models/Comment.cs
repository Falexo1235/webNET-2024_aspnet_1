using System.Text.Json.Serialization;

namespace BlogApi.Models
{
    public class Comment
    {
        public string Content { get; set; }
        public DateTime? ModifiedDate { get; set; }
        public DateTime? DeleteDate { get; set; }
        public Guid AuthorId { get; set; }
        public string? Author { get; set; }

        [JsonIgnore]
        public Guid PostId { get; set; }

        [JsonIgnore]
        public Guid? ParentId { get; set; }
        public int SubComments { get; set; }
        public Guid Id { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
