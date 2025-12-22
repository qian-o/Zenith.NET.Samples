using System.Diagnostics;
using Zenith.NET;
using Zenith.NET.DirectX12;
using Zenith.NET.Views;
using Zenith.NET.Views.WinForms;

namespace Sample.WinForms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        Renderer.Initialize(zenithView.GraphicsContext = GraphicsContext.CreateDirectX12(true), ZenithView.Output);

        zenithView.GraphicsContext.ValidationMessage += static (sender, args) => Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");
        zenithView.RenderRequested += OnRenderRequested;

        comboBox.Items.AddRange(Renderer.Samples);
    }

    private void OnRenderRequested(object? sender, RenderEventArgs e)
    {
        if (comboBox.SelectedIndex < 0)
        {
            comboBox.SelectedIndex = 0;
        }

        Renderer.Render(Renderer.Samples[comboBox.SelectedIndex], new() { Resolution = new(zenithView.ClientSize.Width, zenithView.ClientSize.Height), TotalSeconds = (float)e.TotalSeconds }, e.FrameBuffer);
    }
}
