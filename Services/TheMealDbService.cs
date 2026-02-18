using System.Net.Http;

namespace CookIA.Services
{
    /// <summary>
    /// Servicio encargado de comunicarse con la API pública TheMealDB.
    /// Implementa los métodos definidos en ITheMealDbService.
    /// </summary>
    public class TheMealDbService : ITheMealDbService
    {
        // Factory para crear instancias de HttpClient de forma segura
        private readonly IHttpClientFactory _factory;

        /// <summary>
        /// Constructor que recibe la factoría de HttpClient por inyección de dependencias.
        /// </summary>
        public TheMealDbService(IHttpClientFactory factory)
        {
            _factory = factory;
        }

        /// <summary>
        /// Obtiene la información completa de una receta usando su ID.
        /// </summary>
        /// <param name="id">ID de la receta en TheMealDB</param>
        /// <returns>JSON con la información completa de la receta</returns>
        public async Task<string> GetMealByIdAsync(string id)
        {
            // Creamos el cliente HTTP
            var client = _factory.CreateClient();

            // Construimos la URL para buscar receta por ID
            var url = $"https://www.themealdb.com/api/json/v1/1/lookup.php?i={id}";

            // Realizamos la petición GET
            var response = await client.GetAsync(url);

            // Leemos la respuesta como texto (JSON)
            var json = await response.Content.ReadAsStringAsync();

            // Validamos que la respuesta sea exitosa
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error API TheMealDB: {json}");

            // Retornamos el JSON recibido
            return json;
        }

        /// <summary>
        /// Obtiene una lista de recetas que contienen un ingrediente específico.
        /// </summary>
        /// <param name="ingrediente">Ingrediente a buscar</param>
        /// <returns>JSON con la lista de recetas filtradas</returns>
        public async Task<string> GetMealsByIngredientAsync(string ingrediente)
        {
            // Validamos que el ingrediente no esté vacío
            if (string.IsNullOrWhiteSpace(ingrediente))
                throw new ArgumentException("Ingrediente requerido");

            // Creamos el cliente HTTP
            var client = _factory.CreateClient();

            // Codificamos el ingrediente para que sea seguro en la URL
            var encodedIngredient = Uri.EscapeDataString(ingrediente);

            // Construimos la URL para filtrar recetas por ingrediente
            var url = $"https://www.themealdb.com/api/json/v1/1/filter.php?i={encodedIngredient}";

            // Realizamos la petición GET
            var response = await client.GetAsync(url);

            // Leemos la respuesta JSON
            var json = await response.Content.ReadAsStringAsync();

            // Validamos que la respuesta sea exitosa
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Error API TheMealDB: {json}");

            // Retornamos el JSON recibido
            return json;
        }
    }
}
