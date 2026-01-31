using System.Diagnostics;
using System.Numerics;
using Zenith.NET;
using Zenith.NET.DirectX12;
using Zenith.NET.Extensions.Slang;
using Zenith.NET.Metal;
using Zenith.NET.Views;
using Zenith.NET.Vulkan;
using Buffer = Zenith.NET.Buffer;

namespace Sample;

public static unsafe class Renderer
{
    private static readonly Dictionary<string, GraphicsPipeline> pipelines = [];

    private static Buffer vertexsBuffer = null!;
    private static Buffer indicesBuffer = null!;
    private static Buffer constantsBuffer = null!;
    private static ResourceLayout resourceLayout = null!;
    private static ResourceSet resourceSet = null!;

    static Renderer()
    {
        if (OperatingSystem.IsWindows())
        {
            Context = GraphicsContext.CreateDirectX12(true);
        }
        else if (OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
        {
            Context = GraphicsContext.CreateMetal(true);
        }
        else
        {
            Context = GraphicsContext.CreateVulkan(true);
        }

        Context.ValidationMessage += static (sender, args) =>
        {
            Debug.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");
            Console.WriteLine($"[{args.Source} - {args.Severity}] {args.Message}");
        };
    }

    public static GraphicsContext Context { get; }

    public static string[] Samples => [.. pipelines.Keys];

    public static void Initialize()
    {
        foreach (GraphicsPipeline pipeline in pipelines.Values)
        {
            pipeline.Dispose();
        }
        pipelines.Clear();

        resourceSet?.Dispose();
        resourceLayout?.Dispose();
        constantsBuffer?.Dispose();
        vertexsBuffer?.Dispose();
        indicesBuffer?.Dispose();

        float[] vertices =
        [
            -1.0f, -1.0f, 0.0f, 0.0f,
             1.0f, -1.0f, 1.0f, 0.0f,
             1.0f,  1.0f, 1.0f, 1.0f,
            -1.0f,  1.0f, 0.0f, 1.0f
        ];

        uint[] indices =
        [
            0, 1, 2,
            2, 3, 0
        ];

        vertexsBuffer = Context.CreateBuffer(new()
        {
            SizeInBytes = (uint)(sizeof(float) * vertices.Length),
            StrideInBytes = sizeof(float) * 4,
            Flags = BufferUsageFlags.Vertex
        });
        vertexsBuffer.Upload(vertices, 0);

        indicesBuffer = Context.CreateBuffer(new()
        {
            SizeInBytes = (uint)(sizeof(uint) * indices.Length),
            StrideInBytes = sizeof(uint),
            Flags = BufferUsageFlags.Index
        });
        indicesBuffer.Upload(indices, 0);

        constantsBuffer = Context.CreateBuffer(new()
        {
            SizeInBytes = (uint)sizeof(Constants),
            StrideInBytes = (uint)sizeof(Constants),
            Flags = BufferUsageFlags.Constant | BufferUsageFlags.MapWrite
        });

        resourceLayout = Context.CreateResourceLayout(new() { Bindings = [new() { Type = ResourceType.ConstantBuffer, Index = 0, Count = 1, StageFlags = ShaderStageFlags.Pixel }] });
        resourceSet = Context.CreateResourceSet(new() { Layout = resourceLayout, Resources = [constantsBuffer] });

        using Shader vs = GetShader(FileAccessService.CombinePaths("Shaders", "Common", "Fullscreen.slang"), "VSMain", ShaderStageFlags.Vertex);

        foreach (string file in FileAccessService.GetFiles("Shaders"))
        {
            if (file.EndsWith(".slang"))
            {
                pipelines[Path.GetFileNameWithoutExtension(file)] = CreateGraphicsPipeline(vs, file);
            }
        }
    }

    public static void Render(string sample, double totalSeconds, FrameBuffer frameBuffer)
    {
        Constants constants = new()
        {
            Resolution = new(frameBuffer.Width, frameBuffer.Height),
            TotalSeconds = (float)totalSeconds
        };

        constantsBuffer.Upload([constants], 0);

        CommandBuffer commandBuffer = Context.Graphics.CommandBuffer();

        commandBuffer.SetPipeline(pipelines[sample]);
        commandBuffer.SetVertexBuffer(vertexsBuffer, 0, 0);
        commandBuffer.SetIndexBuffer(indicesBuffer, 0, IndexFormat.UInt32);
        commandBuffer.SetResourceSet(resourceSet, 0);

        commandBuffer.BeginRenderPass(frameBuffer, ClearValues.Default, resourceSet);
        commandBuffer.DrawIndexed(6, 1, 0, 0, 0);
        commandBuffer.EndRenderPass();

        commandBuffer.Submit(true);
    }

    public static void Destroy()
    {
        foreach (GraphicsPipeline pipeline in pipelines.Values)
        {
            pipeline.Dispose();
        }
        pipelines.Clear();

        resourceSet?.Dispose();
        resourceLayout?.Dispose();
        constantsBuffer?.Dispose();
        vertexsBuffer?.Dispose();
        indicesBuffer?.Dispose();

        Context.Dispose();
    }

    private static GraphicsPipeline CreateGraphicsPipeline(Shader vs, string file)
    {
        using Shader ps = GetShader(file, "PSMain", ShaderStageFlags.Pixel);

        InputLayout inputLayout = new();
        inputLayout.Add(new() { Format = ElementFormat.Float2, Semantic = ElementSemantic.Position });
        inputLayout.Add(new() { Format = ElementFormat.Float2, Semantic = ElementSemantic.TexCoord });

        return Context.CreateGraphicsPipeline(new()
        {
            RenderStates = new()
            {
                RasterizerState = RasterizerStates.Default,
                DepthStencilState = DepthStencilStates.Default,
                BlendState = BlendStates.Default
            },
            Vertex = vs,
            Pixel = ps,
            ResourceLayouts = [resourceLayout],
            InputLayouts = [inputLayout],
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            Output = ZenithViewHelper.Output
        });
    }

    private static Shader GetShader(string file, string entryPoint, ShaderStageFlags stage)
    {
        if (OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsMacCatalyst())
        {
            return Context.CreateShader(new()
            {
                ShaderBytes = FileAccessService.ReadAllBytes(Path.ChangeExtension(file, $".{Context.Backend.ToString().ToLower()}")),
                EntryPoint = entryPoint,
                Stage = stage
            });
        }
        else
        {
            Shader shader = Context.LoadShaderFromFile(file, entryPoint, stage);

            File.WriteAllBytes(Path.ChangeExtension(file, $".{Context.Backend.ToString().ToLower()}"), shader.Desc.ShaderBytes);

            return shader;
        }
    }
}

file struct Constants
{
    public Vector2 Resolution;

    public float TotalSeconds;
}