using Richie.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Profile;

public partial class ChangePasswordWindow : FluentWindow
{
    public ChangePasswordViewModel Editor { get; }

    public ChangePasswordWindow(ChangePasswordViewModel editor)
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
