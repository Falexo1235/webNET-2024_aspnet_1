namespace BlogApi.Models
{
    public class CommunityUser
    {
        public Guid UserId { get; set; }
        public Guid CommunityId { get; set; }
        public string Role { get; set; }
        public User User { get; set; }
        public Community Community { get; set; }
    }
}
