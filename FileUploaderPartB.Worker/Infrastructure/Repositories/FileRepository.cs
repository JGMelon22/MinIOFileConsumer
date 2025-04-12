using System.Data;
using Dapper;
using FileUploaderPartB.Worker.Enums;
using FileUploaderPartB.Worker.Infrastructure.Data;
using FileUploaderPartB.Worker.Interfaces;

namespace FileUploaderPartB.Worker.Infrastructure.Repositories;

public class FileRepository : IFileRepository
{
    private readonly DapperDbContext _dbContext;
    private ILogger<FileRepository> _logger;

    public FileRepository(DapperDbContext dbContext, ILogger<FileRepository> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<bool> IsPendingAsync(string s3Path)
    {
        try
        {
            _logger.LogInformation("{Repository}.{Method} - Start: Attempting to fetch pending files from S3 path: {S3Path}",
                GetType().Name, nameof(IsPendingAsync), s3Path);

            const string sql = @"
                                SELECT COUNT(1)
                                FROM imports WHERE s3_path = @S3Path
                                AND status = @Status;";

            using (IDbConnection connection = _dbContext.CreateConnection())
            {
                int result = await connection.ExecuteScalarAsync<int>(sql, new { S3Path = s3Path, Status = nameof(Status.Pending) });

                return result > 0;
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{Repository}.{Method} - Exception occurred while checking file pending status. S3Path: {S3Path}",
                GetType().Name, nameof(IsPendingAsync), s3Path);

            return false;
        }
    }

    public async Task MarkAsFailedAsync(string s3Path)
    {
        try
        {
            _logger.LogInformation("{Repository}.{Method} - Start: Attempting to set file as failed from S3 path: {S3Path}",
                GetType().Name, nameof(MarkAsFailedAsync), s3Path);

            const string sql = @"
                                UPDATE imports 
                                SET status = @Status,
                                ProcessedAt = UTC_TIMESTAMP()
                                WHERE s3_path = @FilePath";

            using (IDbConnection connection = _dbContext.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { S3Path = s3Path, Status = nameof(Status.Failed) });
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{Repository}.{Method} - Exception occurred while setting file status to failed. S3Path: {S3Path}",
                GetType().Name, nameof(MarkAsFailedAsync), s3Path);
        }
    }

    public async Task MarkAsProcessedAsync(string s3Path)
    {
        try
        {
            _logger.LogInformation("{Repository}.{Method} - Start: Attempting to set file as processed from S3 path: {S3Path}",
                GetType().Name, nameof(MarkAsProcessedAsync), s3Path);

            const string sql = @"
                                UPDATE imports 
                                SET status = @Status,
                                ProcessedAt = UTC_TIMESTAMP()
                                WHERE s3_path = @FilePath";

            using (IDbConnection connection = _dbContext.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { S3Path = s3Path, Status = nameof(Status.Processed) });
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{Repository}.{Method} - Exception occurred while setting file status to processed. S3Path: {S3Path}",
                GetType().Name, nameof(MarkAsProcessedAsync), s3Path);
        }
    }

    public async Task MarkAsProcessingAsync(string s3Path)
    {
        try
        {
            _logger.LogInformation("{Repository}.{Method} - Start: Attempting to set file as processing from S3 path: {S3Path}",
                GetType().Name, nameof(MarkAsProcessingAsync), s3Path);

            const string sql = @"
                                UPDATE imports 
                                SET status = @Status,
                                ProcessedAt = UTC_TIMESTAMP()
                                WHERE s3_path = @FilePath";

            using (IDbConnection connection = _dbContext.CreateConnection())
            {
                await connection.ExecuteAsync(sql, new { S3Path = s3Path, Status = nameof(Status.Processing) });
            }

        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "{Repository}.{Method} - Exception occurred while setting file status to processing. S3Path: {S3Path}",
                GetType().Name, nameof(MarkAsProcessingAsync), s3Path);
        }
    }
}