namespace Sample;

internal static class FileAccessService
{
    public static string CombinePaths(params string[] paths)
    {
#if ANDROID || IOS || MACCATALYST
        return Path.Combine(paths);
#else
        return Path.Combine([AppContext.BaseDirectory, .. paths]);
#endif
    }

    public static IEnumerable<string> GetFiles(string path)
    {
#if ANDROID
        Android.Content.Res.AssetManager manager = Application.Context.Assets ?? throw new InvalidOperationException("Assets not available.");

        List<string> files = [];

        foreach (string name in manager.List(path) ?? [])
        {
            string childPath = Path.Combine(path, name);

            if (!(manager.List(childPath)?.Length > 0))
            {
                files.Add(childPath);
            }
        }

        return [.. files];
#elif IOS || MACCATALYST
        return Directory.GetFiles(Path.Combine(NSBundle.MainBundle.ResourcePath, path));
#else
        return Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, path));
#endif
    }

    public static byte[] ReadAllBytes(string path)
    {
#if ANDROID
        Android.Content.Res.AssetManager manager = Application.Context.Assets ?? throw new InvalidOperationException("Assets not available.");

        using Stream stream = manager.Open(path);

        using MemoryStream memoryStream = new();
        stream.CopyTo(memoryStream);

        return memoryStream.ToArray();
#elif IOS || MACCATALYST
        return File.ReadAllBytes(Path.Combine(NSBundle.MainBundle.ResourcePath, path));
#else
        return File.ReadAllBytes(path);
#endif
    }
}
