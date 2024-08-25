using Confluent.Kafka;

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
            BootstrapServers = "localhost:19092"
        };

        using var producer = new ProducerBuilder<Null, string>(kafkaConfig).Build();
        while (!stoppingToken.IsCancellationRequested)
        {
            if (_logger.IsEnabled(LogLevel.Information))
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
            }

            var _ = await producer.ProduceAsync(
                "demo",
                new Message<Null, string>
                {
                    Value = $"Hello World {DateTimeOffset.Now}"
                },
                stoppingToken);

            await Task.Delay(5000, stoppingToken);
        }
    }
}
