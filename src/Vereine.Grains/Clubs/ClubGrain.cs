using Vereine.Abstractions;
using Vereine.Abstractions.Clubs;

namespace Vereine.Grains.Clubs;

public sealed class ClubGrain(
    [PersistentState("club", GrainStorage.Default)] IPersistentState<ClubState> state,
    TimeProvider time) : Grain, IClubGrain
{
    public async ValueTask<ClubDetails> Create(string name)
    {
        var normalized = ClubName.Normalize(name);
        if (state.RecordExists)
        {
            return state.State.Name == normalized
                ? Details()
                : throw new RequestRejectedException(RejectionKind.Conflict, "Unter dieser Id gibt es schon einen anderen Verein.");
        }

        state.State = new ClubState { Name = normalized, CreatedAt = time.GetUtcNow() };
        await state.WriteStateAsync();
        return Details();
    }

    public ValueTask<ClubDetails?> GetDetails() => ValueTask.FromResult(state.RecordExists ? Details() : null);

    public async ValueTask<ClubDetails> Rename(string name)
    {
        if (!state.RecordExists)
        {
            throw new RequestRejectedException(RejectionKind.NotFound, "Den Verein gibt es nicht.");
        }

        var normalized = ClubName.Normalize(name);
        if (state.State.Name != normalized)
        {
            state.State = state.State with { Name = normalized };
            await state.WriteStateAsync();
        }

        return Details();
    }

    private ClubDetails Details() => new(this.GetPrimaryKey(), state.State.Name, state.State.CreatedAt);
}
