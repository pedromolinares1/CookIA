using System.Text;
using System.Text.Json;

namespace CookIA.Services
{
    public class AiRecipeService : IAiRecipeService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly IConfiguration _configuration;

        public AiRecipeService(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _configuration = configuration;
        }

        /// <summary>
        /// Envía un prompt a la API de Groq y devuelve el texto generado por el modelo.
        /// </summary>
        /// <param name="prompt">Texto que se enviará al modelo de IA</param>
        /// <returns>Respuesta generada por la IA</returns>
        private async Task<string> EnviarPromptAsync(string prompt)
        {
            // Obtenemos la API Key desde la configuración (appsettings o user-secrets)
            var apiKey = _configuration["Groq:ApiKey"];

            // Validamos que la clave exista antes de hacer la petición
            if (string.IsNullOrEmpty(apiKey))
                throw new InvalidOperationException("Groq API key no configurada.");

            // Creamos un cliente HTTP usando IHttpClientFactory
            var client = _httpClientFactory.CreateClient();

            // Construimos el cuerpo de la solicitud que se enviará a Groq
            var requestBody = new
            {
                // Modelo que queremos usar
                model = "llama-3.1-8b-instant",

                // Mensajes enviados al modelo (formato tipo ChatGPT)
                messages = new[]
                {
            new { role = "user", content = prompt }
        },

                // Controla creatividad (0 = más preciso, 1 = más creativo)
                temperature = 0.4,

                // Límite máximo de tokens en la respuesta
                max_tokens = 150
            };

            // Convertimos el objeto a formato JSON
            var jsonContent = JsonSerializer.Serialize(requestBody);

            // Creamos el contenido HTTP con tipo application/json
            var httpContent = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Creamos la solicitud HTTP POST hacia el endpoint de Groq
            var request = new HttpRequestMessage(
                HttpMethod.Post,
                "https://api.groq.com/openai/v1/chat/completions");

            // Asignamos el contenido al request
            request.Content = httpContent;

            // Agregamos el header de autorización con la API Key
            request.Headers.Add("Authorization", $"Bearer {apiKey}");

            // Enviamos la solicitud y esperamos la respuesta
            var response = await client.SendAsync(request);

            // Leemos el contenido de la respuesta en formato texto
            var responseJson = await response.Content.ReadAsStringAsync();

            // Si la respuesta no fue exitosa (status != 200), lanzamos excepción
            if (!response.IsSuccessStatusCode)
                throw new HttpRequestException($"Groq error {(int)response.StatusCode}: {responseJson}");

            // Parseamos el JSON para poder acceder a sus propiedades
            using var document = JsonDocument.Parse(responseJson);
            var root = document.RootElement;

            // Navegamos la estructura esperada:
            // root -> choices[0] -> message -> content
            if (root.TryGetProperty("choices", out var choices) &&
                choices.GetArrayLength() > 0 &&
                choices[0].TryGetProperty("message", out var message) &&
                message.TryGetProperty("content", out var content))
            {
                // 🔹 Retornamos el texto generado por el modelo
                return content.GetString() ?? string.Empty;
            }

            // Si la estructura no coincide con lo esperado, lanzamos error
            throw new InvalidOperationException("Formato inválido de respuesta Groq.");
        }

        // ===============================
        // Recomendación
        // ===============================
        public async Task<string> GenerarRecomendacionAsync(string nombreReceta, string objetivo)
        {
            var prompt = $"""
                La receta se llama "{nombreReceta}".
                El objetivo del usuario es: "{objetivo}".

                Genera una recomendación breve, clara y útil.
                Responde únicamente en español neutro.
                Máximo 3 líneas.
                """;

            return await EnviarPromptAsync(prompt);
        }

        // ===============================
        // Dificultad
        // ===============================
        public async Task<string> GenerarDificultadAsync(string instrucciones)
        {
            var prompt = $"""
                Analiza las siguientes instrucciones de cocina y clasifica la dificultad como:

                Fácil
                Media
                Difícil

                Responde SOLO con una palabra en español.

                Instrucciones:
                {instrucciones}
                """;

            var respuesta = await EnviarPromptAsync(prompt);

            return respuesta.Trim();
        }

        // ===============================
        // Sustituciones
        // ===============================
        public async Task<List<string>> GenerarSustitucionesAsync(List<string> ingredientes)
        {
            var lista = string.Join(", ", ingredientes);

            var prompt = $"""
                Sugiere sustituciones para los siguientes ingredientes:
                {lista}

                Reglas:
                - Máximo 5 sustituciones
                - Una por línea
                - Español neutro
                - No agregues explicaciones largas
                """;

            var respuesta = await EnviarPromptAsync(prompt);

            return respuesta
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim('-', ' ', '\r'))
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ToList();
        }

        // ===============================
        // Traduccion de la palabra ingresada
        // en español a ingles,
        // para ser enviada a la api de recetas.
        // ===============================

        public async Task<string> TraducirAInglesAsync(string texto)
        {
            var prompt = $"Traduce el siguiente ingrediente al inglés. " +
                         $"Responde solo con la palabra traducida, sin explicación:\n\n{texto}";

            var respuesta = await EnviarPromptAsync(prompt);

            return respuesta.Trim().ToLower();
        }

        public async Task<string> TraducirAEspanolAsync(string texto)
        {
            var prompt = $@"
                Traduce el siguiente texto al español.
                Devuelve únicamente la traducción.
                No agregues explicaciones.
                No agregues notas.
                No agregues frases como 'Aquí está la traducción'.
                No agregues comillas.

                Texto:
                {texto}";

            return await EnviarPromptAsync(prompt);
        }

        public async Task<List<string>> TraducirListaAEspanolAsync(List<string> textos)
        {
            var lista = string.Join("\n", textos);

            var prompt = $@"
                Traduce cada elemento al español.
                Devuelve solo la lista traducida.
                Una línea por elemento.
                Sin explicaciones.
                Sin notas adicionales.

                Lista:
                {lista}";

            var respuesta = await EnviarPromptAsync(prompt);

            return respuesta
                .Split('\n', StringSplitOptions.RemoveEmptyEntries)
                .Select(x => x.Trim())
                .ToList();
        }
    }
}
