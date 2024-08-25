using Confluent.Kafka;

namespace consumer;

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var kafkaConfig = new ConsumerConfig()
        {
            BootstrapServers = "localhost:19093",
            GroupId = "demo-consumer-group",
        };

        using var consumer = new ConsumerBuilder<Null, string>(kafkaConfig).Build();
        consumer.Subscribe("demo");
        while (!stoppingToken.IsCancellationRequested)
        {
            var cr = consumer.Consume(stoppingToken);
            _logger.LogInformation("Message received: {Message}", cr.Message.Value);
        }

        await Task.CompletedTask;
    }
}
