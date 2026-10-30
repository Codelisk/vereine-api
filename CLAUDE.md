# CLAUDE.md: vereine-api

Backend von Vereineapp: .NET 11, Microsoft Orleans, Aspire, auf Basis von Shiny. Es entlastet das Ehrenamt in Vereinen, zuerst für Fußballvereine. Die Fachlichkeit folgt; bis dahin zeigt das Grain `Club` das Muster.

Die Workspace-Regeln aus `/srv/business/vereineapp/CLAUDE.md` gelten (Secrets, öffentliches Repo, keine Abwärtskompatibilität, Nutzertexte, gezielte Tests).

## Kommandos

```bash
source /srv/business/vereineapp/dev/env.sh                 # RC-SDK aus dev/tools/dotnet, nie ~/.dotnet (gehört Orderlyze)
dotnet build Vereine.Api.slnx                              # Warnungen sind Fehler, Output unter artifacts/
dotnet test --project tests/Vereine.Tests --filter-class "Vereine.Tests.Clubs.ClubGrainTests"
dotnet run --project src/Vereine.Api                       # ohne Docker: Orleans im RAM, http://localhost:5610
dotnet run --project src/Vereine.AppHost                   # voller Stack: Postgres, API, vereine-web (Docker nötig)
```

Ports: API 5610, Webseite 4610, Aspire-Dashboard 15610. Ports von Orderlyze und Terminwerk nie belegen.

## Aufbau

| Projekt | Inhalt |
|---|---|
| `src/Vereine.Abstractions` | Grain-Interfaces, übertragene Typen, Fachregeln (`ClubName`), `RequestRejectedException` |
| `src/Vereine.Grains` | Grain-Implementierungen und ihr gespeicherter Zustand |
| `src/Vereine.Api` | Silo und HTTP in einem Prozess: `IWebModule`s unter `Infrastructure/`, Mediator-Handler je Domäne |
| `src/Vereine.ServiceDefaults` | OpenTelemetry, Health-Checks (`/alive`, `/health`), Service Discovery |
| `src/Vereine.AppHost` | Aspire: Postgres, Orleans-Cluster, API, vereine-web aus dem Geschwister-Repo |
| `tests/Vereine.Tests` | xunit v3 mit Orleans `TestCluster` |

Ablauf einer Anfrage: HTTP → generierter Endpunkt → Mediator-Handler → ein Grain → Antwort. Fachliche Fehler wirft das Grain als `RequestRejectedException`, `ProblemDetailsModule` macht 400/404/409 daraus.

## Harte Regeln

1. **Orleans zuerst** (ADR-0001). Zustand und Fachlogik gehören in Grains. Vor jeder Änderung Skill `orleans-docs`: Wie löst Orleans das (Persistenz, Reminder, Streams, Timer, Observer)? Gegen die offizielle Doku prüfen. Eigene Infrastruktur daneben nur mit ADR.
2. **AOT, soweit Orleans es zulässt** (ADR-0002). Bibliotheken sind `IsAotCompatible`, Analyzer-Warnungen sind Fehler. Der Host läuft mit JIT, Code wird trotzdem AOT-tauglich geschrieben: JSON nur über `ApiJsonContext`, keine Reflection, kein Assembly-Scan. Bei jedem Orleans-Update die Umstiegs-Bedingung im ADR prüfen.
3. **Neueste C#-Vorschau** (`LangVersion` preview): Primary Constructors, Collection Expressions, Records und neue Sprachmittel nutzen, wo sie klarer sind.
4. **Grain-Interfaces sind Cluster-API:** `[Alias]` auf Interface, Methoden und Typen, `[Id(n)]` nur anhängen, Lesemethoden `[ReadOnly]`, schreibende Aufrufe idempotent.
5. **Zeit nur über `TimeProvider`**, nie `DateTime.Now`.
6. **Versionen zentral** in `Directory.Packages.props`, SDK in `global.json`. Neue .NET-Vorschau oder Orleans-Version: mit vereine-app und vereine-web gemeinsam heben, Dockerfile-Digests mitziehen.
7. **Konfiguration bleibt im Repo:** Skills, MCP-Server, Agent-Einstellungen liegen hier und werden eingecheckt.
8. **Conventional Commits auf Deutsch.** Keine Secrets, Verbindungszeichenfolgen, internen Hostnamen oder echten Daten committen (öffentliches Repo). Lokale Werte in User-Secrets (`UserSecretsId` vereine-api) oder `*.local.json`.

## Skills

Eigene Skills und eine Auswahl der Shiny-Skills liegen in `.claude/skills` (für Codex über den Symlink `.agents/skills`).

| Aufgabe | Skill |
|---|---|
| Orleans-Doku und Best Practices, Pflicht vor Backend-Änderungen | `orleans-docs` |
| Neues Grain nach Hausmuster | `add-grain` |
| Neuer HTTP-Endpunkt | `add-endpoint` |
| Build, gezielte Tests, Smoke-Test | `verify` |
| Shiny Mediator, Aspire-Orleans, Web-Module, DI, JSON | `shiny-mediator`, `shiny-aspire-orleans`, `shiny-web-hosting`, `shiny-di`, `shiny-serialization` |

Die Shiny-Skills kommen unverändert aus github.com/shinyorg/skills. Welche, steht in `.claude/skills/.shiny-skills-select`; aktualisieren mit `tools/sync-shiny-skills.sh [ref]`. Nie von Hand ändern, eigene Skills daneben anlegen.

Für Orleans-, ASP.NET-Core- und Aspire-APIs den Microsoft-Learn-MCP nutzen (`.mcp.json`), nicht raten.
