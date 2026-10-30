using Shiny;
using Vereine.Api.Infrastructure;
using Vereine.ServiceDefaults;

var builder = WebApplication.CreateBuilder(args);
builder.AddServiceDefaults();

// Reihenfolge = Reihenfolge der Middleware in Use().
builder.AddInfrastructureModules(
    new ProblemDetailsModule(),
    new OrleansModule(),
    new MediatorModule(),
    new OpenApiModule());

var app = builder.Build();
app.UseInfrastructureModules();
app.MapDefaultEndpoints();
app.Run();
