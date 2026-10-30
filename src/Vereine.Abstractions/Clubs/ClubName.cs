namespace Vereine.Abstractions.Clubs;

/// <summary>Regel für Vereinsnamen: getrimmt, nicht leer, höchstens <see cref="MaxLength"/> Zeichen.</summary>
public static class ClubName
{
    public const int MaxLength = 100;

    public static string Normalize(string? name)
    {
        var trimmed = name?.Trim() ?? "";
        return trimmed switch
        {
            "" => throw new RequestRejectedException(RejectionKind.Invalid, "Der Verein braucht einen Namen."),
            { Length: > MaxLength } => throw new RequestRejectedException(
                RejectionKind.Invalid, $"Der Name darf höchstens {MaxLength} Zeichen haben."),
            _ => trimmed,
        };
    }
}
