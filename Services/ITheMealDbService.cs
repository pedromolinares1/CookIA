namespace CookIA.Services
{
    public interface ITheMealDbService
    {
        Task<string> GetMealsByIngredientAsync(string ingrediente);
        Task<string> GetMealByIdAsync(string id);
    }
}
