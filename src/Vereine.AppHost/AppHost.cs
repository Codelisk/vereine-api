using Shiny.Aspire.Orleans.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Orleans-Zustand in Postgres: Clustering, Grain-Storage und Reminder. Shiny legt das Schema an,
// sobald die Datenbank bereit ist. Die Skripte sind nicht wiederholbar, deshalb ohne Daten-Volume:
// jeder Start beginnt mit leerer Datenbank (ADR-0001).
var postgres = builder.AddPostgres("postgres");
var orleansDb = postgres.AddDatabase("orleans-db");

var orleans = builder.AddOrleans("vereine")
    .WithClustering(orleansDb)
    .WithGrainStorage("Default", orleansDb)
    .WithReminders(orleansDb)
    .WithDatabaseSetup(orleansDb);

var api = builder.AddProject<Projects.Vereine_Api>("vereine-api")
    .WithReference(orleans)
    .WaitFor(orleansDb);

// Verwaltungswebseite als Astro-Dev-Server aus dem Geschwister-Repo (dev/repos/vereine-web).
// Fehlt das Repo, läuft der Stack ohne sie.
var webDirectory = Path.GetFullPath(Path.Combine(builder.AppHostDirectory, "..", "..", "..", "vereine-web"));
if (File.Exists(Path.Combine(webDirectory, "package.json")))
{
    // dev:aspire = astro dev --ignore-lock: bleibt im Vordergrund, auch wenn ein KI-Agent den AppHost startet
    // (sonst schickt Astro den Dev-Server in den Hintergrund und Aspire verliert ihn).
    builder.AddJavaScriptApp("vereine-web", webDirectory, "dev:aspire")
        .WithNpm(install: true, installCommand: "install")
        .WithHttpEndpoint(port: 4610, env: "PORT")
        // Nur auf Loopback lauschen, nach außen geht es über den Aspire-Proxy (4610).
        .WithEnvironment("HOST", "127.0.0.1")
        .WithEnvironment("VEREINE_API_URL", api.GetEndpoint("http"))
        .WithOtlpExporter(OtlpProtocol.HttpProtobuf)
        .WithHttpHealthCheck("/gesund")
        .WaitFor(api);
}

builder.Build().Run();
