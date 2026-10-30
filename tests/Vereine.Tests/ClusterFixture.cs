using Microsoft.Extensions.DependencyInjection;
using Orleans.TestingHost;
using Vereine.Grains;

namespace Vereine.Tests;

/// <summary>
/// Ein Test-Cluster je Testklasse, Speicher im RAM. Client und Silo sprechen über das Netzwerk,
/// Contracts und Ausnahmen gehen also wirklich durch die Orleans-Serialisierung.
/// </summary>
public sealed class ClusterFixture : IAsyncLifetime
{
    public TestCluster Cluster { get; } = new TestClusterBuilder()
        .AddSiloBuilderConfigurator<SiloConfigurator>()
        .Build();

    public ValueTask InitializeAsync() => new(Cluster.DeployAsync());

    public async ValueTask DisposeAsync() => await Cluster.DisposeAsync();

    private sealed class SiloConfigurator : ISiloConfigurator
    {
        public void Configure(ISiloBuilder siloBuilder) => siloBuilder
            .AddMemoryGrainStorage(GrainStorage.Default)
            .ConfigureServices(services => services.AddSingleton(TimeProvider.System));
    }
}
