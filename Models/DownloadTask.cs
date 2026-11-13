namespace DownloadQueue.Models;

public enum TaskStatus
{
    Pending,
    InProgress,
    Completed,
    Failed
}

// TODO 1: Модель задания загрузки с URL и статусом.
public sealed class DownloadTask
{
    public int Id { get; init; }
    public string Url { get; init; } = "";
    public string FileName { get; init; } = "";
    public TaskStatus Status { get; set; } = TaskStatus.Pending;
}