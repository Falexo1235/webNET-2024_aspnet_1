namespace BlogApi.DTOs
{
    public class UpdateProfileDto
    {
        public string? FullName { get; set; }  // Можно обновить имя
        public string? Phone { get; set; }     // Можно обновить номер телефона
        public DateTime? DateOfBirth { get; set; }  // Можно обновить дату рождения
        public string? Gender { get; set; }    // Можно обновить пол
    }
}
