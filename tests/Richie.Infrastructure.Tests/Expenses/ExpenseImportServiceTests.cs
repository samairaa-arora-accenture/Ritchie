using System.IO;
using System.Text;
using Richie.Application.Common;
using Richie.Application.Expenses;
using Richie.Infrastructure.Authentication;
using Richie.Infrastructure.Expenses;
using Richie.Infrastructure.Tests.Helpers;

namespace Richie.Infrastructure.Tests.Expenses;

public sealed class ExpenseImportServiceTests : IDisposable
{
    private readonly TempSqlCipherDatabase _db = new();
    private readonly FakeClock _clock = new();
    private readonly UserSession _session = new();
    private readonly ExpenseService _expenses;
    private readonly ExpenseImportService _sut;

    public ExpenseImportServiceTests()
    {
        _session.SignIn(Guid.NewGuid(), "Tester");
        _expenses = new ExpenseService(_db, _session, _clock);
        _sut = new ExpenseImportService(_expenses);
    }

    private static Stream Csv(string content) => new MemoryStream(Encoding.UTF8.GetBytes(content));

    [Fact]
    public void ImportCsv_ImportsValidRows_AndReportsInvalid()
    {
        string csv = string.Join("\n",
            "Date,Amount,Category,SpentBy,SpentFor",
            "2026-01-05,50,DiningRestaurants,Me,Lunch",
            "2026-01-06,abc,Transportation,Me,Taxi",        // bad amount
            "2026-01-07,30,Unknown,Me,Misc");               // bad category

        ImportResult result = _sut.ImportCsv(Csv(csv));

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(1, result.ImportedCount);
        Assert.Equal(2, result.Errors.Count);
        Assert.Contains(result.Errors, e => e.RowNumber == 3 && e.Message.Contains("Amount"));
        Assert.Contains(result.Errors, e => e.RowNumber == 4 && e.Message.Contains("category"));
        Assert.Single(_expenses.GetExpenses());
    }

    [Fact]
    public void ImportExcel_RoundTripsThroughGeneratedTemplate()
    {
        byte[] template = _sut.CreateExcelTemplate();
        using var wb = new ClosedXML.Excel.XLWorkbook(new MemoryStream(template));
        var sheet = wb.Worksheet(1);
        sheet.Cell(2, 1).Value = "2026-01-10";        // Date
        sheet.Cell(2, 2).Value = 125;                 // Amount
        sheet.Cell(2, 3).Value = "GroceriesFood";     // Category

        using var filled = new MemoryStream();
        wb.SaveAs(filled);
        filled.Position = 0;

        ImportResult result = _sut.ImportExcel(filled);

        Assert.Equal(1, result.ImportedCount);
        Assert.False(result.HasErrors);
        Assert.Equal(125m, _expenses.GetExpenses().Single().Amount);
    }

    [Fact]
    public void CsvTemplate_HasAllColumns()
    {
        string text = Encoding.UTF8.GetString(_sut.CreateCsvTemplate());
        foreach (string column in ExpenseImportColumns.All)
            Assert.Contains(column, text);
    }

    public void Dispose() => _db.Dispose();
}
