using LiveChartsCore;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.SKCharts;
using Richie.Application.Reports;
using SkiaSharp;

namespace Richie.Infrastructure.Reports;

public sealed partial class ReportExporter
{
    private const int ChartWidth = 900;
    private const int ChartHeight = 540;
    private static readonly SKColor LabelColor = new(0x32, 0x32, 0x32);

    // A fixed, distinct palette so slices/bars are visible without relying on a configured LiveCharts theme.
    private static readonly SKColor[] Palette =
    [
        new(0x4F, 0x81, 0xBD), new(0xC0, 0x50, 0x4D), new(0x9B, 0xBB, 0x59), new(0x80, 0x64, 0xA2),
        new(0x4B, 0xAC, 0xC6), new(0xF7, 0x96, 0x46), new(0x2C, 0x4D, 0x75), new(0x77, 0x29, 0x3E),
        new(0x5F, 0x7F, 0x2E), new(0x4D, 0x36, 0x6A)
    ];

    /// <summary>Renders a report chart spec to a PNG image using SkiaSharp in-memory charts (no WPF).</summary>
    public static byte[] RenderChartImage(ReportChart chart)
    {
        InMemorySkiaSharpChart view = chart.Kind == ReportChartKind.Pie
            ? BuildPie(chart.Points)
            : BuildColumn(chart.Points);

        using var stream = new MemoryStream();
        view.SaveImage(stream);
        return stream.ToArray();
    }

    private static SKPieChart BuildPie(IReadOnlyList<ReportChartPoint> points)
    {
        var series = new List<ISeries>(points.Count);
        for (int i = 0; i < points.Count; i++)
        {
            ReportChartPoint pt = points[i];
            series.Add(new PieSeries<double>
            {
                Values = [pt.Value],
                Name = pt.Label,
                Fill = new SolidColorPaint(Palette[i % Palette.Length]),
                DataLabelsPaint = new SolidColorPaint(LabelColor),
                DataLabelsFormatter = _ => pt.Label,
                DataLabelsPosition = PolarLabelsPosition.Outer
            });
        }

        return new SKPieChart
        {
            Width = ChartWidth,
            Height = ChartHeight,
            Background = SKColors.White,
            Series = series
        };
    }

    private static SKCartesianChart BuildColumn(IReadOnlyList<ReportChartPoint> points)
    {
        var column = new ColumnSeries<double>
        {
            Values = points.Select(p => p.Value).ToArray(),
            Name = string.Empty,
            Fill = new SolidColorPaint(Palette[0])
        };

        return new SKCartesianChart
        {
            Width = ChartWidth,
            Height = ChartHeight,
            Background = SKColors.White,
            Series = [column],
            XAxes =
            [
                new Axis
                {
                    Labels = points.Select(p => p.Label).ToArray(),
                    LabelsPaint = new SolidColorPaint(LabelColor),
                    LabelsRotation = 30,
                    TextSize = 12
                }
            ],
            YAxes =
            [
                new Axis { LabelsPaint = new SolidColorPaint(LabelColor), TextSize = 12 }
            ]
        };
    }
}
