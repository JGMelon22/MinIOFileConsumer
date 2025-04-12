using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker.Interfaces;

public interface IKafkaConsumerService
{
    IAsyncEnumerable<Result<string>> ConsumeAsync(CancellationToken cancellationToken);
}