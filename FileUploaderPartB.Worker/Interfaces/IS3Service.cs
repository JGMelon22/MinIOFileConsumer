using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker.Interfaces;

public interface IS3Service
{
    Task<Result<Stream>> DownloadFileAsync(string key);
}