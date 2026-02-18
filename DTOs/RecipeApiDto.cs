namespace CookIA.DTOs
{
    public class RecipeApiDto
    {
        public string Nombre { get; set; }
        public string Categoria { get; set; }
        public string Area { get; set; }
        public string Instrucciones { get; set; }
        public string ImagenUrl { get; set; }

        public string NivelDificultad {  get; set; }
        public string RecomendacionIA { get; set; }

        public List<string> Sustituciones { get; set; }
        public List<IngredientDto> Ingredientes { get; set; }
    }
}
