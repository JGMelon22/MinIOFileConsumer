using FileUploaderPartB.Worker.Interfaces;
using FileUploaderPartB.Worker.Models;
using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker;

public class Worker : BackgroundService
{
    private readonly IKafkaConsumerService _kafkaService;
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<Worker> _logger;

    public Worker(
        IKafkaConsumerService kafkaService,
        IServiceProvider serviceProvider,
        ILogger<Worker> logger)
    {
        _kafkaService = kafkaService;
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            _logger.LogInformation("Worker iteration started at {Time}", DateTimeOffset.Now);

            await foreach (Result<FileMessage> message in _kafkaService.ConsumeAsync(stoppingToken))
            {
                _logger.LogInformation("Received message: {Message}", message);

                if (!message.IsSuccess || message.Data == null)
                {
                    _logger.LogWarning("Skipping invalid message.");
                    continue;
                }

                var fileMessage = message.Data;

                _logger.LogInformation("Processing message with ID: {Id}, S3Path: {S3Path}, Status: {Status}",
                 fileMessage.Id, fileMessage.S3Path, fileMessage.Status);

                using IServiceScope scope = _serviceProvider.CreateScope();

                IS3Service s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
                IFileRepository repository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
                ICsvValidatorService csvValidatorService = scope.ServiceProvider.GetRequiredService<ICsvValidatorService>();

                bool isPending = await repository.IsPendingAsync(fileMessage.S3Path);

                if (!isPending)
                {
                    _logger.LogInformation("File {File} already processed.", fileMessage.Id);
                    continue;
                }

                await repository.MarkAsProcessingAsync(fileMessage.S3Path);

                var downloadResult = await s3Service.DownloadFileAsync(fileMessage.S3Path);

                if (!downloadResult.IsSuccess)
                {
                    await repository.MarkAsFailedAsync(fileMessage.S3Path);
                    _logger.LogError("Failed to download file from S3: {Message}", downloadResult.Message);
                    continue;
                }

                try
                {
                    using var stream = downloadResult.Data!;
                    var validationResult = await csvValidatorService.ValidateCsvAsync(stream);

                    if (!validationResult.IsSuccess)
                    {
                        await repository.MarkAsFailedAsync(fileMessage.S3Path);
                        _logger.LogError("CSV validation failed for file {File}: {Errors}", fileMessage.Id, validationResult.Message);
                        continue;
                    }

                    await repository.MarkAsProcessedAsync(fileMessage.S3Path);
                    _logger.LogInformation("File {File} processed successfully.", fileMessage.Id);
                }
                catch (Exception ex)
                {
                    await repository.MarkAsFailedAsync(fileMessage.S3Path);
                    _logger.LogError(ex, "Error processing message {Message}", fileMessage.Id);
                }
            }

            _logger.LogInformation("Worker iteration finished. Waiting 15 minutes...");
            await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
        }
    }
}
