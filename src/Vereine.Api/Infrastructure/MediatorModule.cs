using Shiny;

namespace Vereine.Api.Infrastructure;

/// <summary>HTTP-Endpunkte aus den Mediator-Handlern (<c>[MediatorHttpGet]</c>, <c>[MediatorHttpPost]</c>).</summary>
public sealed class MediatorModule : IWebModule
{
    public void Add(WebApplicationBuilder builder)
    {
        builder.Services.AddShinyMediator(mediator => mediator.AddMediatorRegistry());

        // Quellgenerierte JSON-Metadaten auch für ASP.NET, damit die Endpunkte ohne Reflection serialisieren.
        builder.Services.ConfigureHttpJsonOptions(options =>
            options.SerializerOptions.TypeInfoResolverChain.Insert(0, ApiJsonContext.Default));
    }

    public void Use(WebApplication app) => app.MapGeneratedMediatorEndpoints();
}
