namespace FileUploaderPartB.Worker.Interfaces;

public interface IFileRepository
{
    Task<bool> IsPendingAsync(string s3Path);
    Task MarkAsProcessedAsync(string s3Path);
    Task MarkAsFailedAsync(string s3Path);
    Task MarkAsProcessingAsync(string s3Path);
}