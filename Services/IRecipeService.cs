using CookIA.DTOs;

namespace CookIA.Services
{
    public interface IRecipeService
    {
        Task<RecipeApiDto> GetRecipeByIngredientAsync(List<string> ingredientes, string objetivo);
    }
}
