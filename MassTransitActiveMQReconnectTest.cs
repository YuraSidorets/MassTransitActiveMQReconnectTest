using FluentAssertions;
using FluentAssertions.Extensions;
using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using System.Threading.Channels;
using Xunit.Abstractions;

namespace MassTransitActiveMQReconnectTest;

public class MassTransitActiveMQReconnectTest : IClassFixture<ActiveMqContainerFixture>
{
    private readonly ActiveMqContainerFixture _activeMqFixture;
    private readonly ITestOutputHelper _testOutputHelper;

    public MassTransitActiveMQReconnectTest(ActiveMqContainerFixture activeMqFixture, ITestOutputHelper testOutputHelper)
    {
        _activeMqFixture = activeMqFixture;
        _testOutputHelper = testOutputHelper;
    }

    [Fact]
    public async Task RequestResponse_ChannelClosedException()
    {
        // arrange
        var activeMqContainer = await _activeMqFixture.ActiveMqContainer;
        var testApp = new TestApp(_testOutputHelper, activeMqContainer.Hostname, activeMqContainer.GetMappedPublicPort(61616));
        testApp.RunAsync();

        var eventBus = testApp.TestHost.Services.GetService<IBus>();
        var testRequest = new TestRequest { Message = "test-message" };

        // act && assert

        // make sure response can be received before ActiveMQ container restart
        var requestClient = eventBus.CreateRequestClient<TestRequest>(new Uri("queue:TestCommand"), RequestTimeout.Default);
        var response = await requestClient.GetResponse<TestRequest>(testRequest, timeout: RequestTimeout.Default).ConfigureAwait(false);

        response.Message.Message.Should().Be(testRequest.Message);

        // restart ActiveMQ 
        await activeMqContainer.StopAsync();
        await activeMqContainer.StartAsync();

        // wait for MassTransit to reconnect
        await Task.Delay(10.Seconds());

        // send new request and receive RequestFault with ChannelClosedException as inner
        var requestClient2 = eventBus.CreateRequestClient<TestRequest>(new Uri("queue:TestCommand"), RequestTimeout.Default);
        var getResponseAction = async () => await requestClient2.GetResponse<TestRequest>(testRequest, timeout: RequestTimeout.Default).ConfigureAwait(false);

        var result = await getResponseAction.Should().ThrowAsync<RequestException>().ConfigureAwait(false);
        result.WithInnerException(typeof(ChannelClosedException));

        await testApp.DisposeAsync();
    }

    public ValueTask DisposeAsync()
    {
        return _activeMqFixture.DisposeAsync();
    }
}