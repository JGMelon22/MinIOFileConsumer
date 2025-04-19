using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker.Interfaces;

public interface IS3Service
{
    Task<Result<MemoryStream>> DownloadFileAsync(string s3Path);
}