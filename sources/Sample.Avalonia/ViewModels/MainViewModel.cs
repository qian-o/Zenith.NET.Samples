using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;
using Zenith.NET.Views.Avalonia;

namespace Sample.Avalonia.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string sample = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    [ObservableProperty]
    private double actualWidth;

    [ObservableProperty]
    private double actualHeight;

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
