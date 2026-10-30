using System.Diagnostics;
using Shiny;
using Vereine.Abstractions;

namespace Vereine.Api.Infrastructure;

/// <summary>Fehler als ProblemDetails (RFC 9457). Fachliche Ablehnungen der Grains werden 400, 404 oder 409.</summary>
public sealed class ProblemDetailsModule : IWebModule
{
    public void Add(WebApplicationBuilder builder)
    {
        builder.Services.AddProblemDetails();
        builder.Services.AddExceptionHandler<RejectionHandler>();
    }

    public void Use(WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages();
    }

    private sealed class RejectionHandler(IProblemDetailsService problemDetails) : Microsoft.AspNetCore.Diagnostics.IExceptionHandler
    {
        public ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
        {
            if (exception is not RequestRejectedException rejection)
            {
                return ValueTask.FromResult(false);
            }

            httpContext.Response.StatusCode = rejection.Kind switch
            {
                RejectionKind.Invalid => StatusCodes.Status400BadRequest,
                RejectionKind.Conflict => StatusCodes.Status409Conflict,
                RejectionKind.NotFound => StatusCodes.Status404NotFound,
                _ => throw new UnreachableException($"Unbekannte Ablehnung {rejection.Kind}."),
            };

            return problemDetails.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                Exception = exception,
                ProblemDetails = { Detail = rejection.Message },
            });
        }
    }
}
