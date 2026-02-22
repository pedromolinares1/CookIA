# CookIA - Backend API

CookIA es una API desarrollada en ASP.NET Core que genera recetas personalizadas a partir de ingredientes y un objetivo nutricional del usuario.

El sistema integra:
- TheMealDB (fuente de recetas)
- OpenAI (IA para enriquecer y adaptar la receta)

---

## Tecnologías usadas

- .NET 8
- ASP.NET Core Web API
- HttpClient
- Swagger
- Render (deploy)
- TheMealDB API
- Groq API

---

## Cómo funciona

1. El usuario envía ingredientes y un objetivo.
2. La API consulta TheMealDB para obtener una receta base.
3. La receta se envía a OpenAI para adaptarla según el objetivo.
4. Se devuelve la receta personalizada al frontend.

---

## 🔗 Endpoint principal

POST

/api/Recipe/generate

### Body ejemplo:

```json
{
  "ingredientes": "pollo",
  "objetivo": "ganar masa muscular"
}
