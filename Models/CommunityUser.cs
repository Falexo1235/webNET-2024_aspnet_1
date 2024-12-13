namespace BlogApi.Models
{
    public class CommunityUser
    {
        public Guid UserId { get; set; }
        public Guid CommunityId { get; set; }
        public string Role { get; set; }
    }
}
