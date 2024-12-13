namespace BlogApi.Models
{
    public class Group
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public bool IsPrivate { get; set; }
        public int AdminId { get; set; }
        public User Admin { get; set; }
    }
}
