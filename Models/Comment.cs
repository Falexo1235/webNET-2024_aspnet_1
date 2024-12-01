using System.Text.Json.Serialization;

namespace BlogApi.Models
{
    public class Comment
    {
        public string Content { get; set; }
        public DateTime? ModifiedDate { get; set; }

        public DateTime? DeleteDate { get; set; }

        public Guid AuthorId { get; set; }  // Используем Guid для связи с User

        // Публичное свойство для извлечения имени автора
        public string? Author { get; set; } // Можем сделать его вычисляемым через метод или загрузить при запросе

        [JsonIgnore]
        public Guid PostId { get; set; }  // Используем Guid для связи с Post

        [JsonIgnore]
        public Guid? ParentId { get; set; }  // Используем Guid для связи с родительским комментарием
        public int SubComments { get; set; }  // Количество подкомментариев
        public Guid Id { get; set; }  // Используем Guid вместо int
        public DateTime CreateTime { get; set; }


    }
}
