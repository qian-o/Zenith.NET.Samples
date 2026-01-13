using Zenith.NET.Views;

namespace Sample.WinForms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();

        zenithView.GraphicsContext = Renderer.Context;
        zenithView.RenderRequested += OnRenderRequested;

        comboBox.Items.AddRange(Renderer.Samples);
    }

    private void OnRenderRequested(object? sender, RenderEventArgs e)
    {
        if (comboBox.SelectedIndex < 0)
        {
            comboBox.SelectedIndex = 0;
        }

        Renderer.Render(Renderer.Samples[comboBox.SelectedIndex], e.TotalSeconds, e.FrameBuffer);
    }
}
