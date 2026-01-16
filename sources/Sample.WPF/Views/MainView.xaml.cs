using System.Windows.Controls;
using Sample.WPF.ViewModels;
using Zenith.NET.Views;

namespace Sample.WPF.Views;

public partial class MainView : Page
{
    public MainView()
    {
        InitializeComponent();
    }

    private void OnRenderRequested(object sender, RenderEventArgs e)
    {
        ((MainViewModel)DataContext).OnRenderRequested(e);
    }
}
