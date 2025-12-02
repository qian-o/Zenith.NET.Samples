using Zenith.NET;
using Zenith.NET.Extensions.Slang;
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

    public static GraphicsContext? Context { get; private set; }

    public static string[] Samples => [.. pipelines.Keys];

    public static void Initialize(GraphicsContext context, Output output, bool useCacheShaders, Func<string, string[]>? getFiles = null, Func<string, byte[]>? readAllBytes = null)
    {
        getFiles ??= path => Directory.GetFiles(Path.Combine(AppContext.BaseDirectory, path));
        readAllBytes ??= File.ReadAllBytes;

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

        Context = context;

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
            Flags = BufferUsageFlags.Constant | BufferUsageFlags.Dynamic
        });

        resourceLayout = Context.CreateResourceLayout(new() { Bindings = [new() { Type = ResourceType.ConstantBuffer, Index = 0, Count = 1, StageFlags = ShaderStageFlags.Pixel }] });
        resourceSet = Context.CreateResourceSet(new() { Layout = resourceLayout, Resources = [constantsBuffer] });

        foreach (string file in getFiles("Shaders"))
        {
            if (file.EndsWith(".slang"))
            {
                pipelines[Path.GetFileNameWithoutExtension(file)] = CreateGraphicsPipeline(output, file, useCacheShaders, readAllBytes);
            }
        }
    }

    public static void Render(string sample, Constants constants, FrameBuffer frameBuffer)
    {
        if (Context is null)
        {
            throw new InvalidOperationException("Renderer is not initialized.");
        }

        constantsBuffer.Upload([constants], 0);

        CommandBuffer commandBuffer = Context.Graphics.CommandBuffer();

        commandBuffer.BindFrameBuffer(frameBuffer, ClearValues.Default);

        commandBuffer.BindPipeline(pipelines[sample]);
        commandBuffer.BindVertexBuffer(vertexsBuffer, 0, 0);
        commandBuffer.BindIndexBuffer(indicesBuffer, 0, IndexFormat.UInt32);
        commandBuffer.BindResourceSet(resourceSet, 0);
        commandBuffer.DrawIndexed(6, 1, 0, 0, 0);

        commandBuffer.Submit();

        Context.Graphics.WaitIdle();
    }

    private static GraphicsPipeline CreateGraphicsPipeline(Output output, string file, bool useCacheShaders, Func<string, byte[]> readAllBytes)
    {
        if (Context is null)
        {
            throw new InvalidOperationException("Renderer is not initialized.");
        }

        Shader? vs = null;
        Shader? ps = null;

        try
        {
            if (useCacheShaders)
            {
                vs = Context.CreateShader(new()
                {
                    ShaderBytes = readAllBytes(Path.ChangeExtension(file, $".vs_{Context.Backend.ToString().ToLower()}")),
                    EntryPoint = "VSMain",
                    Stage = ShaderStageFlags.Vertex
                });

                ps = Context.CreateShader(new()
                {
                    ShaderBytes = readAllBytes(Path.ChangeExtension(file, $".ps_{Context.Backend.ToString().ToLower()}")),
                    EntryPoint = "PSMain",
                    Stage = ShaderStageFlags.Pixel
                });
            }
            else
            {
                vs = Context.LoadShaderFromFile(file, "VSMain", ShaderStageFlags.Vertex);
                ps = Context.LoadShaderFromFile(file, "PSMain", ShaderStageFlags.Pixel);

                File.WriteAllBytes(Path.ChangeExtension(file, $".vs_{Context.Backend.ToString().ToLower()}"), vs.Desc.ShaderBytes);
                File.WriteAllBytes(Path.ChangeExtension(file, $".ps_{Context.Backend.ToString().ToLower()}"), ps.Desc.ShaderBytes);
            }

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
                Output = output
            });
        }
        finally
        {
            vs?.Dispose();
            ps?.Dispose();
        }
    }
}
