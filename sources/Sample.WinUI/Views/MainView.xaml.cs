using Microsoft.UI.Xaml.Controls;
using Sample.WinUI.ViewModels;

namespace Sample.WinUI.Views;

public sealed partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();

        SizeChanged += (_, _) =>
        {
            MainViewModel viewModel = (MainViewModel)DataContext;
            viewModel.ActualWidth = ActualWidth;
            viewModel.ActualHeight = ActualHeight;
        };
    }
}
