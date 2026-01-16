using Sample.Uno.ViewModels;

namespace Sample.Uno.Views;

public sealed partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();
    }

    public MainViewModel ViewModel { get; } = new();
}
