using System.Diagnostics;
using Zenith.NET;
using Zenith.NET.Views;
using Zenith.NET.Views.WinForms;
using Zenith.NET.Vulkan;

namespace Sample.WinForms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        zenithView.GraphicsContext = GraphicsContext.CreateVulkan(true);
        zenithView.GraphicsContext.ValidationMessage += static (sender, args) => Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(zenithView.GraphicsContext, ZenithView.Output);

        zenithView.RenderRequested += OnRenderRequested;
    }

    private void OnRenderRequested(object? sender, RenderEventArgs e)
    {
        Renderer.Render(Renderer.Samples[0], new() { Resolution = new(zenithView.ClientSize.Width, zenithView.ClientSize.Height), TotalTime = (float)e.TotalTime }, e.FrameBuffer);
    }
}
