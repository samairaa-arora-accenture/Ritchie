using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using Richie.Application.Common;
using Richie.Application.Expenses;
using Richie.Domain.Expenses;
using Richie.Infrastructure.Common;

namespace Richie.Infrastructure.Expenses;

public sealed class ExpenseImportService : IExpenseImportService
{
    private readonly IExpenseService _expenses;

    public ExpenseImportService(IExpenseService expenses) => _expenses = expenses;

    public ImportResult ImportCsv(Stream csv) => Import(TabularFileReader.ReadCsv(csv));

    public ImportResult ImportExcel(Stream xlsx) => Import(TabularFileReader.ReadExcel(xlsx));

    public byte[] CreateCsvTemplate() =>
        new UTF8Encoding(false).GetBytes(string.Join(",", ExpenseImportColumns.All) + Environment.NewLine);

    public byte[] CreateExcelTemplate()
    {
        using var workbook = new XLWorkbook();
        IXLWorksheet sheet = workbook.AddWorksheet("Expenses");
        for (int i = 0; i < ExpenseImportColumns.All.Count; i++)
            sheet.Cell(1, i + 1).Value = ExpenseImportColumns.All[i];
        sheet.Row(1).Style.Font.Bold = true;

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    private ImportResult Import(List<(int RowNumber, Dictionary<string, string> Values)> rows)
    {
        var errors = new List<ImportRowError>();
        var inputs = new List<ExpenseInput>();

        foreach ((int rowNumber, Dictionary<string, string> values) in rows)
        {
            ExpenseInput? input = TryBuild(values, out string? error);
            if (input is null)
                errors.Add(new ImportRowError(rowNumber, error!));
            else
                inputs.Add(input);
        }

        foreach (ExpenseInput input in inputs)
            _expenses.Create(input);

        return new ImportResult(inputs.Count, rows.Count, errors);
    }

    private static ExpenseInput? TryBuild(Dictionary<string, string> v, out string? error)
    {
        error = null;

        if (!TryDate(Get(v, ExpenseImportColumns.Date), out DateTime date))
        {
            error = "Invalid or missing Date (use yyyy-MM-dd).";
            return null;
        }
        if (!TryDecimal(Get(v, ExpenseImportColumns.Amount), out decimal amount) || amount <= 0)
        {
            error = "Invalid or missing Amount.";
            return null;
        }

        string categoryRaw = Get(v, ExpenseImportColumns.Category);
        if (!Enum.TryParse(categoryRaw, ignoreCase: true, out ExpenseCategory category) || !Enum.IsDefined(category))
        {
            error = $"Unknown category '{categoryRaw}'.";
            return null;
        }

        return new ExpenseInput(date, amount, category,
            Opt(v, ExpenseImportColumns.SpentBy), Opt(v, ExpenseImportColumns.SpentFor), Opt(v, ExpenseImportColumns.Notes));
    }

    private static string Get(Dictionary<string, string> v, string key) =>
        v.TryGetValue(key, out string? value) ? value.Trim() : string.Empty;

    private static string? Opt(Dictionary<string, string> v, string key)
    {
        string value = Get(v, key);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    private static bool TryDecimal(string s, out decimal value) =>
        decimal.TryParse(s, NumberStyles.Number, CultureInfo.InvariantCulture, out value)
        || decimal.TryParse(s, NumberStyles.Number, CultureInfo.CurrentCulture, out value);

    private static bool TryDate(string s, out DateTime value) =>
        DateTime.TryParse(s, CultureInfo.InvariantCulture, DateTimeStyles.None, out value)
        || DateTime.TryParse(s, CultureInfo.CurrentCulture, DateTimeStyles.None, out value);
}
