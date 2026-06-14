using System.Collections.ObjectModel;
using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using Richie.Application.Common;

namespace Richie.UI.ViewModels;

/// <summary>Generic bulk-upload VM driven by any <see cref="IBulkImporter"/> (assets, expenses, …).</summary>
public partial class BulkUploadViewModel : ObservableObject
{
    private IBulkImporter? _importer;

    [ObservableProperty] private string _heading = "Bulk upload";
    [ObservableProperty] private string _summary = "Download a template, fill it in, then drop the file here or browse.";
    [ObservableProperty] private ObservableCollection<ImportRowError> _errors = [];
    [ObservableProperty] private bool _hasErrors;

    public bool ImportedAny { get; private set; }

    public void Initialize(IBulkImporter importer, string heading)
    {
        _importer = importer;
        Heading = heading;
    }

    public void ImportFile(string filePath)
    {
        if (_importer is null)
            return;

        string ext = Path.GetExtension(filePath).ToLowerInvariant();
        using FileStream stream = File.OpenRead(filePath);

        ImportResult result;
        switch (ext)
        {
            case ".csv":
                result = _importer.ImportCsv(stream);
                break;
            case ".xlsx":
                result = _importer.ImportExcel(stream);
                break;
            default:
                Summary = "Unsupported file type. Use a .csv or .xlsx file.";
                Errors = [];
                HasErrors = false;
                return;
        }

        Summary = $"Imported {result.ImportedCount} of {result.TotalRows} row(s)" +
                  (result.HasErrors ? $", {result.Errors.Count} row(s) had errors:" : ".");
        Errors = new ObservableCollection<ImportRowError>(result.Errors);
        HasErrors = result.HasErrors;
        ImportedAny |= result.ImportedCount > 0;
    }

    public void SaveCsvTemplate(string path)
    {
        if (_importer is not null)
            File.WriteAllBytes(path, _importer.CreateCsvTemplate());
    }

    public void SaveExcelTemplate(string path)
    {
        if (_importer is not null)
            File.WriteAllBytes(path, _importer.CreateExcelTemplate());
    }
}
