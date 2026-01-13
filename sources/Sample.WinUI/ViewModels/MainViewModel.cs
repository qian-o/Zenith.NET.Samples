using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;
using Zenith.NET.Views.WinUI;

namespace Sample.WinUI.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string Sample { get; set; } = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    [ObservableProperty]
    public partial double ActualWidth { get; set; }

    [ObservableProperty]
    public partial double ActualHeight { get; set; }

    [RelayCommand]
    private void Loaded(ZenithView view)
    {
        view.RenderRequested -= OnRenderRequested;
        view.RenderRequested += OnRenderRequested;
    }

    private void OnRenderRequested(object? sender, RenderEventArgs e)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, e.TotalSeconds, e.FrameBuffer);
        }
    }
}
