// Install:
//   dotnet tool install -g dotnet-script
// Run:
//   dotnet script --no-cache CompileShaders.csx

#r "nuget: Zenith.NET.DirectX12, 0.0.7"
#r "nuget: Zenith.NET.Vulkan, 0.0.7"
#r "nuget: Zenith.NET.Metal, 0.0.7"
#r "nuget: Zenith.NET.Extensions.Slang, 0.0.7"

using Zenith.NET;
using Zenith.NET.DirectX12;
using Zenith.NET.Vulkan;
using Zenith.NET.Metal;
using Zenith.NET.Extensions.Slang;

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

// Define backends to compile for.
// Each entry: (backend name, file extension, context factory)
(string Name, string Extension, Func<GraphicsContext> Factory)[] backends =
[
    ("DirectX12", "directx12", () => GraphicsContext.CreateDirectX12(true)),
    ("Vulkan",    "vulkan",    () => GraphicsContext.CreateVulkan(true)),
    ("Metal",     "metal",     () => GraphicsContext.CreateMetal(true)),
];

foreach (var (name, extension, factory) in backends)
{
    GraphicsContext context = null;

    try
    {
        context = factory();
    }
    catch
    {
        Console.WriteLine($"[{name}] Skipped - backend not available on this platform.");
        Console.WriteLine();

        continue;
    }

    Console.WriteLine($"[{name}] Compiling shaders...");

    try
    {
        // Compile the shared vertex shader.
        using Shader vs = context.LoadShaderFromFile(fullscreenPath, "VSMain", ShaderStageFlags.Vertex);

        foreach (string slangFile in slangFiles)
        {
            string fileName = Path.GetFileNameWithoutExtension(slangFile);

            try
            {
                // Compile the pixel shader.
                using Shader ps = context.LoadShaderFromFile(slangFile, "PSMain", ShaderStageFlags.Pixel);

                // Write compiled vertex shader bytes.
                string vsOutput = Path.Combine(shadersDir, "Common", $"Fullscreen.{extension}");
                File.WriteAllBytes(vsOutput, vs.Desc.ShaderBytes);

                // Write compiled pixel shader bytes.
                string psOutput = Path.ChangeExtension(slangFile, $".{extension}");
                File.WriteAllBytes(psOutput, ps.Desc.ShaderBytes);

                Console.WriteLine($"  [{name}] {fileName} -> {Path.GetFileName(psOutput)}");
            }
            catch
            {
                Console.WriteLine($"  [{name}] {fileName} FAILED.");
            }
        }
    }
    finally
    {
        context.Dispose();
    }

    Console.WriteLine($"[{name}] Done.");
    Console.WriteLine();
}

Console.WriteLine("Shader compilation complete.");
