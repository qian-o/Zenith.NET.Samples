using Zenith.NET.Views.Maui;

namespace Sample.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        return MauiApp.CreateBuilder()
                      .UseMauiApp<App>()
                      .ConfigureFonts(Configure)
                      .UseZenithView()
                      .Build();
    }

    private static void Configure(IFontCollection fonts)
    {
        fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
        fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
    }
}
