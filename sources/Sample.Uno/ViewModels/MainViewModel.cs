using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;
using Zenith.NET.Views.WinUI;

namespace Sample.Uno.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    public string sample = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    [ObservableProperty]
    public double actualWidth;

    [ObservableProperty]
    public double actualHeight;

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
