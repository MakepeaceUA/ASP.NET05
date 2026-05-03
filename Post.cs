namespace BlogMinimalApi.Models
{
    public class Post
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Content { get; set; }
        public string ImageUrl { get; set; } // Требование 3: Посты должны иметь картинку
        public List<Category> Categories { get; set; } = new List<Category>();
    }
}
