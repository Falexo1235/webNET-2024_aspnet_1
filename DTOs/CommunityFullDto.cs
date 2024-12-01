namespace BlogApi.DTOs
{
    public class CommunityFullDto
    {
        public Guid Id { get; set; }
        public DateTime CreateTime { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public bool IsClosed { get; set; }
        public int SubscribersCount { get; set; }
        public List<CommunityUserDto> Administrators { get; set; }
    }
}