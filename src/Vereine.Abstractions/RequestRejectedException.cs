namespace Vereine.Abstractions;

/// <summary>Art der fachlichen Ablehnung. Die API leitet daraus den HTTP-Status ab.</summary>
public enum RejectionKind
{
    /// <summary>Eingabe verletzt eine fachliche Regel (400).</summary>
    Invalid,

    /// <summary>Widerspricht dem bestehenden Zustand (409).</summary>
    Conflict,

    /// <summary>Das Aggregat gibt es nicht (404).</summary>
    NotFound,
}

/// <summary>
/// Fachliche Ablehnung eines Grain-Aufrufs. Grains werfen sie für Regelverstöße, die API macht daraus
/// ProblemDetails. Alle anderen Ausnahmen gelten als Fehler (500).
/// </summary>
[GenerateSerializer]
[Alias("vereine.request-rejected")]
public sealed class RequestRejectedException(RejectionKind kind, string message) : Exception(message)
{
    [Id(0)]
    public RejectionKind Kind { get; } = kind;
}
