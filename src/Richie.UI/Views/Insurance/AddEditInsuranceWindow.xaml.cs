using Richie.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Insurance;

public partial class AddEditInsuranceWindow : FluentWindow
{
    public AddEditInsuranceViewModel Editor { get; }

    public AddEditInsuranceWindow(AddEditInsuranceViewModel editor)
    {
        InitializeComponent();
        Editor = editor;
        DataContext = editor;
        editor.CloseRequested += OnCloseRequested;
        Closed += (_, _) => editor.CloseRequested -= OnCloseRequested;
    }

    private void OnCloseRequested(bool success)
    {
        DialogResult = success;
        Close();
    }
}
