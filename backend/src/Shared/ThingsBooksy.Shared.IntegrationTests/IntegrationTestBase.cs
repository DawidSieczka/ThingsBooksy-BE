using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace ThingsBooksy.Shared.IntegrationTests;

[Collection("IntegrationTestCollection")]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly ThingsBooksyWebAppFactory Factory;

    protected IntegrationTestBase(ThingsBooksyWebAppFactory factory)
    {
        Factory = factory;
    }

    public async Task InitializeAsync()
    {
        await Factory.ResetDatabaseAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    protected IServiceScope CreateScope() => Factory.Services.CreateScope();
}
