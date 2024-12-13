using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }
        [Phone]
        public string? Phone { get; set; }
        public DateTime? BirthDate { get; set; }
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
        public string? Gender { get; set; }
    }
}
