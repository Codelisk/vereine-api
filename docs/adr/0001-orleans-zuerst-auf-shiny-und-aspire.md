# ADR 0001: Orleans zuerst, auf Shiny und Aspire

Status: angenommen, 2026-10-30

## Kontext

Der Owner will das Backend so weit wie möglich auf Microsoft Orleans bauen. Vereinsdaten zerfallen natürlich in Einheiten mit eigener Konsistenz: ein Verein, ein Mitglied, eine Mannschaft, eine Saison. Wiederkehrende Aufgaben (Beitragserinnerung, Saisonwechsel, Fristen) sind typisch. Beides passt zu Grains mit Persistenz und Remindern. Basis für die Projektstruktur ist das Shiny-Template `shinyaspnet`, danach auf .NET 11 RC3, neueste Pakete und die Regeln dieses Repos gebracht.

## Entscheidung

- **Fachlogik und Zustand in Grains** (`src/Vereine.Grains`), Contracts in `src/Vereine.Abstractions`. Die API (`src/Vereine.Api`) ist ein dünner Rand: Shiny-Mediator-Handler mit `[MediatorHttpGet/Post]` rufen genau ein Grain und mappen auf HTTP-Contracts.
- **Silo und API in einem Prozess** (co-hosted). Kein separater Orleans-Client, keine zweite Bereitstellung, solange ein Prozess reicht.
- **Persistenz, Clustering und Reminder über ADO.NET auf Postgres**, verdrahtet mit Shiny.Aspire.Orleans: Der AppHost beschreibt den Cluster (`AddOrleans(...).WithClustering/WithGrainStorage/WithReminders`), `WithDatabaseSetup` legt das Orleans-Schema an, sobald die Datenbank bereit ist; im Silo liest `UseAdoNet()` die von Aspire injizierte Konfiguration.
- **Ohne AppHost** (`dotnet run`, Tests) laufen Clustering, Speicher und Reminder im RAM. So startet die API ohne Docker.
- **Fachliche Ablehnungen** als `RequestRejectedException` mit `RejectionKind`. Sie ist serialisierbar, überquert also Silo-Grenzen, und wird in der API zu Problem Details (400/404/409).
- **Endpunkte über `IWebModule`** (Shiny.AspNet.WebHosting): eine Klasse je Belang, explizit registriert, ohne Assembly-Scan.
- **Eigene Infrastruktur neben Orleans** (eigene Tabellen, Caches, Hintergrunddienste, Queues) nur mit eigenem ADR, das begründet, warum Orleans es nicht kann.

## Folgen

- Das Postgres im AppHost hat **kein Daten-Volume**. Die Orleans-Skripte für Postgres sind nicht idempotent (`CREATE TABLE` ohne `IF NOT EXISTS`), ein zweiter Lauf von `WithDatabaseSetup` auf einer bestehenden Datenbank scheitert. Jeder Start beginnt leer. Das ist für die Entwicklung gewollt.
- **Offen für den Betrieb:** Wie das Schema in Produktion angelegt und bei neuen Orleans-Versionen migriert wird. Das entscheidet ein eigenes ADR, bevor es eine Produktionsumgebung gibt.
- Grain-Interfaces sind ab dem ersten Deploy Cluster-API: `[Id]` nur anhängen, `[Alias]` überall setzen.
- Keine Abwärtskompatibilität (Workspace-Regel): Bis zum ersten echten Betrieb dürfen Grain-Zustände ohne Migration brechen.
