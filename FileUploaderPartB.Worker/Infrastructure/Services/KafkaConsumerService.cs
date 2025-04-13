using System.Runtime.CompilerServices;
using System.Text.Json;
using Confluent.Kafka;
using FileUploaderPartB.Worker.Infrastructure.Configurations;
using FileUploaderPartB.Worker.Interfaces;
using FileUploaderPartB.Worker.Models;
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

    public async IAsyncEnumerable<Result<FileMessage>> ConsumeAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            Result<FileMessage> result;

            try
            {
                ConsumeResult<Ignore, string> consumeResult = _consumer.Consume(cancellationToken);
                if (consumeResult == null || consumeResult.Message?.Value == null)
                {
                    result = Result<FileMessage>.Failure("No message received or null value.");
                }
                else
                {
                    try
                    {
                        FileMessage? fileMessage = JsonSerializer.Deserialize<FileMessage>(consumeResult.Message.Value,
                            new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                        if (fileMessage == null)
                        {
                            result = Result<FileMessage>.Failure("Deserialization resulted in a null object.");
                        }
                        else
                        {
                            result = Result<FileMessage>.Success(fileMessage);
                        }
                    }
                    catch (JsonException ex)
                    {
                        _logger.LogError(ex, "JSON deserialization error for message: {Message}", consumeResult.Message.Value);
                        result = Result<FileMessage>.Failure($"JSON deserialization error: {ex.Message}");
                    }
                }
            }
            catch (ConsumeException ex)
            {
                _logger.LogError(ex, "Kafka consume error occurred while consuming from topic: {Topic}", _options.Topic);
                result = Result<FileMessage>.Failure($"Kafka consume error: {ex.Message}");
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Kafka consume operation was cancelled");
                yield break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error occurred while consuming from topic: {Topic}", _options.Topic);
                result = Result<FileMessage>.Failure($"Unexpected error: {ex.Message}");
            }

            yield return result;
            await Task.Yield();
        }
    }
}