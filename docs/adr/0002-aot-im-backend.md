# ADR 0002: AOT im Backend, soweit Orleans es zulässt

Status: angenommen, 2026-10-30

## Kontext

Der Owner hat AOT für App und Backend zur Pflicht gemacht, mit der Einschränkung: Wenn Orleans AOT nicht zulässt, darf das Backend ohne AOT laufen. Geprüft wurde Orleans 10.4.2 (neueste Version am 2026-10-30) auf .NET 11 RC3.

## Befund

- **Orleans-Silo unter NativeAOT wird nicht unterstützt.** dotnet/orleans#11375 (NativeAOT für den ganzen Silo) ist offen, der zugehörige PR dotnet/orleans#11379 nicht gemergt.
- **Serializer-Kontexte** (Voraussetzung, damit der Orleans-Serializer ohne Reflection auskommt) sind mit dotnet/orleans#11382 im Hauptzweig, aber in keinem 10.4.x-Release.
- Wer Orleans heute unter NativeAOT betreibt, braucht laut dotnet/orleans#11526 rd.xml-Dateien und einen eigenen Codec-Provider. Das ist nicht wartbar und gegen die Regel, Orleans so zu nutzen, wie es gedacht ist.
- **Shiny Mediator:** Die generierten HTTP-Endpunkte rufen ASP.NET-APIs mit `RequiresDynamicCode` (`IL3050`). Auch ohne Orleans wäre die API heute nicht warnungsfrei AOT-fähig.

## Entscheidung

- **Bibliotheken sind AOT-tauglich:** `Vereine.Abstractions`, `Vereine.Grains` und `Vereine.ServiceDefaults` sind `IsAotCompatible`. Trim-, AOT- und Single-File-Analyzer laufen dort, Warnungen sind Fehler.
- **Der Host läuft mit JIT:** `Vereine.Api` schaltet die drei Analyzer ab (Begründung im csproj), der Container startet `dotnet Vereine.Api.dll`. `Vereine.AppHost` und `tests/Vereine.Tests` sind Entwicklungswerkzeuge und ohnehin JIT.
- **Neuer Code wird so geschrieben, als liefe er unter AOT:** JSON nur über source-generierte Kontexte (`ApiJsonContext`, `[ShinyJsonContext]`), keine Reflection, kein `MakeGenericType`, kein Assembly-Scan. Damit ist der Umstieg später eine Projekt-Einstellung und kein Umbau.

## Bedingung für den Umstieg

Sobald ein Orleans-Release Serializer-Kontexte (#11382) und den Silo-Fix (#11375) enthält und Shiny Mediator AOT-sichere Endpunkte erzeugt:

1. Analyzer in `Vereine.Api` wieder einschalten, `PublishAot=true` setzen.
2. `dotnet publish -c Release` muss ohne Warnung durchlaufen; danach Smoke-Test des Containers (Grain anlegen, lesen, Reminder).
3. Dieses ADR auf „ersetzt“ setzen und das Ergebnis dokumentieren.

Bei jedem Orleans-Update die Release Notes auf diese Punkte prüfen.
