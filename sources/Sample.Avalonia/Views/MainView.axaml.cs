using Avalonia.Controls;
using Sample.Avalonia.ViewModels;
using Zenith.NET.Views;

namespace Sample.Avalonia.Views;

public partial class MainView : UserControl
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnRenderRequested(object sender, RenderEventArgs e)
    {
        ((MainViewModel)DataContext!).OnRenderRequested(e);
    }
}