namespace BlogApi.Models
{
    public class User
    {
        public Guid Id { get; set; }  // Используем Guid вместо int
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string PasswordHash { get; set; }
    }
}
