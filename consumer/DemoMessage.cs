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