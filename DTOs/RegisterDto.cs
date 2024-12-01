public class RegisterDto
{
    public string Email { get; set; }
    public string Password { get; set; }
    public string FullName { get; set; }
    public DateTime? DateOfBirth { get; set; }  // Необязательное
    public string? Phone { get; set; } // Поле для телефона
    public string Gender { get; set; } // Поле для телефона

}
