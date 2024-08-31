using Confluent.Kafka;
using Confluent.Kafka.SyncOverAsync;
using Confluent.SchemaRegistry.Serdes;

namespace consumer2;

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
        var kafkaConfig = new ConsumerConfig()
        {
            BootstrapServers = "localhost:19092,localhost:19093,localhost:19094",
            GroupId = "demo-consumer-group-2",
        };

        using var consumer = new ConsumerBuilder<string, MyCoolMessage>(kafkaConfig)
            .SetValueDeserializer(new JsonDeserializer<MyCoolMessage>().AsSyncOverAsync())
            .SetErrorHandler((_, e) => _logger.LogError(e.Reason))
            .SetKeyDeserializer(Deserializers.Utf8)
            .Build();

        consumer.Subscribe("demo-objects");
        while (!stoppingToken.IsCancellationRequested)
        {
            var cr = consumer.Consume(stoppingToken);
            var myCoolMessage = cr.Message.Value;
            Console.WriteLine($"{myCoolMessage.CurrentTime} - {myCoolMessage.Message}");
        }

        await Task.CompletedTask;
    }
}

