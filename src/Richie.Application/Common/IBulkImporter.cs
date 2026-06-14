namespace Richie.Application.Common;

public sealed record ImportRowError(int RowNumber, string Message);

public sealed record ImportResult(int ImportedCount, int TotalRows, IReadOnlyList<ImportRowError> Errors)
{
    public bool HasErrors => Errors.Count > 0;
}

/// <summary>
/// Common CSV/Excel bulk-import surface (row-by-row validation + template generation). Implemented
/// per module (assets, expenses, …) so one upload UI can drive any of them. Valid rows are imported
/// even when other rows fail.
/// </summary>
public interface IBulkImporter
{
    ImportResult ImportCsv(Stream csv);
    ImportResult ImportExcel(Stream xlsx);

    byte[] CreateCsvTemplate();
    byte[] CreateExcelTemplate();
}
