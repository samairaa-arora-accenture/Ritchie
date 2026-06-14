using System.Collections.ObjectModel;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using Richie.Application.Audit;
using Richie.Domain.Audit;

namespace Richie.UI.ViewModels;

public partial class HealthAuditViewModel : ObservableObject
{
    private readonly IHealthAuditService _audit;

    public sealed record BenchmarkDisplay(
        string ClassName, string ActualText, string RecommendedText, string StatusText, Brush StatusBrush);

    [ObservableProperty] private bool _hasAssets;
    [ObservableProperty] private bool _scoresAreInterim;

    [ObservableProperty] private int _healthScore;
    [ObservableProperty] private string _healthRating = string.Empty;
    [ObservableProperty] private Brush _healthBrush = Brushes.Gray;
    [ObservableProperty] private ObservableCollection<ScoreFactor> _healthFactors = [];

    [ObservableProperty] private int _riskScore;
    [ObservableProperty] private string _riskBand = string.Empty;
    [ObservableProperty] private string _riskInterpretation = string.Empty;
    [ObservableProperty] private Brush _riskBrush = Brushes.Gray;

    [ObservableProperty] private string _ageBandName = string.Empty;
    [ObservableProperty] private ObservableCollection<BenchmarkDisplay> _benchmark = [];
    [ObservableProperty] private string _diversificationText = string.Empty;
    [ObservableProperty] private ObservableCollection<GoalProgressRow> _goals = [];
    [ObservableProperty] private bool _hasGoals;
    [ObservableProperty] private bool _noGoals;
    [ObservableProperty] private ObservableCollection<string> _coverageGaps = [];
    [ObservableProperty] private bool _hasCoverageGaps;
    [ObservableProperty] private bool _coverageOk;
    [ObservableProperty] private ObservableCollection<string> _suggestions = [];

    public string HealthScaleLegend => "Scale: 80–100 Excellent · 60–79 Good · below 60 Needs attention.";
    public string RiskScaleLegend => "Scale: ≤20 Low · ≤40 Moderate · ≤60 Moderately High · ≤80 High · >80 Very High.";
    public string InterimNotice =>
        "Interim scoring — the Risk Score, Health Score and age-group benchmarks are placeholder formulas pending team finalization (PRD §22).";

    private static readonly Brush Red = new SolidColorBrush(Color.FromRgb(0xC4, 0x2B, 0x1C));
    private static readonly Brush Amber = new SolidColorBrush(Color.FromRgb(0x9D, 0x5D, 0x00));
    private static readonly Brush Green = new SolidColorBrush(Color.FromRgb(0x0F, 0x7B, 0x0F));

    public HealthAuditViewModel(IHealthAuditService audit) => _audit = audit;

    public void Load()
    {
        HealthAuditReport r = _audit.GetReport();

        HasAssets = r.HasAssets;
        ScoresAreInterim = r.ScoresAreInterim;

        HealthScore = r.HealthScore;
        HealthRating = r.HealthRating;
        HealthBrush = r.HealthScore >= 80 ? Green : r.HealthScore >= 60 ? Amber : Red;
        HealthFactors = new ObservableCollection<ScoreFactor>(r.HealthFactors);

        RiskScore = r.RiskScore;
        RiskBand = r.RiskBand;
        RiskInterpretation = r.RiskInterpretation;
        RiskBrush = r.RiskScore <= 40 ? Green : r.RiskScore <= 60 ? Amber : Red;

        AgeBandName = r.AgeBandName;
        Benchmark = new ObservableCollection<BenchmarkDisplay>(r.Benchmark.Select(ToDisplay));
        DiversificationText = $"{r.DistinctClassCount} of 4 broad asset classes represented" +
            (r.MissingClasses.Count > 0 ? $" — missing: {string.Join(", ", r.MissingClasses)}." : ".");

        Goals = new ObservableCollection<GoalProgressRow>(r.Goals);
        HasGoals = r.Goals.Count > 0;
        NoGoals = !HasGoals;
        CoverageGaps = new ObservableCollection<string>(r.CoverageGaps);
        HasCoverageGaps = r.CoverageGaps.Count > 0;
        CoverageOk = !HasCoverageGaps;
        Suggestions = new ObservableCollection<string>(r.Suggestions);
    }

    private BenchmarkDisplay ToDisplay(BenchmarkRow row)
    {
        (string text, Brush brush) = row.Status switch
        {
            BenchmarkStatus.OnTarget => ("On target", Green),
            BenchmarkStatus.Over => ("Over", Amber),
            _ => ("Under", Amber)
        };
        return new BenchmarkDisplay(row.ClassName, $"{row.ActualPercent:0.#}%", $"{row.RecommendedPercent:0}%", text, brush);
    }
}
