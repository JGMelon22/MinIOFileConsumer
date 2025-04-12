using System.Runtime.CompilerServices;
using Confluent.Kafka;
using FileUploaderPartB.Worker.Infrastructure.Configurations;
using FileUploaderPartB.Worker.Interfaces;
using FileUploaderPartB.Worker.Shared;
using Microsoft.Extensions.Options;

namespace FileUploaderPartB.Worker.Infrastructure.Services;

public class KafkaConsumerService : IKafkaConsumerService
{
    private readonly KafkaOptions _options;
    private readonly IConsumer<Ignore, string> _consumer;
    private readonly ILogger<KafkaConsumerService> _logger;

    public KafkaConsumerService(IOptions<KafkaOptions> options, ILogger<KafkaConsumerService> logger)
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
        _logger = logger;
    }

    public async IAsyncEnumerable<Result<string>> ConsumeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Result<string> result;

            try
            {
                var consumeResult = _consumer.Consume(cancellationToken);
                result = Result<string>.Success(consumeResult.Message.Value);
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error occurred while consuming from topic: {Topic}", _options.Topic);
                result = Result<string>.Failure($"Kafka consume error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consume operation was cancelled");
                yield break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while consuming from topic: {Topic}", _options.Topic);
                result = Result<string>.Failure($"Unexpected error: {ex.Message}");
            }

            yield return result;
            await Task.Yield();
        }
    }
}
