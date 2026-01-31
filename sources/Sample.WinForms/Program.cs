namespace Sample.WinForms;

internal static class Program
{
    [STAThread]
    public static void Main()
    {
        Renderer.Initialize();

        ApplicationConfiguration.Initialize();

        Application.Run(new Form1());

        Renderer.Destroy();
    }
}