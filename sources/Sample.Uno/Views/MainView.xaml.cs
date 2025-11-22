using Sample.Uno.ViewModels;

namespace Sample.Uno.Views;

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
