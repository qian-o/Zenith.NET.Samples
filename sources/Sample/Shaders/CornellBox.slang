import "Common/Constants";
import "Common/Vertex";

uniform Constants constants;

float4 PSMain(Vertex input) : SV_TARGET
{
    float4 color = float4(1.0, 0.0, 0.0, 0.0);

    return color;
}
