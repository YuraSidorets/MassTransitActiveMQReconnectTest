using MassTransit;

namespace MassTransitActiveMQReconnectTest;

public class TestCommandHandler : IConsumer<TestRequest>
{
    public Task Consume(ConsumeContext<TestRequest> context)
    {
        return context.RespondAsync(context.Message);
    }
}

public class TestCommandHandlerConsumerDefinition : ConsumerDefinition<TestCommandHandler>
{
    public TestCommandHandlerConsumerDefinition()
    {
        EndpointName = "TestCommand";
    }
}

public class TestRequest
{
    public string Message { get; set; }
}