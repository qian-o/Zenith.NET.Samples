using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;

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
    private void Render(RenderEventArgs args)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, new() { Resolution = new((float)ActualWidth, (float)ActualHeight), TotalSeconds = (float)args.TotalSeconds }, args.FrameBuffer);
        }
    }
}
