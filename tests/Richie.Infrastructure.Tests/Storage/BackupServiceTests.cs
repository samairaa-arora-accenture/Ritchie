using System.IO;
using System.IO.Compression;
using System.Text;
using Richie.Infrastructure;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Settings;
using Richie.Infrastructure.Storage;
using Richie.Infrastructure.Tests.Helpers;

namespace Richie.Infrastructure.Tests.Storage;

public sealed class BackupServiceTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new();
    private readonly UserSession _session = new();
    private readonly string _originalDataDir = AppPaths.DataDirectory;
    private readonly string _dataDir = Path.Combine(Path.GetTempPath(), $"richie-bkp-{Guid.NewGuid():N}");
    private readonly string _destDir = Path.Combine(Path.GetTempPath(), $"richie-dest-{Guid.NewGuid():N}");
    private readonly BackupService _sut;

    public BackupServiceTests()
    {
        AppPaths.DataDirectory = _dataDir;
        Directory.CreateDirectory(_dataDir);
        _session.SignIn(Guid.NewGuid(), "Tester");
        _sut = new BackupService(new AppSettingsService(_db, _session), _clock);
    }

    [Fact]
    public void CreateBackup_ZipsDbAndFiles_AndRestoreBringsThemBack()
    {
        File.WriteAllText(AppPaths.DatabasePath, "fake-encrypted-db");
        string receiptsDir = Path.Combine(_dataDir, "receipts", "expenses");
        Directory.CreateDirectory(receiptsDir);
        File.WriteAllBytes(Path.Combine(receiptsDir, "bill.enc"), [1, 2, 3]);

        string zipPath = _sut.CreateBackup(_destDir);

        Assert.True(File.Exists(zipPath));
        Assert.NotNull(_sut.LastBackupUtc);
        using (ZipArchive zip = ZipFile.OpenRead(zipPath))
        {
            Assert.Contains(zip.Entries, e => e.FullName == "richie.db");
            Assert.Contains(zip.Entries, e => e.FullName == "receipts/expenses/bill.enc");
        }

        // Wipe and restore.
        File.Delete(AppPaths.DatabasePath);
        Directory.Delete(Path.Combine(_dataDir, "receipts"), recursive: true);

        _sut.Restore(zipPath);

        Assert.Equal("fake-encrypted-db", File.ReadAllText(AppPaths.DatabasePath));
        Assert.True(File.Exists(Path.Combine(receiptsDir, "bill.enc")));
    }

    public void Dispose()
    {
        AppPaths.DataDirectory = _originalDataDir;
        _db.Dispose();
        foreach (string dir in new[] { _dataDir, _destDir })
            if (Directory.Exists(dir))
                Directory.Delete(dir, recursive: true);
    }
}
