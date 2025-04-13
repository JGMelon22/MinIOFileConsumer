using FileUploaderPartB.Worker.Models;
using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker.Interfaces;

public interface IKafkaConsumerService
{
    IAsyncEnumerable<Result<FileMessage>> ConsumeAsync(CancellationToken cancellationToken);
}