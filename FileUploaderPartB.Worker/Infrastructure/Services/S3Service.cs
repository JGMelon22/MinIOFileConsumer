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

    public async Task<Result<MemoryStream>> DownloadFileAsync(string s3Path)
    {
        try
        {
            string key = ExtractFileKey(s3Path);

            _logger.LogInformation("Attempting to download file from S3: {Key}", key);

            GetObjectRequest request = new()
            {
                BucketName = _bucketName,
                Key = key
            };

            using GetObjectResponse response = await _s3Client.GetObjectAsync(request);

            // Copiar o conteúdo do ResponseStream para um MemoryStream
            MemoryStream memoryStream = new();
            await response.ResponseStream.CopyToAsync(memoryStream);
            memoryStream.Position = 0; // Reposicionar para o início

            _logger.LogInformation("Successfully downloaded file: {Key}", key);

            return Result<MemoryStream>.Success(memoryStream);
        }
        catch (AmazonS3Exception s3Ex)
        {
            _logger.LogError(s3Ex, "AmazonS3Exception occurred while downloading file: {Key}", s3Path);
            return Result<MemoryStream>.Failure($"S3 error: {s3Ex.Message}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error occurred while downloading file: {Key}", s3Path);
            return Result<MemoryStream>.Failure($"Unexpected error: {ex.Message}");
        }
    }


    private string ExtractFileKey(string s3Path)
    {
        // Extraia apenas o caminho relativo
        if (Uri.TryCreate(s3Path, UriKind.Absolute, out Uri? uri))
        {
            // Remove o nome do bucket do caminho, se estiver presente
            string relativePath = uri.AbsolutePath.TrimStart('/');
            if (relativePath.StartsWith(_bucketName + "/"))
            {
                return relativePath.Substring(_bucketName.Length + 1);
            }
            return relativePath;
        }
        return s3Path;
    }
}