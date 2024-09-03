using Confluent.Kafka;
using KafkaFlow;
using KafkaFlow.Producers;
using KafkaFlow.Serializer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var brokers = builder.Configuration.GetSection("Kafka:BootstrapServers").Get<string[]>();
var topic = builder.Configuration.GetSection("Kafka:Topic").Get<string>();
const string ProducerName = "demo-monthly-invoice";

builder.Services.AddKafka(k => k
    .UseConsoleLog()
    .AddCluster(c => c
        .WithBrokers(brokers)
        .CreateTopicIfNotExists(
            topicName: topic,
            numberOfPartitions: 3,
            replicationFactor: 3)
        .AddProducer(ProducerName, p => p
            .DefaultTopic(topic)
            .WithCompression(CompressionType.Gzip)
            .AddMiddlewares(m => m
                .AddSerializer<NewtonsoftJsonSerializer>()
            )
        )
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();


app.MapGet("/demo", async (IProducerAccessor producer) =>
{
    await producer[ProducerName].ProduceAsync("demo", new DemoMessage
    {
        PolicyNumber = "NCI0001Q",
        InvoiceYearMonth = DateTime.Now.AddMonths(-1)
    });
})
.WithName("DemoMonthlyInvoice")
.WithOpenApi();

app.Run();

class DemoMessage
{
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime InvoiceYearMonth { get; set; }
}