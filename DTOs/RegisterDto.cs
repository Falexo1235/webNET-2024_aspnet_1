using System.ComponentModel.DataAnnotations;
public class RegisterDto
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
    [Required]
    [MinLength(6)]
    public string Password { get; set; }
    [Required]
    [Compare("Password", ErrorMessage = "Password and confirm password does not match")]
    public string ConfirmPassword { get; set; }
    [Required]
    public string FullName { get; set; }
    public DateTime? BirthDate { get; set; }
    [Phone]
    public string? Phone { get; set; }
    [Required]
    [RegularExpression("^(Male|Female)$", ErrorMessage = "Gender must be either 'Male' or 'Female'.")]
    public string Gender { get; set; }
}
