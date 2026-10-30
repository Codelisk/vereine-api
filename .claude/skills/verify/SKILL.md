---
name: verify
description: Build ohne Warnungen, betroffene Tests und Smoke-Test der API in einem Rutsch. Nutzen vor jedem Commit in vereine-api.
---

# Prüfen

```bash
source /srv/business/vereineapp/dev/env.sh                       # RC3-SDK, nie ~/.dotnet
dotnet build Vereine.Api.slnx -c Debug --nologo -v q             # 0 Warnungen, 0 Fehler (Warnungen sind Fehler)
dotnet test --project tests/Vereine.Tests --no-build --filter-class "Vereine.Tests.<Domäne>.<Name>Tests"
```

Tests nur gezielt (Workspace-Regel), die ganze Suite nur auf Anweisung des Owners. `dotnet test` läuft im MTP-Modus (`global.json`), VSTest-Flags gelten nicht; Filter: `--filter-class`, `--filter-method`, `--filter-namespace`.

Smoke-Test ohne Aspire (Speicher im RAM, Port 5610):

```bash
dotnet run --project src/Vereine.Api --no-build --launch-profile http &
curl -s localhost:5610/alive
curl -s localhost:5610/openapi/v1.json | head
```

Voller Stack mit Postgres und Webseite: `dotnet run --project src/Vereine.AppHost` (Docker nötig). Beenden mit Strg+C; nach `kill` räumt Aspire Container und Kindprozesse erst nach etwa 30 Sekunden ab.
