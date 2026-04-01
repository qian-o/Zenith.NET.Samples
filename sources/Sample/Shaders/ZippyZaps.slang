/*
    "Zippy Zaps" by SnoopethDuckDuck

    shadertoy: https://www.shadertoy.com/view/XXyGzh
*/

import "Common/Constants";
import "Common/Vertex";

uniform Constants constants;

static const float4 COLOR_SHIFT = float4(1.0, 2.0, 3.0, 0.0);
static const float4 COLOR_ROTATION_PHASE = float4(COLOR_SHIFT.w, COLOR_SHIFT.x, COLOR_SHIFT.z, COLOR_SHIFT.w) * 11.0;
static const int ITERATIONS = 18;

float safeRcp(float value, float epsilon)
{
    if (abs(value) < epsilon)
    {
        return value < 0.0 ? -1.0 / epsilon : 1.0 / epsilon;
    }

    return 1.0 / value;
}

float4 PSMain(Vertex input) : SV_TARGET
{
    float2 res = constants.Resolution;
    float2 uv = input.UV * res;
    float2 p = 0.2 * (uv + uv - res) / res.y;

    float4 col = COLOR_SHIFT;
    float scale = 0.5;
    float t = constants.TotalTime;
    float2 q;

    for (int i = 1; i <= ITERATIONS; i++)
    {
        float fi = float(i);
        t += 1.0;
        scale += 0.03;

        q = cos(t - 7.0 * p * pow(scale, fi)) - 5.0 * p;

           float4 angles = fi + 0.02 * t - COLOR_ROTATION_PHASE;
        float4 ca = cos(angles);
        p = mul(p, float2x2(ca.x, ca.y, ca.z, ca.w));

        float pp = dot(p, p);
           float energy = min(dot(col, col), 800.0);
           float pulse = 4.0 * exp(-energy / 100.0);
           float2 turbulence = tanh(clamp(40.0 * pp * cos(100.0 * p.yx + t), -40.0, 40.0));
           p += turbulence / 200.0 + 0.2 * scale * p + cos(pulse + t) / 300.0;

           float invProj = safeRcp(0.5 - pp, 1e-4);
        float2 sinArg = 1.5 * p * invProj - 9.0 * p.yx + t;
           float denom = max(length((1.0 + fi * dot(q, q)) * sin(sinArg)), 1e-4);
        col += (1.0 + cos(COLOR_SHIFT + t)) / max(denom, 1e-4);
    }

    col = 25.6 / (min(col, 13.0) + 164.0 / max(col, 1e-4)) - dot(p, p) / 250.0;

    return col;
}
