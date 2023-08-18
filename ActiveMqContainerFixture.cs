using DotNet.Testcontainers.Builders;

namespace MassTransitActiveMQReconnectTest;

public sealed class ActiveMqContainerFixture : IAsyncDisposable
{
    public readonly AsyncLazy<DotNet.Testcontainers.Containers.IContainer> ActiveMqContainer = new(async () =>
    {
        var container = new ContainerBuilder()
          .WithImage("rmohr/activemq:latest")
          .WithPortBinding(61616, 61616)
          .WithWaitStrategy(Wait.ForUnixContainer().UntilPortIsAvailable(61616))
          .Build();
        await container.StartAsync();
        return container;
    });

    public async ValueTask DisposeAsync()
    {
        await (await ActiveMqContainer).DisposeAsync().ConfigureAwait(false);
    }
}
