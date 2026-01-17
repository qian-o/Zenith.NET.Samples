import "Vertex";

Vertex VSMain(float2 position: POSITION0, float2 uv: TEXCOORD0)
{
    return Vertex(float4(position, 0.0, 1.0), uv);
}
