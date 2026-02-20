using CookIA.Services;

var builder = WebApplication.CreateBuilder(args);

// 🔥 IMPORTANTE: usar el puerto que Render asigna
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://*:{port}");

// 🔥 Verificación directa de variable de entorno
var rawEnvKey = Environment.GetEnvironmentVariable("Groq__ApiKey");
Console.WriteLine("ENV DIRECTA Groq__ApiKey: " + (string.IsNullOrEmpty(rawEnvKey) ? "NULL" : "OK"));

var configKey = builder.Configuration["Groq:ApiKey"];
Console.WriteLine("CONFIG Groq:ApiKey: " + (string.IsNullOrEmpty(configKey) ? "NULL" : "OK"));

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient();

builder.Services.AddScoped<ITheMealDbService, TheMealDbService>();
builder.Services.AddScoped<IRecipeService, RecipeService>();
builder.Services.AddScoped<IAiRecipeService, AiRecipeService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend",
        policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyHeader()
                  .AllowAnyMethod();
        });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseCors("AllowFrontend");

app.UseAuthorization();

app.MapControllers();

app.Run();