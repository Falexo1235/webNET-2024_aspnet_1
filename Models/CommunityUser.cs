namespace BlogApi.Models
{
     public class CommunityUser
    {
        public Guid UserId { get; set; }
        public Guid CommunityId { get; set; }
        public string Role { get; set; } // "Administrator" or "Subscriber"

        public Community Community { get; set; }
        public User User { get; set; }
    }
}