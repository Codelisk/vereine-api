using Vereine.Abstractions.Clubs;

namespace Vereine.Api.Clubs;

/// <summary>Verein anlegen. Die Id vergibt der Aufrufer (Guid v7), dadurch ist der Aufruf wiederholbar.</summary>
public sealed record CreateClubRequest(Guid Id, string Name) : IRequest<ClubResponse>;

public sealed record GetClubRequest(Guid Id) : IRequest<ClubResponse>;

public sealed record ClubResponse(Guid Id, string Name, DateTimeOffset CreatedAt)
{
    public static ClubResponse From(ClubDetails club) => new(club.Id, club.Name, club.CreatedAt);
}
