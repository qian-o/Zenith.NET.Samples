// Install:
//   dotnet tool install -g dotnet-script
// Run:
//   dotnet script --no-cache CompileShaders.csx

#r "nuget: Zenith.NET, 1.0.0"

using Zenith.NET;

string shaderDirectory = Path.GetDirectoryName(Path.GetFullPath("CompileShaders.csx"))!;

string[] shaderFiles = Directory.GetFiles(shaderDirectory, "*.slang", SearchOption.TopDirectoryOnly);

if (shaderFiles.Length is 0)
{
    Console.WriteLine("No .slang files found in the Shaders directory.");

    return;
}

Console.WriteLine($"Found {shaderFiles.Length} shader(s) to compile:");

foreach (string shaderFile in shaderFiles)
{
    Console.WriteLine($"  - {Path.GetFileName(shaderFile)}");
}

Console.WriteLine();

string fullscreenShaderPath = Path.Combine(shaderDirectory, "Common", "Fullscreen.slang");

GraphicsApi[] graphicsApis =
[
    GraphicsApi.DirectX12,
    GraphicsApi.Metal,
    GraphicsApi.Vulkan
];

foreach (GraphicsApi graphicsApi in graphicsApis)
{
    Console.WriteLine($"[{graphicsApi}] Compiling shaders...");

    string extension = graphicsApi.ToString().ToLowerInvariant();
    ShaderDesc vertexShader;

    try
    {
        vertexShader = ZenithCompiler.CompileFromFile(graphicsApi, fullscreenShaderPath, "VSMain", [shaderDirectory]);
    }
    catch (Exception exception) when (IsCompilerUnavailable(exception))
    {
        Console.WriteLine($"[{graphicsApi}] Skipped: required compiler is unavailable in this environment.");
        Console.WriteLine();

        continue;
    }

    string vertexShaderOutput = Path.Combine(shaderDirectory, "Common", $"Fullscreen.{extension}");
    File.WriteAllBytes(vertexShaderOutput, vertexShader.CodeBytes);

    foreach (string shaderFile in shaderFiles)
    {
        ShaderDesc fragmentShader = ZenithCompiler.CompileFromFile(graphicsApi, shaderFile, "PSMain", [shaderDirectory]);
        string fragmentShaderOutput = Path.ChangeExtension(shaderFile, $".{extension}");
        File.WriteAllBytes(fragmentShaderOutput, fragmentShader.CodeBytes);

        Console.WriteLine($"  [{graphicsApi}] {Path.GetFileNameWithoutExtension(shaderFile)} -> {Path.GetFileName(fragmentShaderOutput)}");
    }

    Console.WriteLine($"[{graphicsApi}] Done.");
    Console.WriteLine();
}

Console.WriteLine("Shader compilation complete.");

static bool IsCompilerUnavailable(Exception exception)
{
    for (Exception currentException = exception; currentException is not null; currentException = currentException.InnerException)
    {
        if (currentException is DllNotFoundException or PlatformNotSupportedException)
        {
            return true;
        }

        if (currentException.Message.Contains("failed to load downstream compiler", StringComparison.OrdinalIgnoreCase) ||
            currentException.Message.Contains("pass-through compiler not found", StringComparison.OrdinalIgnoreCase) ||
            currentException.Message.Contains("failed to load dynamic library", StringComparison.OrdinalIgnoreCase))
        {
            return true;
        }
    }

    return false;
}
