using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;

namespace Variant13;

internal static class Program
{
    private static async Task Main()
    {
        Console.OutputEncoding = System.Text.Encoding.UTF8;
        Console.WriteLine("Зачётный проект 13: Очередь загрузки ресурсов");
        Console.WriteLine(new string('=', 64));

        // Стартовый сценарий нужен для проверки структуры проекта.
        // Реализацию индивидуальных требований выполняйте в ProjectService.
        var service = new ProjectService();
        await service.RunAsync();

        Console.WriteLine();
        Console.WriteLine("Нажмите Enter для завершения.");
        Console.ReadLine();
    }
}

public sealed class ProjectService
{
    public async Task RunAsync()
    {
        // TODO: перенесите предметную логику в отдельные методы и классы.
        // TODO: не размещайте весь проект в Main.
        // TODO 1: Создать модель задания загрузки с URL и статусом.
        // TODO 2: Реализовать асинхронное сохранение локального файла из предоставленного тестового источника/потока.
        // TODO 3: Ограничить число одновременных загрузок.
        // TODO 4: Организовать producer-consumer очередь через Channel.
        // TODO 5: Собирать результат и ошибку каждого задания.
        // TODO 6: Добавить журнал времени начала и окончания.
        // TODO 7: Сохранять каталог загрузки в HKCU.

        // TODO 8: Добавить повторную постановку задания после временной ошибки с ограничением попыток.
        // TODO 9: Сформировать итоговую статистику через LINQ.
        // TODO 10: Добавить запуск папки результата через Process.Start с проверкой платформы.

        Console.WriteLine("Стартовый проект собран. Реализуйте задания варианта.");
        await Task.CompletedTask;
    }
}
