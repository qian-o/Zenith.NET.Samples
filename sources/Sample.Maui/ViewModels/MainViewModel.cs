using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;

namespace Sample.Maui.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string Sample { get; set; } = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    [RelayCommand]
    private void Render(RenderEventArgs args)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, args.TotalSeconds, args.FrameBuffer);
        }
    }
}