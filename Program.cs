using System.Diagnostics;
using DownloadQueue.Models;
using DownloadQueue.Services;

namespace DownloadQueue;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Зачётный проект 13: Очередь загрузки ресурсов");
        Console.WriteLine(new string('=', 64));

        // TODO 7: Получение и сохранение каталога в HKCU
        string downloadDir = RegistryService.GetDownloadDirectory();
        Console.WriteLine($"Текущий каталог загрузки: {downloadDir}");

        Console.Write("Изменить каталог? (y/n): ");
        if (Console.ReadLine()?.Trim().ToLower() == "y")
        {
            Console.Write("Введите новый путь: ");
            string? newPath = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(newPath))
            {
                downloadDir = newPath;
                RegistryService.SaveDownloadDirectory(downloadDir);
            }
        }

        // TODO 1: Создание заданий (одно намеренно битое для проверки ошибок и повторов)
        var tasks = new List<DownloadTask>
        {
            new() { Id = 1, Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", FileName = "dummy1.pdf" },
            new() { Id = 2, Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", FileName = "dummy2.pdf" },
            new() { Id = 3, Url = "https://invalid-url.example.com/file.txt", FileName = "error.txt" },
            new() { Id = 4, Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", FileName = "dummy3.pdf" },
            new() { Id = 5, Url = "https://www.w3.org/WAI/ER/tests/xhtml/testfiles/resources/pdf/dummy.pdf", FileName = "dummy4.pdf" }
        };

        Console.WriteLine($"Создано заданий: {tasks.Count}");
        Console.Write("Максимальное количество одновременных загрузок: ");
        int maxConcurrency = int.TryParse(Console.ReadLine(), out int mc) && mc > 0 ? mc : 2;

        var processor = new QueueProcessor(maxConcurrency, downloadDir, maxRetries: 2);
        var cts = new CancellationTokenSource();

        Console.WriteLine("Начало обработки... (Нажмите Enter для отмены)");
        Task.Run(() => { Console.ReadLine(); cts.Cancel(); });

        try
        {
            var results = await processor.ProcessAsync(tasks, cts.Token);

            // TODO 9: Итоговая статистика через LINQ
            PrintStatistics(results);

            // TODO 10: Запуск папки результата через Process.Start с проверкой платформы
            if (OperatingSystem.IsWindows() || OperatingSystem.IsLinux() || OperatingSystem.IsMacOS())
            {
                Console.Write("Открыть папку с результатами? (y/n): ");
                if (Console.ReadLine()?.Trim().ToLower() == "y")
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = downloadDir,
                        UseShellExecute = true
                    });
                }
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Обработка отменена пользователем.");
        }

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }

    private static void PrintStatistics(IReadOnlyList<DownloadResult> results)
    {
        Console.WriteLine();
        Console.WriteLine("=== СТАТИСТИКА (LINQ) ===");
        Console.WriteLine($"Всего заданий: {results.Count}");
        Console.WriteLine($"Успешно: {results.Count(r => r.Success)}");
        Console.WriteLine($"С ошибками: {results.Count(r => !r.Success)}");

        var totalTimeMs = results.Sum(r => (r.EndTime - r.StartTime).TotalMilliseconds);
        Console.WriteLine($"Суммарное время обработки: {totalTimeMs:F0} мс");

        Console.WriteLine();
        Console.WriteLine("Журнал операций (Start -> End):");
        foreach (var r in results.OrderBy(r => r.StartTime))
        {
            string status = r.Success ? "OK  " : "FAIL";
            Console.WriteLine($"[{status}] ID:{r.TaskId} {r.FileName,-12} | {r.StartTime:HH:mm:ss.fff} -> {r.EndTime:HH:mm:ss.fff} | {(r.Error ?? "")}");
        }
    }
}