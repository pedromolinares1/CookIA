namespace CookIA.Services
{
    public interface IAiRecipeService
    {
        Task<string> GenerarRecomendacionAsync(string nombreReceta, string objetivo);
        Task<string> GenerarDificultadAsync(string instrucciones);
        Task<List<string>> GenerarSustitucionesAsync(List<string> ingredientes);
        Task<string> TraducirAInglesAsync(string texto);
        Task<string> TraducirAEspanolAsync(string texto);
        Task<List<string>> TraducirListaAEspanolAsync(List<string> textos);
    }
}
