using Vereine.Abstractions;
using Vereine.Abstractions.Clubs;

namespace Vereine.Tests.Clubs;

public sealed class ClubGrainTests(ClusterFixture fixture) : IClassFixture<ClusterFixture>
{
    private IClubGrain Club(Guid id) => fixture.Cluster.GrainFactory.GetGrain<IClubGrain>(id);

    [Fact]
    public async Task Create_then_GetDetails_returns_trimmed_name()
    {
        var id = Guid.CreateVersion7();

        var created = await Club(id).Create("  SV Musterdorf  ");
        var loaded = await Club(id).GetDetails();

        Assert.Equal(new ClubDetails(id, "SV Musterdorf", created.CreatedAt), loaded);
    }

    [Fact]
    public async Task Create_is_idempotent_for_same_name()
    {
        var id = Guid.CreateVersion7();

        var first = await Club(id).Create("FC Beispiel");
        var second = await Club(id).Create("FC Beispiel");

        Assert.Equal(first, second);
    }

    [Fact]
    public async Task Create_with_other_name_for_taken_id_is_a_conflict()
    {
        var id = Guid.CreateVersion7();
        await Club(id).Create("FC Beispiel");

        var rejection = await Assert.ThrowsAsync<RequestRejectedException>(() => Club(id).Create("SC Anders").AsTask());

        Assert.Equal(RejectionKind.Conflict, rejection.Kind);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public async Task Create_without_name_is_invalid(string name)
    {
        var rejection = await Assert.ThrowsAsync<RequestRejectedException>(() => Club(Guid.CreateVersion7()).Create(name).AsTask());

        Assert.Equal(RejectionKind.Invalid, rejection.Kind);
        Assert.Equal("Der Verein braucht einen Namen.", rejection.Message);
    }

    [Fact]
    public async Task Create_with_too_long_name_is_invalid()
    {
        var name = new string('x', ClubName.MaxLength + 1);

        var rejection = await Assert.ThrowsAsync<RequestRejectedException>(() => Club(Guid.CreateVersion7()).Create(name).AsTask());

        Assert.Equal(RejectionKind.Invalid, rejection.Kind);
    }

    [Fact]
    public async Task GetDetails_of_unknown_club_returns_null()
    {
        Assert.Null(await Club(Guid.CreateVersion7()).GetDetails());
    }

    [Fact]
    public async Task Rename_changes_only_this_club()
    {
        var renamed = Guid.CreateVersion7();
        var other = Guid.CreateVersion7();
        await Club(renamed).Create("FC Alt");
        await Club(other).Create("FC Alt");

        var result = await Club(renamed).Rename("FC Neu");

        Assert.Equal("FC Neu", result.Name);
        Assert.Equal("FC Alt", (await Club(other).GetDetails())?.Name);
    }

    [Fact]
    public async Task Rename_unknown_club_is_not_found()
    {
        var rejection = await Assert.ThrowsAsync<RequestRejectedException>(() => Club(Guid.CreateVersion7()).Rename("FC Neu").AsTask());

        Assert.Equal(RejectionKind.NotFound, rejection.Kind);
    }
}
