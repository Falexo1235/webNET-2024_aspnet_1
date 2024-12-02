using System.ComponentModel.DataAnnotations;

namespace BlogApi.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }  // Можно обновить имя
        [Phone]
        public string? Phone { get; set; }     // Можно обновить номер телефона
        public DateTime? BirthDate { get; set; }  // Можно обновить дату рождения
        [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
        public string? Gender { get; set; }    // Можно обновить пол
    }
}
