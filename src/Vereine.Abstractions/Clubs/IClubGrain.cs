using Orleans.Concurrency;

namespace Vereine.Abstractions.Clubs;

/// <summary>
/// Ein Verein. Schlüssel ist die Vereins-Id, die der Aufrufer beim Anlegen vergibt (Guid v7):
/// damit ist das Anlegen idempotent und ein Retry erzeugt keinen zweiten Verein.
/// </summary>
[Alias("vereine.club")]
public interface IClubGrain : IGrainWithGuidKey
{
    /// <summary>
    /// Legt den Verein an. Derselbe Aufruf ein zweites Mal liefert den bestehenden Stand;
    /// ein anderer Name für eine schon vergebene Id wird als <see cref="RejectionKind.Conflict"/> abgelehnt.
    /// </summary>
    [Alias("create")]
    ValueTask<ClubDetails> Create(string name);

    /// <summary>Stand des Vereins oder <see langword="null"/>, wenn er nicht angelegt ist.</summary>
    [ReadOnly]
    [Alias("get")]
    ValueTask<ClubDetails?> GetDetails();

    /// <summary>Benennt den Verein um.</summary>
    [Alias("rename")]
    ValueTask<ClubDetails> Rename(string name);
}

[GenerateSerializer]
[Immutable]
[Alias("vereine.club-details")]
public sealed record ClubDetails(
    [property: Id(0)] Guid Id,
    [property: Id(1)] string Name,
    [property: Id(2)] DateTimeOffset CreatedAt);
