namespace Richie.Application.Storage;

/// <summary>Encrypted backup &amp; restore of the local database and document/photo/receipt files
/// (PRD §16). The artifacts are already encrypted at rest, so the archive is unreadable without the
/// app and the correct master password / Windows account.</summary>
public interface IBackupService
{
    /// <summary>Zips the encrypted database + file storage into the destination folder; returns the file path.</summary>
    string CreateBackup(string destinationFolder);

    /// <summary>Restores the database + files from a backup archive (same machine). Restart recommended.</summary>
    void Restore(string backupFilePath);

    DateTime? LastBackupUtc { get; }
}
