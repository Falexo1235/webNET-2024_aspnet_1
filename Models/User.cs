using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace BlogApi.Models
{
    public class User
    {
        public string Email { get; set; }
        [MaxLength(70)]
        public string FullName { get; set; }
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        public string Gender { get; set; }
        public Guid Id { get; set; }
        public DateTime CreationTime { get; set; }
        
        [JsonIgnore]
        public string PasswordHash { get; set; }
    }
}
