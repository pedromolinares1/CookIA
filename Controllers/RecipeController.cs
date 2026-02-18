using Microsoft.AspNetCore.Mvc;
using CookIA.Services;
using CookIA.DTOs;

namespace CookIA.Controllers
{

    /// <summary>
    /// Controlador encargado de manejar las peticiones relacionadas con recetas.
    /// Expone los endpoints HTTP que consume el frontend.
    /// </summary>
    [ApiController] // Indica que es un controlador de API (habilita validaciones automáticas)
    [Route("api/[controller]")] // Ruta base: api/Recipe
    public class RecipeController : ControllerBase
    {
        // Servicio que contiene la lógica de negocio
        private readonly IRecipeService _recipeService;

        /// <summary>
        /// Constructor que recibe el servicio por inyección de dependencias.
        /// </summary>
        public RecipeController(IRecipeService recipeService)
        {
            _recipeService = recipeService;
        }

        /// <summary>
        /// Endpoint que genera una receta basada en ingredientes y objetivo del usuario.
        /// </summary>
        /// <param name="request">Objeto que contiene la lista de ingredientes y el objetivo</param>
        /// <returns>Receta generada con datos enriquecidos por IA</returns>
        [HttpPost("generate")] // Ruta final: POST api/Recipe/generate
        public async Task<IActionResult> GenerateRecipe([FromBody] RecipeRequestDto request)
        {
            try
            {
                // Llamamos al servicio que contiene la lógica principal
                var result = await _recipeService
                    .GetRecipeByIngredientAsync(request.Ingredientes, request.Objetivo);

                // Si todo sale bien, devolvemos 200 OK con la receta
                return Ok(result);
            }
            catch (Exception ex)
            {
                // Si no se encuentran recetas, devolvemos 404
                if (ex.Message.Contains("No se encontraron recetas"))
                    return NotFound(new { message = ex.Message });

                // Para cualquier otro error, devolvemos 400 BadRequest
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
