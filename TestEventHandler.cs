using MassTransit;

namespace MassTransitActiveMQReconnectTest;

public class TestEventHandler : IConsumer<TestEvent>
{
    public Task Consume(ConsumeContext<TestEvent> context)
    {
        return Task.CompletedTask;
    }
}

public class TestEventHandlerConsumerDefinition : ConsumerDefinition<TestEventHandler>
{
    public TestEventHandlerConsumerDefinition()
    {
        EndpointName = "TestEvent";
    }
}

public class TestEvent
{
}