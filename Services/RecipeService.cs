using CookIA.DTOs;
using System.Text.Json;

namespace CookIA.Services
{
    /// <summary>
    /// Servicio principal que orquesta la lógica de negocio de recetas.
    /// Se encarga de:
    /// - Traducir ingredientes
    /// - Consultar TheMealDB
    /// - Traducir información al español
    /// - Enriquecer datos usando IA
    /// </summary>
    public class RecipeService : IRecipeService
    {
        // Servicio que se comunica con TheMealDB (API de recetas)
        private readonly ITheMealDbService _mealservice;

        // Servicio que se comunica con la IA (Groq)
        private readonly IAiRecipeService _aiService;

        /// <summary>
        /// Constructor que recibe las dependencias por inyección.
        /// </summary>
        public RecipeService(ITheMealDbService mealservice, IAiRecipeService aiService)
        {
            _mealservice = mealservice;
            _aiService = aiService;
        }

        /// <summary>
        /// Método principal que genera una receta completa a partir de ingredientes y objetivo.
        /// </summary>
        public async Task<RecipeApiDto> GetRecipeByIngredientAsync(List<string> ingredientes, string objetivo)
        {
            // Validamos que exista al menos un ingrediente
            if (ingredientes == null || !ingredientes.Any())
                throw new ArgumentException("Debe proporcionar al menos un ingrediente.");

            // TRADUCIR INGREDIENTE A INGLÉS
            var ingredienteOriginal = ingredientes.First().Trim().ToLower();
            var ingredienteEnIngles = await _aiService.TraducirAInglesAsync(ingredienteOriginal);

            // BUSCAR RECETAS POR INGREDIENTE
            var filterJson = await _mealservice.GetMealsByIngredientAsync(ingredienteEnIngles);

            using var filterDoc = JsonDocument.Parse(filterJson);
            var filterRoot = filterDoc.RootElement;

            if (!filterRoot.TryGetProperty("meals", out var meals) || meals.ValueKind == JsonValueKind.Null)
                throw new Exception("No se encontraron recetas.");

            var mealId = meals[0].GetProperty("idMeal").GetString();

            // OBTENER RECETA COMPLETA POR ID
            var lookupJson = await _mealservice.GetMealByIdAsync(mealId);

            using var lookupDoc = JsonDocument.Parse(lookupJson);
            var fullMeal = lookupDoc.RootElement.GetProperty("meals")[0];

            var nombreRecetaEn = fullMeal.GetProperty("strMeal").GetString();
            var instruccionesEn = fullMeal.GetProperty("strInstructions").GetString();

            // CONSTRUIR LISTA DE INGREDIENTES
            var listaIngredientes = new List<IngredientDto>();

            for (int i = 1; i <= 20; i++)
            {
                var ingProp = $"strIngredient{i}";
                var medProp = $"strMeasure{i}";

                if (fullMeal.TryGetProperty(ingProp, out var ingredienteProp) &&
                    fullMeal.TryGetProperty(medProp, out var medidaProp))
                {
                    var ing = ingredienteProp.GetString();
                    var med = medidaProp.GetString();

                    if (!string.IsNullOrWhiteSpace(ing))
                    {
                        listaIngredientes.Add(new IngredientDto
                        {
                            Nombre = ing,
                            Medida = med
                        });
                    }
                }
            }

            // TRADUCIR CONTENIDO AL ESPAÑOL
            var nombreRecetaEs = await _aiService.TraducirAEspanolAsync(nombreRecetaEn);
            var instruccionesEs = await _aiService.TraducirAEspanolAsync(instruccionesEn);

            // Traducimos lista de ingredientes
            var nombresIngredientes = listaIngredientes.Select(i => i.Nombre).ToList();
            var ingredientesTraducidos = await _aiService.TraducirListaAEspanolAsync(nombresIngredientes);

            for (int i = 0; i < listaIngredientes.Count && i < ingredientesTraducidos.Count; i++)
            {
                listaIngredientes[i].Nombre = ingredientesTraducidos[i];
            }

            // CONSTRUIR RESPUESTA FINAL
            return new RecipeApiDto
            {
                Nombre = nombreRecetaEs,
                Categoria = fullMeal.GetProperty("strCategory").GetString(),
                Area = fullMeal.GetProperty("strArea").GetString(),
                Instrucciones = instruccionesEs,
                ImagenUrl = fullMeal.GetProperty("strMealThumb").GetString(),
                Ingredientes = listaIngredientes,

                // Generamos dificultad basada en instrucciones traducidas
                NivelDificultad = await _aiService
                    .GenerarDificultadAsync(instruccionesEs),

                // Generamos recomendación personalizada
                RecomendacionIA = await _aiService
                    .GenerarRecomendacionAsync(nombreRecetaEs, objetivo),

                // Generamos sustituciones usando ingredientes traducidos
                Sustituciones = await _aiService
                    .GenerarSustitucionesAsync(
                        listaIngredientes.Select(i => i.Nombre).ToList())
            };
        }
    }
}
