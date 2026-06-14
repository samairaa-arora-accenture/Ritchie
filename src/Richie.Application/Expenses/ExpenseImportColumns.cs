namespace Richie.Application.Expenses;

/// <summary>Column names for the bulk expense-upload template and parser (PRD §7.7).</summary>
public static class ExpenseImportColumns
{
    public const string Date = "Date";
    public const string Amount = "Amount";
    public const string Category = "Category";
    public const string SpentBy = "SpentBy";
    public const string SpentFor = "SpentFor";
    public const string Notes = "Notes";

    public static readonly IReadOnlyList<string> All = [Date, Amount, Category, SpentBy, SpentFor, Notes];
}
