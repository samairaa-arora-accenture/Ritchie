using System.IO;
using System.IO.Compression;
using Microsoft.Data.Sqlite;
using Richie.Application.Abstractions;
using Richie.Application.Settings;
using Richie.Application.Storage;

namespace Richie.Infrastructure.Storage;

public sealed class BackupService : IBackupService
{
    private static readonly string[] FileFolders = ["photos", "documents", "receipts"];

    private readonly IAppSettingsService _settings;
    private readonly IClock _clock;

    public BackupService(IAppSettingsService settings, IClock clock)
    {
        _settings = settings;
        _clock = clock;
    }

    public DateTime? LastBackupUtc => _settings.Get().LastBackupUtc;

    public string CreateBackup(string destinationFolder)
    {
        Directory.CreateDirectory(destinationFolder);
        string path = Path.Combine(destinationFolder, $"richie-backup-{_clock.UtcNow:yyyyMMdd-HHmmss}.zip");

        // Flush any open SQLite handles so the file copies cleanly.
        SqliteConnection.ClearAllPools();

        using (ZipArchive zip = ZipFile.Open(path, ZipArchiveMode.Create))
        {
            if (File.Exists(AppPaths.DatabasePath))
                zip.CreateEntryFromFile(AppPaths.DatabasePath, "richie.db");

            foreach (string sub in FileFolders)
            {
                string dir = Path.Combine(AppPaths.DataDirectory, sub);
                if (!Directory.Exists(dir))
                    continue;
                foreach (string file in Directory.EnumerateFiles(dir, "*", SearchOption.AllDirectories))
                {
                    string rel = Path.GetRelativePath(AppPaths.DataDirectory, file).Replace('\\', '/');
                    zip.CreateEntryFromFile(file, rel);
                }
            }
        }

        _settings.SetLastBackup(_clock.UtcNow);
        return path;
    }

    public void Restore(string backupFilePath)
    {
        SqliteConnection.ClearAllPools();

        using ZipArchive zip = ZipFile.OpenRead(backupFilePath);
        foreach (ZipArchiveEntry entry in zip.Entries)
        {
            if (string.IsNullOrEmpty(entry.Name))   // directory entry
                continue;
            string dest = Path.Combine(AppPaths.DataDirectory, entry.FullName.Replace('/', Path.DirectorySeparatorChar));
            Directory.CreateDirectory(Path.GetDirectoryName(dest)!);
            entry.ExtractToFile(dest, overwrite: true);
        }
    }
}
