using Vereine.Abstractions;
using Vereine.Abstractions.Clubs;

namespace Vereine.Api.Clubs;

[MediatorScoped]
public sealed partial class CreateClubHandler(IGrainFactory grains) : IRequestHandler<CreateClubRequest, ClubResponse>
{
    [MediatorHttpPost("/clubs", OperationId = "CreateClub")]
    public async Task<ClubResponse> Handle(CreateClubRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var club = await grains.GetGrain<IClubGrain>(request.Id).Create(request.Name);
        return ClubResponse.From(club);
    }
}

[MediatorScoped]
public sealed partial class GetClubHandler(IGrainFactory grains) : IRequestHandler<GetClubRequest, ClubResponse>
{
    [MediatorHttpGet("/clubs/{Id}", OperationId = "GetClub")]
    public async Task<ClubResponse> Handle(GetClubRequest request, IMediatorContext context, CancellationToken cancellationToken)
    {
        var club = await grains.GetGrain<IClubGrain>(request.Id).GetDetails()
            ?? throw new RequestRejectedException(RejectionKind.NotFound, "Den Verein gibt es nicht.");
        return ClubResponse.From(club);
    }
}
