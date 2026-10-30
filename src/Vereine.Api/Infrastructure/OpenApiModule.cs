using Scalar.AspNetCore;
using Shiny;

namespace Vereine.Api.Infrastructure;

/// <summary>
/// OpenAPI-Dokument unter <c>/openapi/v1.json</c>: Quelle für die Typen in vereine-web und die
/// Mediator-HTTP-Contracts in vereine-app. Scalar als Oberfläche nur in Development.
/// </summary>
public sealed class OpenApiModule : IWebModule
{
    public void Add(WebApplicationBuilder builder) => builder.Services.AddOpenApi();

    public void Use(WebApplication app)
    {
        app.MapOpenApi();
        if (app.Environment.IsDevelopment())
        {
            app.MapScalarApiReference();
        }
    }
}
