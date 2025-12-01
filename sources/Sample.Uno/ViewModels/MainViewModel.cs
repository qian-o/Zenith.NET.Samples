using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;

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
    private void Render(RenderEventArgs args)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, new() { Resolution = new((float)ActualWidth, (float)ActualHeight), TotalSeconds = (float)args.TotalSeconds }, args.FrameBuffer);
        }
    }
}
