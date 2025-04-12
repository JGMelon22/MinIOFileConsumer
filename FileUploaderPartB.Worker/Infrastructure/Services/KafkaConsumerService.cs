using System.Runtime.CompilerServices;
using Confluent.Kafka;
using FileUploaderPartB.Worker.Infrastructure.Configurations;
using FileUploaderPartB.Worker.Interfaces;
using Microsoft.Extensions.Options;

namespace FileUploaderPartB.Worker.Infrastructure.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly KafkaOptions _options;
    private readonly IConsumer<Ignore, string> _consumer;

    public KafkaConsumerService(IOptions<KafkaOptions> options)
    {
        _options = options.Value;
        ConsumerConfig configuration = new()
        {
            BootstrapServers = _options.BootstrapServers,
            GroupId = _options.GroupId,
            AutoOffsetReset = AutoOffsetReset.Earliest
        };

        _consumer = new ConsumerBuilder<Ignore, string>(configuration).Build();
        _consumer.Subscribe(_options.Topic);
    }

    public async IAsyncEnumerable<string> ConsumeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            ConsumeResult<Ignore, string> consumeResult = _consumer.Consume(cancellationToken);
            yield return consumeResult.Message.Value;

            await Task.Yield();
        }
    }
}