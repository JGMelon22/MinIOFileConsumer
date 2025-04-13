using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker.Interfaces;

public interface ICsvValidatorService
{
    public Task<Result<List<string>>> ValidateCsvAsync(MemoryStream stream);
}