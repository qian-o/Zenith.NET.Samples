using Zenith.NET.Views;
using Zenith.NET.Views.WinForms;

namespace Sample.WinForms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        Renderer.Initialize(ZenithView.Output);

        zenithView.GraphicsContext = Renderer.Context;
        zenithView.RenderRequested += OnRenderRequested;
        zenithView.Disposed += static (_, _) => Renderer.Shutdown();

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
