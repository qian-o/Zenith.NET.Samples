using Zenith.NET.Views;

namespace Sample.WinForms;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        Renderer.Initialize(ZenithViewOutputs.WinForms);

        ApplicationConfiguration.Initialize();

        Application.Run(new Form1());

        Renderer.Destroy();
    }
}