using Microsoft.Win32;

namespace DownloadQueue.Services;

// TODO 7: Сохранять каталог загрузки в HKCU.
public static class RegistryService
{
    private const string RegistryPath = @"Software\DownloadQueueLab";
    private const string DownloadDirKey = "DownloadDirectory";
    private const string DefaultDir = "Downloads";

    public static string GetDownloadDirectory()
    {
        using var key = Registry.CurrentUser.OpenSubKey(RegistryPath);
        if (key != null)
        {
            var dir = key.GetValue(DownloadDirKey) as string;
            if (!string.IsNullOrWhiteSpace(dir))
                return dir;
        }

        return Path.Combine(AppContext.BaseDirectory, DefaultDir);
    }

    public static void SaveDownloadDirectory(string path)
    {
        using var key = Registry.CurrentUser.CreateSubKey(RegistryPath, writable: true);
        key.SetValue(DownloadDirKey, path, RegistryValueKind.String);
    }
}