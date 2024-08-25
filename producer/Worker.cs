using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry;
using Confluent.SchemaRegistry.Serdes;

namespace producer;

public class MyCoolMessage
{
    public string Message { get; set; } = string.Empty;
    public DateTime CurrentTime { get; set; }
}

public class Worker : BackgroundService
{
    private readonly ILogger<Worker> _logger;

    public Worker(ILogger<Worker> logger)
    {
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var kafkaConfig = new ProducerConfig()
        {
            BootstrapServers = "localhost:19092,localhost:19093,localhost:19094"
        };

        var schemaRegistryConfig = new SchemaRegistryConfig()
        {
            Url = "http://localhost:8081"
        };

        using var schemaRegistry = new CachedSchemaRegistryClient(schemaRegistryConfig);
        using var producer = new ProducerBuilder<string, MyCoolMessage>(kafkaConfig)
            .SetValueSerializer(new JsonSerializer<MyCoolMessage>(schemaRegistry))
            .Build();
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            var _ = await producer.ProduceAsync(
                "demo-objects",
                new Message<string, MyCoolMessage>
                {
                    Value = new MyCoolMessage()
                    {
                        CurrentTime = DateTime.Now,
                        Message = "Hello World!"
                    }
                },
                stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }
}
