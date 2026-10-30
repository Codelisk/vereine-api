# vereine-api

Backend von **Vereineapp**, einer App, die das Ehrenamt in Vereinen entlastet (zuerst Fußballvereine).

- .NET 11 (neueste Vorschau, `global.json`), C# preview
- [Microsoft Orleans](https://learn.microsoft.com/dotnet/orleans/): Fachlogik und Zustand in Grains
- [Aspire](https://aspire.dev): lokaler Stack mit Postgres und der Verwaltungswebseite
- [Shiny](https://shinylib.net): Mediator-Endpunkte, Web-Module, Aspire-Orleans-Anbindung

Geschwister-Repos: [vereine-app](https://github.com/Codelisk/vereine-app) (MAUI Blazor Hybrid) und [vereine-web](https://github.com/Codelisk/vereine-web) (Astro).

## Starten

Voraussetzung: .NET SDK aus `global.json`, für den vollen Stack Docker und Node.

```bash
dotnet run --project src/Vereine.Api        # nur API, Orleans im RAM: http://localhost:5610/scalar
dotnet run --project src/Vereine.AppHost    # Postgres, API, Webseite; Dashboard: http://localhost:15610
```

Der AppHost startet `vereine-web`, wenn das Repo daneben liegt (`../vereine-web`).

## Bauen und testen

```bash
dotnet build Vereine.Api.slnx
dotnet test --project tests/Vereine.Tests
docker build -t vereine-api .
```

## Aufbau

| Projekt | Inhalt |
|---|---|
| `Vereine.Abstractions` | Grain-Interfaces und Contracts |
| `Vereine.Grains` | Grains |
| `Vereine.Api` | Silo und HTTP-API in einem Prozess |
| `Vereine.ServiceDefaults` | Telemetrie und Health-Checks |
| `Vereine.AppHost` | Aspire-Stack für die Entwicklung |

Entscheidungen stehen unter [`docs/adr`](docs/adr). Kurz: Orleans zuerst ([ADR-0001](docs/adr/0001-orleans-zuerst-auf-shiny-und-aspire.md)); die Bibliotheken sind AOT-tauglich, der Silo läuft mit JIT, bis Orleans NativeAOT unterstützt ([ADR-0002](docs/adr/0002-aot-im-backend.md)).
