using System.Collections.Concurrent;
using System.Threading.Channels;
using DownloadQueue.Models;

namespace DownloadQueue.Services;

// TODO 3, 4, 5, 6, 8: Ограничение параллелизма, Channel, сбор результатов, логирование времени, повторные попытки.
public sealed class QueueProcessor
{
    private readonly int _maxConcurrency;
    private readonly string _outputDirectory;
    private readonly int _maxRetries;

    public QueueProcessor(int maxConcurrency, string outputDirectory, int maxRetries = 2)
    {
        _maxConcurrency = maxConcurrency;
        _outputDirectory = outputDirectory;
        _maxRetries = maxRetries;
    }

    public async Task<IReadOnlyList<DownloadResult>> ProcessAsync(
        IEnumerable<DownloadTask> tasks,
        CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(_outputDirectory);

        // Ограниченный канал для backpressure
        var channel = Channel.CreateBounded<DownloadTask>(new BoundedChannelOptions(_maxConcurrency * 2)
        {
            FullMode = BoundedChannelFullMode.Wait
        });

        var results = new ConcurrentBag<DownloadResult>();

        // TODO 3: Ограничение числа одновременных загрузок
        using var semaphore = new SemaphoreSlim(_maxConcurrency, _maxConcurrency);

        // Producer: заполняет канал заданиями
        var producerTask = Task.Run(async () =>
        {
            try
            {
                foreach (var task in tasks)
                {
                    await channel.Writer.WriteAsync(task, cancellationToken);
                }
                channel.Writer.Complete();
            }
            catch (OperationCanceledException)
            {
                channel.Writer.Complete();
            }
        }, cancellationToken);

        // Consumers: обрабатывают задания из канала
        var consumers = Enumerable.Range(0, _maxConcurrency)
            .Select(_ => ConsumerAsync(channel.Reader, semaphore, results, cancellationToken))
            .ToArray();

        await Task.WhenAll(consumers);
        await producerTask;

        return results.OrderBy(r => r.TaskId).ToList();
    }

    private async Task ConsumerAsync(
        ChannelReader<DownloadTask> reader,
        SemaphoreSlim semaphore,
        ConcurrentBag<DownloadResult> results,
        CancellationToken cancellationToken)
    {
        await foreach (var task in reader.ReadAllAsync(cancellationToken))
        {
            await semaphore.WaitAsync(cancellationToken);
            try
            {
                var result = await ProcessTaskWithRetryAsync(task, cancellationToken);
                results.Add(result);
            }
            finally
            {
                semaphore.Release();
            }
        }
    }

    // TODO 8: Повторная постановка задания после временной ошибки с ограничением попыток.
    private async Task<DownloadResult> ProcessTaskWithRetryAsync(DownloadTask task, CancellationToken cancellationToken)
    {
        var startTime = DateTime.UtcNow;
        Exception? lastException = null;

        for (int attempt = 0; attempt <= _maxRetries; attempt++)
        {
            try
            {
                var destPath = Path.Combine(_outputDirectory, task.FileName);
                await DownloadService.DownloadAsync(task.Url, destPath, cancellationToken);

                return new DownloadResult
                {
                    TaskId = task.Id,
                    FileName = task.FileName,
                    StartTime = startTime,
                    EndTime = DateTime.UtcNow,
                    Success = true,
                    FilePath = destPath
                };
            }
            catch (Exception ex)
            {
                lastException = ex;
                if (attempt < _maxRetries)
                {
                    // Экспоненциальная задержка перед повтором
                    await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, attempt)), cancellationToken);
                }
            }
        }

        return new DownloadResult
        {
            TaskId = task.Id,
            FileName = task.FileName,
            StartTime = startTime,
            EndTime = DateTime.UtcNow,
            Success = false,
            Error = lastException?.Message
        };
    }
}