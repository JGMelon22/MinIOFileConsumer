using FileUploaderPartB.Worker.Shared;

namespace FileUploaderPartB.Worker.Interfaces;

public interface ICsvValidatorService
{
    public Result<List<string>> ValidateCsv(string filePath);
}