namespace FileUploaderPartB.Worker.Interfaces;

public interface IKafkaConsumerService
{
    IAsyncEnumerable<string> ConsumeAsync(CancellationToken cancellationToken);
}