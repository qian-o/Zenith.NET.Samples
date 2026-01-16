using CommunityToolkit.Mvvm.ComponentModel;
using Zenith.NET.Views;

namespace Sample.WinUI.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string Sample { get; set; } = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    public void OnRenderRequested(object? sender, RenderEventArgs e)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, e.TotalSeconds, e.FrameBuffer);
        }
    }
}
