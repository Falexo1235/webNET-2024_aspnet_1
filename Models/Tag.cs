using System.Text.Json.Serialization;

namespace BlogApi.Models
{
    public class Tag
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public DateTime CreateTime { get; set; }
        [JsonIgnore]
        public List<PostTag> PostTags { get; set; }
    }
}
