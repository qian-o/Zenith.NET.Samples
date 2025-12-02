namespace Sample.Maui.Helpers;

internal static class MauiAssetService
{
    public static string[] GetFiles(string path)
#if ANDROID
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
    {
        throw new NotImplementedException();
    }
#else
    {
        return Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, path));
    }
#endif

    public static byte[] ReadAllBytes(string path)
#if ANDROID
    {
        Android.Content.Res.AssetManager manager = Android.App.Application.Context.Assets ?? throw new InvalidOperationException("Assets not available.");

        using Stream stream = manager.Open(path);

        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
    }
#elif IOS || MACCATALYST
    {
        throw new NotImplementedException();
    }
#else
    {
        return File.ReadAllBytes(path);
    }
#endif
}
