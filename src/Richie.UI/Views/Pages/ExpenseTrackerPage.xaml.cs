using System.Windows;
using System.Windows.Controls;
using Microsoft.Extensions.DependencyInjection;
using Richie.Application.Expenses;
using Richie.UI.Services;
using Richie.UI.ViewModels;
using Richie.UI.Views.Assets;
using Richie.UI.Views.Expenses;

namespace Richie.UI.Views.Pages;

public partial class ExpenseTrackerPage : Page
{
    public ExpenseTrackerPage()
    {
        InitializeComponent();
        DataContext = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<ExpenseTrackerViewModel>();
    }

    private ExpenseTrackerViewModel Vm => (ExpenseTrackerViewModel)DataContext;

    private void OnSearchTextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
    {
        if (Vm == null)
            return;

        // Real-time filtering based on the existing SearchText value.
        // Business logic remains in the ViewModel.
        Vm.ApplyFilter();
    }

    private void OnExpenseGridPreviewMouseLeftButtonDown(object? sender, System.Windows.Input.MouseButtonEventArgs e)
    {

        // Ensures single-click interaction with embedded controls (CheckBox / Buttons)
        // even when the DataGrid is IsReadOnly.
        if (e.Handled)
            return;

        if (e.OriginalSource is not DependencyObject original)
            return;

        // If clicking on a CheckBox or Button inside the DataGrid cell template, mark as handled
        // so the control receives the click immediately.
        if (FindAncestor<CheckBox>(original) != null || FindAncestor<Button>(original) != null)
            e.Handled = true;
    }

    private static T? FindAncestor<T>(DependencyObject current) where T : DependencyObject
    {
        var d = current;
        while (d != null)
        {
            if (d is T t)
                return t;
            d = System.Windows.Media.VisualTreeHelper.GetParent(d);
        }

        return null;
    }



    private void OnAddExpense(object sender, RoutedEventArgs e) => OpenEditor(null);

    private void OnIncome(object sender, RoutedEventArgs e)
    {
        var window = ((App)System.Windows.Application.Current).Services.GetRequiredService<IncomeWindow>();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        Vm.Refresh();   // monthly total + chart may have changed
    }

    private void OnBills(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Guid id })
            return;

        var window = ((App)System.Windows.Application.Current).Services.GetRequiredService<BillsWindow>();
        window.Owner = Window.GetWindow(this);
        window.Bills.Initialize(id, "Bills & receipts");
        window.ShowDialog();
    }

    private void OnEditExpense(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { Tag: Guid id })
            OpenEditor(id);
    }

    private void OnDeleteExpense(object sender, RoutedEventArgs e)
    {
        if (sender is not FrameworkElement { Tag: Guid id })
            return;

        if (MessageBox.Show("Delete this expense?", "Confirm delete",
                MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            Vm.Delete(id);
    }

    private void OnRecurring(object sender, RoutedEventArgs e)
    {
        var window = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<RecurringExpensesWindow>();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        Vm.Refresh();
    }

    private void OnAnalytics(object sender, RoutedEventArgs e)
    {
        var window = ((App)System.Windows.Application.Current).Services
            .GetRequiredService<ExpenseAnalyticsWindow>();
        window.Owner = Window.GetWindow(this);
        window.ShowDialog();
        Vm.Refresh();
    }

    private void OnBulkUpload(object sender, RoutedEventArgs e)
    {
        var services = ((App)System.Windows.Application.Current).Services;
        var window = services.GetRequiredService<BulkUploadWindow>();
        window.Owner = Window.GetWindow(this);
        window.Upload.Initialize(services.GetRequiredService<IExpenseImportService>(), "Bulk upload expenses");
        window.ShowDialog();
        if (window.Upload.ImportedAny)
            Vm.Refresh();
    }

    private void OnApplyFilter(object sender, RoutedEventArgs e) => Vm.ApplyFilter();

    private void OnClearFilter(object sender, RoutedEventArgs e) => Vm.ClearFilter();

    private void OpenEditor(Guid? expenseId)
    {
        var window = ((App)System.Windows.Application.Current).Services.GetRequiredService<AddEditExpenseWindow>();
        window.Owner = Window.GetWindow(this);
        window.Editor.Initialize(expenseId);
        if (window.ShowDialog() == true)
            Vm.Refresh();
    }
}
