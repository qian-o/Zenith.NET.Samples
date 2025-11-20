import "Common/Constants";
import "Common/Vertex";

uniform Constants constants;

float mod(float x, float y)
{
    return x - y * floor(x / y);
}

Vertex VSMain(float2 position: POSITION0, float2 uv: TEXCOORD0)
{
    return Vertex(float4(position, 0.0, 1.0), uv);
}

/*
    "Vortex" by @XorDev

    X: X.com/XorDev/status/1726103550986469869
    Twigl: twigl.app/?ol=true&ss=-Nj_xbLdHGKREF78Hd96
    shadertoy: https://www.shadertoy.com/view/DlVczV
*/
float4 PSMain(Vertex input) : SV_TARGET
{
    float4 color = float4(0.0, 0.0, 0.0, 0.0);

    float2 v = constants.Resolution;
    float2 p = (input.UV * constants.Resolution * 2.0 - v) / v.y;

    for (float i = 0.2, l; i < 1.0; i += 0.05)
    {
        color += (cos(i * 5.0 + float4(0.0, 1.0, 2.0, 3.0)) + 1.0) * (1.0 + v.y / (l = length(v) + 0.003)) / l;
        v = float2(mod(atan2(p.x, p.y) + i + i * constants.TotalTime, 6.283185) - 3.141592, 1.0) * length(p) - i;
        v.x -= clamp(v.x += i, -i, i);
    }

    color = tanh(color / 1e2);

    return color;
}
