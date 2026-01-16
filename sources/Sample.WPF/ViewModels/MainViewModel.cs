using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;

namespace Sample.WPF.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    private string sample = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    [RelayCommand]
    private void RenderRequested(RenderEventArgs e)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, e.TotalSeconds, e.FrameBuffer);
        }
    }
}
