using FileUploaderPartB.Worker.Enums;

namespace FileUploaderPartB.Worker.Models;
public record FileMessage
{
    public string Id { get; init; } = string.Empty;
    public string S3Path { get; init; } = string.Empty;
    public Status Status { get; init; }
    public DateTime CreatedAt { get; init; }
}
