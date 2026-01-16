using CommunityToolkit.Mvvm.ComponentModel;
using Zenith.NET.Views;

namespace Sample.Uno.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string sample = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    public void RenderRequested(object? sender, RenderEventArgs e)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, e.TotalSeconds, e.FrameBuffer);
        }
    }
}
