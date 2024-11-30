namespace BlogApi.Models
{
    public class User
    {
        public int Id { get; set; }

        public string Email { get; set; }  // Обязательное
        public string PasswordHash { get; set; }  // Обязательное
        public string FullName { get; set; }  // Обязательное
        public string? Phone { get; set; }  // Необязательное, может быть NULL
        public DateTime? DateOfBirth { get; set; }  // Необязательное
        public string Gender { get; set; }  // Обязательное

        // Связь с постами
        public ICollection<Post> Posts { get; set; }
    }
}
