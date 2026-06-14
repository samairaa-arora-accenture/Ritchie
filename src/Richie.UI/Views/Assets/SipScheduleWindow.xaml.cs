using Richie.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Assets;

public partial class SipScheduleWindow : FluentWindow
{
    public SipScheduleViewModel Schedule { get; }

    public SipScheduleWindow(SipScheduleViewModel schedule)
    {
        InitializeComponent();
        Schedule = schedule;
        DataContext = schedule;
        schedule.CloseRequested += OnCloseRequested;
        Closed += (_, _) => schedule.CloseRequested -= OnCloseRequested;
    }

    private void OnCloseRequested(bool success)
    {
        DialogResult = success;
        Close();
    }
}
