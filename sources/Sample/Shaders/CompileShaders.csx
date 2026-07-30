// Install:
//   dotnet tool install -g dotnet-script
// Run:
//   dotnet script --no-cache CompileShaders.csx

#r "nuget: Zenith.NET, 1.0.0-rc"

using Zenith.NET;

string shadersDir = Path.GetDirectoryName(Path.GetFullPath("CompileShaders.csx"))!;

string[] slangFiles = Directory.GetFiles(shadersDir, "*.slang", SearchOption.TopDirectoryOnly);

if (slangFiles.Length is 0)
{
    Console.WriteLine("No .slang files found in the Shaders directory.");

    return;
}

Console.WriteLine($"Found {slangFiles.Length} shader(s) to compile:");

foreach (string file in slangFiles)
{
    Console.WriteLine($"  - {Path.GetFileName(file)}");
}

Console.WriteLine();

string fullscreenPath = Path.Combine(shadersDir, "Common", "Fullscreen.slang");

(GraphicsApi Api, string Extension)[] backends =
[
    (GraphicsApi.DirectX12, "directx12"),
    (GraphicsApi.Vulkan, "vulkan"),
    (GraphicsApi.Metal, "metal")
];

foreach ((GraphicsApi graphicsApi, string extension) in backends)
{
    Console.WriteLine($"[{graphicsApi}] Compiling shaders...");

    ShaderDesc vertexShader = ZenithCompiler.CompileFromFile(graphicsApi, fullscreenPath, "VSMain", [shadersDir]);
    string vertexOutput = Path.Combine(shadersDir, "Common", $"Fullscreen.{extension}");
    File.WriteAllBytes(vertexOutput, vertexShader.CodeBytes);

    foreach (string slangFile in slangFiles)
    {
        ShaderDesc fragmentShader = ZenithCompiler.CompileFromFile(graphicsApi, slangFile, "PSMain", [shadersDir]);
        string fragmentOutput = Path.ChangeExtension(slangFile, $".{extension}");
        File.WriteAllBytes(fragmentOutput, fragmentShader.CodeBytes);

        Console.WriteLine($"  [{graphicsApi}] {Path.GetFileNameWithoutExtension(slangFile)} -> {Path.GetFileName(fragmentOutput)}");
    }

    Console.WriteLine($"[{graphicsApi}] Done.");
    Console.WriteLine();
}

Console.WriteLine("Shader compilation complete.");
