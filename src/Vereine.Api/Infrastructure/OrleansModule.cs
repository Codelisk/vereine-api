using Orleans.Dashboard;
using Shiny;
using Shiny.Aspire.Orleans.Server;

namespace Vereine.Api.Infrastructure;

/// <summary>
/// Orleans-Silo im API-Prozess. Unter Aspire kommen Clustering, Grain-Storage und Reminder aus der
/// injizierten Konfiguration (Postgres über Shiny.Aspire.Orleans). Ohne sie startet ein
/// Einzel-Silo mit Speicher im RAM: für <c>dotnet run</c> ohne AppHost.
/// </summary>
public sealed class OrleansModule : IWebModule
{
    public void Add(WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);

        var aspireWired = builder.Configuration["Orleans:Clustering:ProviderType"] is not null;
        builder.UseOrleans(silo =>
        {
            if (aspireWired)
            {
                silo.UseAdoNet();
            }
            else
            {
                silo.UseLocalhostClustering()
                    .AddMemoryGrainStorage(Grains.GrainStorage.Default)
                    .UseInMemoryReminderService();
            }

            if (builder.Environment.IsDevelopment())
            {
                silo.AddDashboard();
            }
        });
    }

    public void Use(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
        {
            // Nicht ins OpenAPI-Dokument: daraus entstehen die Clients von App und Web.
            app.MapOrleansDashboard(routePrefix: "/orleans").ExcludeFromDescription();
        }
    }
}
