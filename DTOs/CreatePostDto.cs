namespace BlogApi.Models
{
    public class CreatePostDto
    {
        public string Title { get; set; }  // Обязательное
        public string Description { get; set; }  // Обязательное
        public int ReadingTime { get; set; }
        public string? Image { get; set; }  // Необязательное
        public Guid? AddressId { get; set; }  // Необязательное
        public List<Guid> Tags { get; set; }  // Обязательное
    }
}
