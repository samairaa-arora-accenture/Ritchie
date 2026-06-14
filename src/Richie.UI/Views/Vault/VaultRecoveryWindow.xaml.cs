using Richie.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Vault;

public partial class VaultRecoveryWindow : FluentWindow
{
    private readonly VaultRecoveryViewModel _vm;

    public VaultRecoveryWindow(VaultRecoveryViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        DataContext = vm;
        vm.CloseRequested += OnCloseRequested;
        Closed += (_, _) => vm.CloseRequested -= OnCloseRequested;
        Loaded += (_, _) => _vm.Load();
    }

    private void OnCloseRequested(bool success)
    {
        DialogResult = success;
        Close();
    }
}
