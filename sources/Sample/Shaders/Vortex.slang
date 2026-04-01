/*
    "Vortex" by @XorDev

    X: X.com/XorDev/status/1726103550986469869
    Twigl: twigl.app/?ol=true&ss=-Nj_xbLdHGKREF78Hd96
    shadertoy: https://www.shadertoy.com/view/DlVczV
*/

import "Common/Constants";
import "Common/Vertex";

uniform Constants constants;

static const float PI = 3.14159265359;
static const float TAU = PI * 2.0;
static const float4 PALETTE_PHASE = float4(0.0, 1.0, 2.0, 3.0);

float wrap(float x, float period)
{
    return x - period * floor(x / period);
}

float4 PSMain(Vertex input) : SV_TARGET
{
    float4 color = float4(0.0, 0.0, 0.0, 0.0);

    float2 resolution = constants.Resolution;
    float2 v = resolution;
    float2 p = (input.UV * resolution * 2.0 - resolution) / resolution.y;
    float radius = length(p);
    float time = constants.TotalTime;

    for (float i = 0.2, l; i < 1.0; i += 0.05)
    {
        l = max(length(v), 0.003);
        color += (cos(i * 5.0 + PALETTE_PHASE) + 1.0) * (1.0 + v.y / l) / l;

        float angle = wrap(atan2(p.x, p.y) + i + i * time, TAU) - PI;
        v = float2(angle, 1.0) * radius - i;

        float shiftedX = v.x + i;
        v.x = shiftedX - clamp(shiftedX, -i, i);
    }

    color = tanh(color * 0.01);

    return color;
}
