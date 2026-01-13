using Zenith.NET.Views.WinForms;

namespace Sample.WinForms;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        Renderer.Initialize(ZenithView.Output);

        ApplicationConfiguration.Initialize();

        Application.Run(new Form1());

        Renderer.Destroy();
    }
}