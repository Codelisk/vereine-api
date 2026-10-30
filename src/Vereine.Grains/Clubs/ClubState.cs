namespace Vereine.Grains.Clubs;

/// <summary>Persistierter Zustand eines Vereins. Eigener Typ, nicht das Contract-DTO.</summary>
[GenerateSerializer]
[Alias("vereine.club-state")]
public sealed record ClubState
{
    [Id(0)]
    public string Name { get; init; } = "";

    [Id(1)]
    public DateTimeOffset CreatedAt { get; init; }
}
