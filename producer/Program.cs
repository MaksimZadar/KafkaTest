using Confluent.Kafka;
using KafkaFlow;
using KafkaFlow.Producers;
using Microsoft.AspNetCore.Mvc;

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
        .WithSchemaRegistry(config => config.Url = "localhost:8081")
        .CreateTopicIfNotExists(
            topicName: topic,
            numberOfPartitions: 3,
            replicationFactor: 3)
        .AddProducer(ProducerName, p => p
            .DefaultTopic(topic)
            .WithCompression(CompressionType.Gzip)
            .AddMiddlewares(m => m
                .AddSchemaRegistryJsonSerializer<DemoMessage>()
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


app.MapGet("/demo", async (IProducerAccessor producer, [FromQuery] string policyNumber = "NCI0001Q") =>
{
    await producer[ProducerName].ProduceAsync("demo", new DemoMessage
    {
        PolicyNumber = policyNumber,
        InvoiceYearMonth = DateTime.Now.AddMonths(-1)
    });
})
.WithName("DemoMonthlyInvoice")
.WithOpenApi();

app.MapGet("/demo2", async (IProducerAccessor producer, [FromQuery] string message = "Hello World") =>
{
    await producer[ProducerName].ProduceAsync("demo2", "1", new DemoMessage2
    {
        Message = message,
        Version = 1
    });
})
.WithName("Demo2")
.WithDescription("This will test a different message type and see how the consumer handles it.")
.WithOpenApi();

app.Run();

class DemoMessage
{
    public string PolicyNumber { get; set; } = string.Empty;
    public DateTime InvoiceYearMonth { get; set; }
}

class DemoMessage2
{
    public int Version { get; set; }
    public string Message { get; set; } = string.Empty;
}