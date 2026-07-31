using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Zenith.NET.Views;

namespace Sample.Avalonia.ViewModels;

public partial class MainViewModel : ObservableRecipient
{
    [ObservableProperty]
    public partial string Sample { get; set; } = Renderer.Samples.FirstOrDefault() ?? string.Empty;

    [RelayCommand]
    private void RenderRequested(RenderEventArgs e)
    {
        if (!string.IsNullOrEmpty(Sample))
        {
            Renderer.Render(Sample, e.TotalSeconds, e.CommandBuffer, e.Drawable);
        }
    }
}
