---
name: add-endpoint
description: Neuen HTTP-Endpunkt in vereine-api anlegen, als Shiny-Mediator-Handler mit [MediatorHttpGet/Post], der ein Grain aufruft. Nutzen, wenn App oder Web eine neue Operation brauchen.
---

# Neuer Endpunkt

Referenz: `src/Vereine.Api/Clubs/` (`ClubContracts.cs`, `ClubHandlers.cs`). Details zu Shiny Mediator: Skill `shiny-mediator`.

1. **Contracts** in `src/Vereine.Api/<Domäne>/<Domäne>Contracts.cs`: Request als `record … : IRequest<TResponse>` (oder `ICommand`), Response als `record` mit `From(<GrainDetails>)`. HTTP-Contracts sind nicht die Grain-Contracts.
2. **Handler** `[MediatorScoped] public sealed partial class …Handler(IGrainFactory grains)`, Methode mit `[MediatorHttpPost("/<pfad>", OperationId = "<Name>")]` bzw. `[MediatorHttpGet("/<pfad>/{Id}", …)]`. Route-Parameter heißen wie die Record-Eigenschaft. Der Handler ruft genau ein Grain und mappt, keine Fachlogik, kein Zustand.
3. **JSON:** jeden neuen Request- und Response-Typ in `ApiJsonContext` eintragen (`[JsonSerializable]`). Shiny Mediator serialisiert AOT-strikt und findet sonst den Typ zur Laufzeit nicht.
4. **Fehler:** nichts abfangen. `RequestRejectedException` aus dem Grain wird über `ProblemDetailsModule` zu 400/404/409.
5. **OperationId** ist Vertrag: daraus entstehen die Typen in vereine-web (`npm run api:spec && npm run api:types`) und die Mediator-Contracts in vereine-app. Nicht umbenennen, ohne beide nachzuziehen.
6. **Prüfen:** Skill `verify`, dann `curl` gegen `dotnet run --project src/Vereine.Api` (Port 5610) und `/openapi/v1.json` ansehen.
