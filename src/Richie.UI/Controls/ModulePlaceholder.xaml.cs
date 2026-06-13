using System.Windows;
using System.Windows.Controls;

namespace Richie.UI.Controls;

/// <summary>
/// Reusable "module not built yet" panel — a title + description. Used by the sidebar
/// module pages until each module's real screen lands in its phase.
/// </summary>
public partial class ModulePlaceholder : UserControl
{
    public static readonly DependencyProperty TitleProperty =
        DependencyProperty.Register(nameof(Title), typeof(string), typeof(ModulePlaceholder),
            new PropertyMetadata(string.Empty));

    public static readonly DependencyProperty DescriptionProperty =
        DependencyProperty.Register(nameof(Description), typeof(string), typeof(ModulePlaceholder),
            new PropertyMetadata(string.Empty));

    public ModulePlaceholder() => InitializeComponent();

    public string Title
    {
        get => (string)GetValue(TitleProperty);
        set => SetValue(TitleProperty, value);
    }

    public string Description
    {
        get => (string)GetValue(DescriptionProperty);
        set => SetValue(DescriptionProperty, value);
    }
}
