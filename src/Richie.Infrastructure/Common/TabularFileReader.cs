using System.Globalization;
using System.IO;
using ClosedXML.Excel;
using CsvHelper;

namespace Richie.Infrastructure.Common;

/// <summary>
/// Reads a CSV or XLSX stream into header-keyed rows for bulk importers. Row numbers are the
/// file/sheet row (header = row 1). Only columns present in the file appear in each row's dictionary.
/// </summary>
public static class TabularFileReader
{
    public static List<(int RowNumber, Dictionary<string, string> Values)> ReadCsv(Stream csv)
    {
        var rows = new List<(int, Dictionary<string, string>)>();
        using var reader = new StreamReader(csv);
        using var parser = new CsvReader(reader, CultureInfo.InvariantCulture);

        parser.Read();
        parser.ReadHeader();
        string[] headers = parser.HeaderRecord ?? [];

        int rowNumber = 1; // header is row 1
        while (parser.Read())
        {
            rowNumber++;
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string header in headers)
                values[header] = parser.GetField(header) ?? string.Empty;
            rows.Add((rowNumber, values));
        }
        return rows;
    }

    public static List<(int RowNumber, Dictionary<string, string> Values)> ReadExcel(Stream xlsx)
    {
        var rows = new List<(int, Dictionary<string, string>)>();
        using var workbook = new XLWorkbook(xlsx);
        IXLWorksheet sheet = workbook.Worksheet(1);
        List<IXLRangeRow>? rowList = sheet.RangeUsed()?.RowsUsed().ToList();
        if (rowList is null || rowList.Count == 0)
            return rows;

        var headerColumns = new List<(int Column, string Name)>();
        foreach (IXLCell cell in rowList[0].CellsUsed())
            headerColumns.Add((cell.Address.ColumnNumber, cell.GetString()));

        for (int i = 1; i < rowList.Count; i++)
        {
            IXLRangeRow row = rowList[i];
            var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach ((int column, string name) in headerColumns)
                values[name] = row.Worksheet.Cell(row.RowNumber(), column).GetString();
            rows.Add((row.RowNumber(), values));
        }
        return rows;
    }
}
