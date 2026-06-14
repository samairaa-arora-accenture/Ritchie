using System.Globalization;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Win32;
using Richie.Application.Storage;
using Richie.UI.Services;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Profile;

public partial class BackupWindow : FluentWindow
{
    private readonly IBackupService _backup;
    private readonly ToastService _toast;

    public BackupWindow(IBackupService backup, ToastService toast)
    {
        InitializeComponent();
        _backup = backup;
        _toast = toast;
        Loaded += (_, _) => RefreshLastBackup();
    }

    private void RefreshLastBackup() =>
        LastBackupText.Text = _backup.LastBackupUtc is { } utc
            ? $"Last backup: {utc.ToLocalTime().ToString("g", CultureInfo.CurrentCulture)}"
            : "No backup yet.";

    private void OnBackup(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFolderDialog { Title = "Choose a folder for the backup" };
        if (dialog.ShowDialog(this) != true)
            return;

        string path = _backup.CreateBackup(dialog.FolderName);
        RefreshLastBackup();
        _toast.Success($"Backup saved to {System.IO.Path.GetFileName(path)}.");
    }

    private void OnRestore(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Title = "Choose a backup archive", Filter = "Richie backup|*.zip" };
        if (dialog.ShowDialog(this) != true)
            return;

        if (System.Windows.MessageBox.Show(
                "Restoring will overwrite your current data with the backup. Continue?",
                "Confirm restore", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning)
            != System.Windows.MessageBoxResult.Yes)
            return;

        _backup.Restore(dialog.FileName);
        System.Windows.MessageBox.Show(
            "Restore complete. Please close and reopen Richie for the restored data to take effect.",
            "Restore complete", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        Close();
    }
}
