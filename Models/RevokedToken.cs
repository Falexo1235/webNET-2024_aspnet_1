namespace BlogApi.Models
{
    public class RevokedToken
    {
        public int Id { get; set; }
        public string Token { get; set; }  // Можно хранить сам токен или его хэш
        public DateTime ExpirationDate { get; set; }  // Дата истечения срока действия токена
    }
}