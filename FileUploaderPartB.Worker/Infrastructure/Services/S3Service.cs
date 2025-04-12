using Amazon.S3;
using Amazon.S3.Model;
using FileUploaderPartB.Worker.Infrastructure.Configurations;
using FileUploaderPartB.Worker.Interfaces;
using FileUploaderPartB.Worker.Shared;
using Microsoft.Extensions.Options;

namespace FileUploaderPartB.Worker.Infrastructure.Services;

public class S3Service : IS3Service
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3Service> _logger;

    public S3Service(IOptions<AmazonS3Configuration> options, ILogger<S3Service> logger)
    {

        AmazonS3Configuration configuration = options.Value;

        _s3Client = new AmazonS3Client(
            configuration.AccessKey,
            configuration.SecretKey,
            new AmazonS3Config
            {
                ServiceURL = configuration.ServiceURL,
                ForcePathStyle = configuration.ForcePathStyle
            });

        _bucketName = configuration.BucketName;
        _logger = logger;
    }

    public async Task<Result<Stream>> DownloadFileAsync(string key)
    {
        try
        {
            _logger.LogInformation("Attempting to download file from S3: {Key}", key);

            GetObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = key
            };

            GetObjectResponse response = await _s3Client.GetObjectAsync(request);

            _logger.LogInformation("Successfully downloaded file: {Key}", key);

            return Result<Stream>.Success(response.ResponseStream);
        }

        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "AmazonS3Exception occurred while downloading file: {Key}", key);
            return Result<Stream>.Failure($"S3 error: {s3Ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while downloading file: {Key}", key);
            return Result<Stream>.Failure($"Unexpected error: {ex.Message}");
        }
    }
}