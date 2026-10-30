---
name: orleans-docs
description: Offizielle Microsoft-Orleans-Doku und Best Practices nachschlagen (Microsoft-Learn-MCP, dotnet/orleans). Pflicht vor JEDER Backend-Änderung (Workspace-Regel „Orleans zuerst“) und bei jeder Unsicherheit über Grains, Persistenz, Reminder, Streams, Timer, Serialisierung oder TestingHost, bevor Code entsteht.
---

# Orleans zuerst

Regel des Owners: Zustand und Fachlogik leben so weit wie möglich in Orleans. Vor jeder Backend-Änderung zuerst klären, wie Orleans das löst, und das gegen die offizielle Doku prüfen. Nie aus dem Gedächtnis raten.

## Ablauf

1. **Passt es in ein Grain?** Ein Grain = ein Konsistenzbereich (ein Verein, ein Mitglied, eine Mannschaft, ein Nummernkreis). Wiederkehrendes = Reminder am zuständigen Grain. Benachrichtigen = Streams oder Grain-Observer. Kurzlebiges Takten = Grain-Timer.
2. **Doku prüfen** mit dem Microsoft-Learn-MCP (`.mcp.json`, Codex über `.codex/config.toml`):
   - Konzept: `microsoft_docs_search`, dann `microsoft_docs_fetch` für die ganze Seite.
   - API-Signatur: `microsoft_code_sample_search` (language: csharp).
3. **Versionsstand beachten:** Orleans 10.4.x auf .NET 11 RC3 (`Directory.Packages.props`). Doku-Beispiele zeigen oft ältere Stände. Letzte Instanz: Quellcode und Release Notes in [dotnet/orleans](https://github.com/dotnet/orleans) (`gh release view v10.4.2 --repo dotnet/orleans`).
4. **Abweichung von Orleans** (eigene Tabelle, eigener Hintergrunddienst, Cache, Queue) nur mit ADR unter `docs/adr/`, das begründet, warum Orleans es nicht kann.

## Einstiegspunkte

| Thema | URL |
|---|---|
| Doku | https://learn.microsoft.com/dotnet/orleans/ |
| Best Practices (Pflicht) | https://learn.microsoft.com/dotnet/orleans/resources/best-practices |
| Grain-Persistenz | https://learn.microsoft.com/dotnet/orleans/grains/grain-persistence/ |
| Request-Scheduling, Reentrancy, `[ReadOnly]` | https://learn.microsoft.com/dotnet/orleans/grains/request-scheduling |
| Timer und Reminder | https://learn.microsoft.com/dotnet/orleans/grains/timers-and-reminders |
| Streams | https://learn.microsoft.com/dotnet/orleans/streaming/ |
| Serialisierung, `[GenerateSerializer]`, `[Id]`, `[Alias]` | https://learn.microsoft.com/dotnet/orleans/host/configuration-guide/serialization |
| Tests mit TestingHost | https://learn.microsoft.com/dotnet/orleans/implementation/testing |
| Samples | https://github.com/dotnet/samples/tree/main/orleans |

## Kern-Best-Practices

- **Grains sind nicht reentrant, Aufrufzyklen (A→B→A) blockieren.** Aufrufrichtungen azyklisch halten, reine Lesemethoden `[ReadOnly]`, `[Reentrant]` nur mit ADR.
- **At-most-once:** Wiederholen ist Sache des Aufrufers. Schreibende Aufrufe idempotent bauen (Id vom Aufrufer, Muster `IClubGrain.Create`).
- **Keine Flaschenhals-Grains** (zentrale Registry für alles). Last abschätzen, bevor ein „Verzeichnis-Grain“ entsteht.
- **Nicht geschwätzig:** eng gekoppelte Daten in ein Grain statt vieler kleiner Aufrufe.
- **Nie blockieren** (`.Result`, `.Wait()`, Locks, `Thread.Sleep`). Alles async, Fan-out mit `Task.WhenAll`.
- **`WriteStateAsync` nach jeder Zustandsänderung** und den Task zurückgeben. Storage-Fehler gehen an den Aufrufer, Provider wiederholen nicht. `InconsistentStateException` ist transient.
- **Speicher im RAM nur für Tests und `dotnet run` ohne AppHost.** Unter Aspire: Postgres über Shiny.Aspire.Orleans (ADR-0001).
- **Grain-Interfaces sind Cluster-API:** `[Id(n)]` nur anhängen, nie umwidmen. `[Alias]` auf Interfaces, Methoden und Typen setzen, damit Umbenennen nichts bricht.
- **Timer sind flüchtig, Reminder dauerhaft:** fachlich wiederkehrende Aufgaben (Beitragserinnerung, Saisonwechsel) immer als Reminder.
