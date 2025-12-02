using System.Diagnostics;
using Sample.Maui.Views;
using Zenith.NET;
using Zenith.NET.Views.Maui;
#if ANDROID
using Zenith.NET.Vulkan;
#elif IOS || MACCATALYST
using Zenith.NET.Metal;
#else
using Zenith.NET.Vulkan;
#endif

namespace Sample.Maui;

public partial class App : Application
{
    static App()
    {
#if ANDROID
        Context = GraphicsContext.CreateVulkan(true);
#elif IOS || MACCATALYST
        Context = GraphicsContext.CreateMetal(true);
#else
        Context = GraphicsContext.CreateVulkan(true);
#endif
        Context.ValidationMessage += static (sender, args) => Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");

        Renderer.Initialize(Context, ZenithView.Output, true, MauiDirectory.GetFiles, MauiFile.ReadAllBytes);
    }

    public App()
    {
        InitializeComponent();
    }

    public static GraphicsContext Context { get; }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new(new MainView());
    }
}

public static class MauiDirectory
{
#if ANDROID
    public static string[] GetFiles(string path)
    {
        Android.Content.Res.AssetManager manager = Android.App.Application.Context.Assets ?? throw new InvalidOperationException("Assets not available.");

        List<string> files = [];

        foreach (string name in manager.List(path) ?? [])
        {
            string childPath = Path.Combine(path, name);

            if (FileSystem.AppPackageFileExistsAsync(childPath).Result)
            {
                files.Add(childPath);
            }
        }

        return [.. files];
    }
#elif IOS || MACCATALYST
    public static string[] GetFiles(string path)
    {
        throw new NotImplementedException();
    }
#else
    public static string[] GetFiles(string path)
    {
        return Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, path));
    }
#endif
}

public static class MauiFile
{
#if ANDROID
    public static byte[] ReadAllBytes(string path)
    {
        Android.Content.Res.AssetManager manager = Android.App.Application.Context.Assets ?? throw new InvalidOperationException("Assets not available.");

        using Stream stream = manager.Open(path);

        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }
#elif IOS || MACCATALYST
    public static byte[] ReadAllBytes(string path)
    {
        throw new NotImplementedException();
    }
#else
    public static byte[] ReadAllBytes(string path)
    {
        return File.ReadAllBytes(path);
    }
#endif
}