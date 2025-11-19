using Avalonia.Controls;
using Sample.Avalonia.ViewModels;

namespace Sample.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();

        SizeChanged += (_, _) =>
        {
            MainViewModel viewModel = (MainViewModel)DataContext!;
            viewModel.ActualWidth = Bounds.Width;
            viewModel.ActualHeight = Bounds.Height;
        };
    }
}