namespace DownloadQueue.Models;

// TODO 5 и 6: Сбор результата, ошибки и журнала времени
public sealed class DownloadResult
{
    public int TaskId { get; init; }
    public string FileName { get; init; }
    public DateTime StartTime { get; init; }
    public DateTime EndTime { get; init; }
    public bool Success { get; init; }
    public string? Error { get; init; }
    public string? FilePath { get; init; }
}