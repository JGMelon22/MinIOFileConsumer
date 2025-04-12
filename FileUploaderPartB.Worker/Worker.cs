using FileUploaderPartB.Worker.Interfaces;
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
        await foreach (Result<string> message in _kafkaService.ConsumeAsync(stoppingToken))
        {
            _logger.LogInformation("Received message: {Message}", message);

            using var scope = _serviceProvider.CreateScope();

            IS3Service s3Service = scope.ServiceProvider.GetRequiredService<IS3Service>();
            IFileRepository repository = scope.ServiceProvider.GetRequiredService<IFileRepository>();
            ICsvValidatorService csvValidatorService = scope.ServiceProvider.GetRequiredService<ICsvValidatorService>();

            bool isPending = await repository.IsPendingAsync(message.Data!);

            if (isPending)
            {
                _logger.LogInformation("File {File} already processed.", message);
                continue;
            }

            await repository.MarkAsProcessingAsync(message.Data!);


            var downloadResult = await s3Service.DownloadFileAsync(message.Data!);

            if (!downloadResult.IsSuccess)
            {
                await repository.MarkAsFailedAsync(message.Data!);

                _logger.LogError("Failed to download file from S3: {Message}", downloadResult.Message);
                continue;
            }

            try
            {
                using (var stream = downloadResult.Data!)
                {
                    var validationResult = csvValidatorService.ValidateCsv(stream);
                    if (!validationResult.IsSuccess)
                    {
                        // If validation fails, mark the file as failed and log the error
                        await repository.MarkAsFailedAsync(message.Data!);
                        _logger.LogError("CSV validation failed for file {File}: {Errors}", message, validationResult.Message);
                        continue;
                    }
                }

                await repository.MarkAsProcessedAsync(message.Data!);
                _logger.LogInformation("File {File} processed successfully.", message);
            }

            catch (Exception ex)
            {
                await repository.MarkAsFailedAsync(message.Data!);
                _logger.LogError(ex, "Error processing message {Message}", message);
            }
        }
    }
}
