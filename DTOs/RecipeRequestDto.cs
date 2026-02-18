namespace CookIA.DTOs
{
    public class RecipeRequestDto
    {
        public List<string> Ingredientes { get; set; } = new();
        public string Objetivo { get; set; }
    }
}
