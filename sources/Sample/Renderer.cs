using System.Diagnostics;
using System.Numerics;
using System.Runtime.InteropServices;
using Zenith.NET;
using Zenith.NET.Views;
using Buffer = Zenith.NET.Buffer;

#if ANDROID
using Zenith.NET.Vulkan;
#elif IOS || MACCATALYST
using Zenith.NET.Metal;
#else
using Zenith.NET.DirectX12;
#endif

namespace Sample;

public static unsafe class Renderer
{
    private static readonly Dictionary<string, GraphicsPipeline> pipelines = [];

    private static Buffer vertexsBuffer = null!;
    private static Buffer indicesBuffer = null!;
    private static Buffer constantsBuffer = null!;

    static Renderer()
    {
#if ANDROID
        Context = GraphicsContext.CreateVulkan(true);
#elif IOS || MACCATALYST
        Context = GraphicsContext.CreateMetal(true);
#else
        Context = GraphicsContext.CreateDirectX12(true);
#endif
        Context.ValidationMessage += static (_, args) =>
        {
            Debug.WriteLine($"[{args.Severity}] {args.Message}");
            Console.WriteLine($"[{args.Severity}] {args.Message}");
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
            Usages = BufferUsages.Vertex,
            Residency = MemoryResidency.CpuWriteOnly
        });

        fixed (float* pointer = vertices)
        {
            vertexsBuffer.Upload(0, new()
            {
                Pointer = (nint)pointer,
                SizeInBytes = (uint)(sizeof(float) * vertices.Length)
            });
        }

        indicesBuffer = Context.CreateBuffer(new()
        {
            SizeInBytes = (uint)(sizeof(uint) * indices.Length),
            StrideInBytes = sizeof(uint),
            Usages = BufferUsages.Index,
            Residency = MemoryResidency.CpuWriteOnly
        });

        fixed (uint* pointer = indices)
        {
            indicesBuffer.Upload(0, new()
            {
                Pointer = (nint)pointer,
                SizeInBytes = (uint)(sizeof(uint) * indices.Length)
            });
        }

        constantsBuffer = Context.CreateBuffer(new()
        {
            SizeInBytes = (uint)sizeof(Constants),
            StrideInBytes = (uint)sizeof(Constants),
            Usages = BufferUsages.Constant,
            Residency = MemoryResidency.CpuWriteOnly
        });

        using Shader vertexShader = LoadShader(FileAccessService.CombinePaths("Shaders", "Common", "Fullscreen.slang"), "VSMain");

        foreach (string file in FileAccessService.GetFiles("Shaders"))
        {
            if (file.EndsWith(".slang"))
            {
                pipelines[Path.GetFileNameWithoutExtension(file)] = CreateGraphicsPipeline(vertexShader, file);
            }
        }
    }

    public static void Render(string sample, double totalSeconds, CommandBuffer commandBuffer, Texture drawable)
    {
        Constants constants = new()
        {
            Resolution = new(drawable.Desc.Width, drawable.Desc.Height),
            TotalSeconds = (float)totalSeconds
        };

        constantsBuffer.Upload(0, new()
        {
            Pointer = (nint)(&constants),
            SizeInBytes = (uint)sizeof(Constants)
        });

        commandBuffer.BeginRenderPass([ColorAttachment.Clear(drawable, Vector4.Zero)], null);

        commandBuffer.SetPipeline(pipelines[sample]);
        commandBuffer.SetVertexBuffer(vertexsBuffer, 0, 0);
        commandBuffer.SetIndexBuffer(indicesBuffer, 0, IndexFormat.UInt32);
        commandBuffer.SetConstantBuffer(constantsBuffer, 0);

        commandBuffer.DrawIndexed(6, 1, 0, 0, 0);

        commandBuffer.EndRenderPass();
    }

    public static void Destroy()
    {
        foreach (GraphicsPipeline pipeline in pipelines.Values)
        {
            pipeline.Dispose();
        }
        pipelines.Clear();

        constantsBuffer?.Dispose();
        vertexsBuffer?.Dispose();
        indicesBuffer?.Dispose();

        Context.Dispose();
    }

    private static GraphicsPipeline CreateGraphicsPipeline(Shader vertexShader, string file)
    {
        using Shader fragmentShader = LoadShader(file, "PSMain");

        InputLayout inputLayout = new();
        inputLayout.Add(new() { Format = ElementFormat.Float2, Semantic = ElementSemantic.Position });
        inputLayout.Add(new() { Format = ElementFormat.Float2, Semantic = ElementSemantic.TexCoord });

        return Context.CreateGraphicsPipeline(new()
        {
            VertexShader = vertexShader,
            FragmentShader = fragmentShader,
            InputLayouts = [inputLayout],
            PrimitiveTopology = PrimitiveTopology.TriangleList,
            AttachmentFormats = new()
            {
                ColorFormats = [ZenithViewHelper.DrawableFormat],
                SampleCount = SampleCount.Count1
            },
            RenderState = new()
            {
                Rasterizer = RasterizerState.CullNone(),
                DepthStencil = DepthStencilState.DepthNone(),
                Blend = BlendState.Opaque()
            }
        });
    }

    private static Shader LoadShader(string file, string name)
    {
        return Context.CreateShader(new()
        {
            Name = name,
            CodeBytes = FileAccessService.ReadAllBytes(Path.ChangeExtension(file, $".{Context.GraphicsApi.ToString().ToLower()}"))
        });
    }
}

[StructLayout(LayoutKind.Explicit, Size = 16)]
file struct Constants
{
    [FieldOffset(0)]
    public Vector2 Resolution;

    [FieldOffset(8)]
    public float TotalSeconds;
}