using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;

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
    private void Render(RenderEventArgs args)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, new() { Resolution = new((float)ActualWidth, (float)ActualHeight), TotalTime = (float)args.TotalTime }, args.FrameBuffer);
        }
    }
}
