using System.Text.Json.Serialization;

namespace BlogApi.Models
{
    public class User
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public Guid Id { get; set; }  // Используем Guid вместо int
        public DateTime CreationTime { get; set; }
        
        [JsonIgnore]
        public string PasswordHash { get; set; }
    }
}
