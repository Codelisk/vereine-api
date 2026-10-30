# vereine-api: Einstieg für Codex

Backend von Vereineapp: .NET 11, Microsoft Orleans, Aspire, auf Shiny. Lies zuerst `CLAUDE.md` vollständig: Kommandos, Aufbau und harte Regeln (Orleans zuerst, AOT soweit Orleans es zulässt, Grain-Interfaces als Cluster-API) gelten unverändert. Die Regeln aus `/srv/business/vereineapp/CLAUDE.md` gelten ebenfalls.

```bash
source /srv/business/vereineapp/dev/env.sh
dotnet build Vereine.Api.slnx                                    # vor jedem Commit, Warnungen sind Fehler
dotnet test --project tests/Vereine.Tests --filter-class "<Klasse>"   # nur betroffene Tests
```

Skills (`orleans-docs`, `add-grain`, `add-endpoint`, `verify`, Shiny) liegen unter `.agents/skills`. Orleans-, ASP.NET-Core- und Aspire-Doku über den MCP-Server `microsoft-learn` (`.codex/config.toml`).
