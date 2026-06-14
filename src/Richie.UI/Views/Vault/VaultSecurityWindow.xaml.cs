using Richie.UI.ViewModels;
using Wpf.Ui.Controls;

namespace Richie.UI.Views.Vault;

public partial class VaultSecurityWindow : FluentWindow
{
    public VaultSecurityWindow(VaultSecurityViewModel vm)
    {
        InitializeComponent();
        DataContext = vm;
        Loaded += (_, _) => vm.Load();
    }
}
