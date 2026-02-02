using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SharpMetal.Metal;
using Zenith.NET.Views;

namespace Sample.Maui.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    public MainViewModel()
    {
#if IOS || MACCATALYST
        var device = MTLDevice.CreateSystemDefaultDevice();

        Console.WriteLine($"Running on device: {device.Name}");
#endif
    }

    [ObservableProperty]
    public partial string Sample { get; set; } = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    [RelayCommand]
    private void RenderRequested(RenderEventArgs args)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, args.TotalSeconds, args.FrameBuffer);
        }
    }
}