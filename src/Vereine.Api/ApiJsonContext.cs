using System.Text.Json;
using System.Text.Json.Serialization;
using Shiny;
using Vereine.Api.Clubs;

namespace Vereine.Api;

/// <summary>
/// Quellgenerierte JSON-Metadaten aller HTTP-Contracts. Shiny Mediator serialisiert AOT-strikt und
/// findet einen Typ nur, wenn er hier steht: neue Contracts immer eintragen.
/// </summary>
[ShinyJsonContext]
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web)]
[JsonSerializable(typeof(CreateClubRequest))]
[JsonSerializable(typeof(GetClubRequest))]
[JsonSerializable(typeof(ClubResponse))]
internal sealed partial class ApiJsonContext : JsonSerializerContext;
