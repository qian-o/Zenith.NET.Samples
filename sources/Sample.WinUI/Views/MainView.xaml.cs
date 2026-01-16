using Microsoft.UI.Xaml.Controls;
using Sample.WinUI.ViewModels;

namespace Sample.WinUI.Views;

public sealed partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();
    }

    public MainViewModel ViewModel { get; } = new();
}
