using KafkaFlow;

namespace consumer;

class DemoMessage
{
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime InvoiceYearMonth { get; set; }
}

internal class DemoMessageHandler : IMessageHandler<DemoMessage>
{
    public Task Handle(IMessageContext context, DemoMessage message)
    {
        Console.WriteLine($"{message.PolicyNumber} - {message.InvoiceYearMonth}");

        return Task.CompletedTask;
    }
}

class DemoMessage2
{
    public int Version { get; set; }
    public string Message { get; set; } = string.Empty;
}

internal class DemoMessage2Handler : IMessageHandler<DemoMessage2>
{
    public Task Handle(IMessageContext context, DemoMessage2 message)
    {
        Console.WriteLine($"Version {message.Version} - {message.Message}");

        return Task.CompletedTask;
    }
}