using consumer;
using KafkaFlow;

var builder = Host.CreateApplicationBuilder(args);

var bootstrapServers = builder.Configuration.GetSection("Kafka:BootstrapServers").Get<string[]>() ?? [];
var topic = builder.Configuration.GetSection("Kafka:Topic").Get<string>() ?? "";
var groupId = builder.Configuration.GetSection("Kafka:GroupId").Get<string>() ?? "";

builder.Services.AddKafka(k => k
    .UseConsoleLog()
    .AddCluster(c => c
        .WithBrokers(bootstrapServers)
        .WithSchemaRegistry(config => config.Url = "localhost:8081")
        .AddConsumer(consumer => consumer
            .Topic(topic)
            .WithGroupId(groupId)
            .WithAutoOffsetReset(AutoOffsetReset.Earliest)
            .WithBufferSize(100)
            .WithWorkersCount(3)
            .AddMiddlewares(m => m
                .AddSchemaRegistryJsonSerializer<DemoMessage>()
                .AddTypedHandlers(h => h
                    .WithHandlerLifetime(InstanceLifetime.Transient)
                    .AddHandler<DemoMessageHandler>()
                    .WhenNoHandlerFound(c => Console.WriteLine("No handler found for message {0}", c.Message.Value))
                )
            )
        )
    )
);

var host = builder.Build();
var bus = host.Services.CreateKafkaBus();

await bus.StartAsync();

Console.WriteLine($"Consumer count: {bus.Consumers.All.Count()}");
host.Run();