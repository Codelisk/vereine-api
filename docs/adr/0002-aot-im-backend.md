# ADR 0002: AOT im Backend, soweit Orleans es zulässt

Status: angenommen, 2026-10-30

## Kontext

Der Owner hat AOT für App und Backend zur Pflicht gemacht, mit der Einschränkung: Wenn Orleans AOT nicht zulässt, darf das Backend ohne AOT laufen. Geprüft wurde Orleans 10.4.2 (neueste Version am 2026-10-30) auf .NET 11 RC3.

## Befund

- **Orleans-Silo unter NativeAOT wird nicht unterstützt.** dotnet/orleans#11375 (NativeAOT für den ganzen Silo) ist offen, der zugehörige PR dotnet/orleans#11379 nicht gemergt.
- **Manifest-Metadaten** (dotnet/orleans#11374): Ohne den Fix trimmt NativeAOT Assemblies und Typ-Metadaten weg, die Orleans zur Laufzeit per Name lädt. Im Hauptzweig gemergt, in keinem 10.4.x-Release.
- **Serializer-Kontexte** (Voraussetzung, damit der Orleans-Serializer ohne Reflection auskommt) sind mit dotnet/orleans#11382 im Hauptzweig, aber in keinem 10.4.x-Release.
- **Stand 2026-10-30:** 10.4.2 kommt vom Zweig `10.x` und enthält keinen der drei Fixes. Der Nightly-Feed von Orleans endet bei 9.2, eine Vorschau mit den Fixes gibt es nicht.
- **Ohne Fork, aber mit Umgehungen:** dotnet/orleans#11526 zeigt einen Silo mit 10.4 unter NativeAOT in Produktion. Er braucht `TrimmerRootAssembly`, einen ILLink-Descriptor, einen eigenen ServiceProvider für Codecs (die DI schließt unter NativeAOT keine offenen Generics über Werttypen) und rund 2.150 generierte rd.xml-Einträge, die aus privaten Orleans-Caches ausgelesen werden. Fehlende Einträge fallen erst zur Laufzeit auf, jede neue Grain-Methode und jedes Orleans-Update kann neue brauchen. Das baut Orleans-Interna nach und widerspricht der Regel, Orleans so zu nutzen, wie es gedacht ist. Für uns kein Weg, solange keine Sicherheitsvorgabe JIT verbietet.
- **Shiny Mediator:** Die generierten HTTP-Endpunkte rufen ASP.NET-APIs mit `RequiresDynamicCode` (`IL3050`). Auch ohne Orleans wäre die API heute nicht warnungsfrei AOT-fähig.

## Entscheidung

- **Bibliotheken sind AOT-tauglich:** `Vereine.Abstractions`, `Vereine.Grains` und `Vereine.ServiceDefaults` sind `IsAotCompatible`. Trim-, AOT- und Single-File-Analyzer laufen dort, Warnungen sind Fehler.
- **Der Host läuft mit JIT:** `Vereine.Api` schaltet die drei Analyzer ab (Begründung im csproj), der Container startet `dotnet Vereine.Api.dll`. `Vereine.AppHost` und `tests/Vereine.Tests` sind Entwicklungswerkzeuge und ohnehin JIT.
- **Neuer Code wird so geschrieben, als liefe er unter AOT:** JSON nur über source-generierte Kontexte (`ApiJsonContext`, `[ShinyJsonContext]`), keine Reflection, kein `MakeGenericType`, kein Assembly-Scan. Damit ist der Umstieg später eine Projekt-Einstellung und kein Umbau.

## Bedingung für den Umstieg

Wir warten, bis alle Punkte erfüllt sind:

- Ein Orleans-Release (kein Nightly, kein eigener Build) enthält die Manifest-Metadaten (#11374), die Serializer-Kontexte (#11382) und den Silo-Fix (#11379 für #11375).
- Orleans bestätigt, dass ein Silo mit Serializer-Kontexten ohne eigene Umgehungen für DI und rd.xml auskommt (offene Frage 3 in #11526).
- Shiny Mediator erzeugt AOT-sichere Endpunkte (kein `IL3050` mehr).

Dann:

1. Analyzer in `Vereine.Api` wieder einschalten, `PublishAot=true` setzen.
2. `dotnet publish -c Release` muss ohne Warnung durchlaufen; danach Smoke-Test des Containers (Grain anlegen, lesen, Reminder).
3. Dieses ADR auf „ersetzt“ setzen und das Ergebnis dokumentieren.

Bei jedem Orleans-Update die Release Notes auf diese Punkte prüfen, ebenso #11526 auf eine Antwort der Maintainer.
