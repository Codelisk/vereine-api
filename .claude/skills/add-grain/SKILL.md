---
name: add-grain
description: Neues Orleans-Grain nach Hausmuster anlegen, also Interface und Contracts in Abstractions, Implementierung und State in Grains, Cluster-Test in Tests. Nutzen, wenn ein neues Aggregat entstehen soll (Mitglied, Mannschaft, Beitrag ...).
---

# Neues Grain anlegen

Referenz: `IClubGrain` / `ClubGrain` / `ClubGrainTests`. Vorher Skill `orleans-docs` (Zuschnitt prüfen).

1. **Interface und Contracts** in `src/Vereine.Abstractions/<Domäne>/`:
   - `I<Name>Grain : IGrainWithGuidKey` (oder String-Key; Schlüssel und Format im XML-Doc begründen).
   - Methoden geben `ValueTask`/`ValueTask<T>` zurück, Lesemethoden `[ReadOnly]`.
   - `[Alias("vereine.<name>")]` auf Interface und Typen, `[Alias("<methode>")]` auf Methoden.
   - Übertragene Typen: `record` mit `[GenerateSerializer]`, `[Immutable]` wenn unveränderlich, explizite `[Id(n)]` (nur anhängen).
   - Fachregeln, die auch API oder App kennen müssen (Längen, Formate), als statische Regelklasse daneben (Muster `ClubName`).
   - Fachliche Ablehnung: `RequestRejectedException` mit `RejectionKind` (wird 400/404/409). Keine eigenen Ausnahmetypen pro Fall.
2. **State und Implementierung** in `src/Vereine.Grains/<Domäne>/`:
   - State als eigener `[GenerateSerializer]`-Record, nicht das Contract-DTO.
   - Klasse `sealed`, Primary Constructor mit `[PersistentState("<name>", GrainStorage.Default)] IPersistentState<TState>`.
   - Zeit nur über `TimeProvider` (im Silo registriert), nie `DateTime.Now`.
   - Nach jeder Änderung `await state.WriteStateAsync()`.
   - Das Projekt ist `IsAotCompatible`: keine Reflection, die der Analyzer anmeckert.
3. **Cluster-Test** in `tests/Vereine.Tests/<Domäne>/<Name>GrainTests.cs` mit `ClusterFixture`: mindestens Happy Path, jede Ablehnung, Idempotenz, Schlüssel-Isolation.
4. **Prüfen:** Skill `verify`.
