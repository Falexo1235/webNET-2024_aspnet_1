using System.ComponentModel.DataAnnotations;

public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public DateTime? BirthDate { get; set; }  // Необязательное
    public string? Phone { get; set; } // Поле для телефона
    [Required]
    [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
    public string Gender { get; set; } // Поле для телефона

}
