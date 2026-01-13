using System.ComponentModel;
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
        comboBox.SelectedIndex = 0;
    }

    [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
    public int SelectedIndex { get; set; }

    private void OnSelectedIndexChanged(object sender, EventArgs e)
    {
        if (comboBox.SelectedIndex < 0)
        {
            comboBox.SelectedIndex = 0;

            return;
        }

        SelectedIndex = comboBox.SelectedIndex;
    }

    private void OnRenderRequested(object? sender, RenderEventArgs e)
    {
        Renderer.Render(Renderer.Samples[SelectedIndex], e.TotalSeconds, e.FrameBuffer);
    }
}
