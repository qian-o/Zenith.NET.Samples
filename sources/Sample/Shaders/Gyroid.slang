/*
    "Gyroid" by tubeman

    shadertoy: https://www.shadertoy.com/view/tXtyW8
*/

import "Common/Constants";
import "Common/Vertex";

uniform Constants constants;

static const float PI = 3.14159265359;
static const float FAR = 30.0;
static const float EPSILON = 0.001;
static const int MAX_ITER = 150;

static int m = 0;

float2x2 rot(float a)
{
    float c = cos(a), s = sin(a);
    return float2x2(c, -s, s, c);
}

float3x3 lookAt(float3 dir)
{
    float3 up = float3(0.0, 1.0, 0.0);
    float3 rt = normalize(cross(dir, up));
    return float3x3(rt, cross(rt, dir), dir);
}

float gyroid(float3 p)
{
    return dot(cos(p), sin(p.zxy)) + 1.0;
}

float map(float3 p)
{
    float r = 1e5, d;

    d = gyroid(p);
    if (d < r)
    {
        r = d;
        m = 1;
    }

    d = gyroid(p - float3(0, 0, PI));
    if (d < r)
    {
        r = d;
        m = 2;
    }

    return r;
}

float raymarch(float3 ro, float3 rd)
{
    float t = 0.0;

    for (int i = 0; i < MAX_ITER; i++)
    {
        float d = map(ro + rd * t);

        if (abs(d) < EPSILON)
        {
            break;
        }

        t += d;

        if (t > FAR)
        {
            break;
        }
    }

    return t;
}

float getAO(float3 p, float3 sn)
{
    float occ = 0.0;

    for (float i = 0.0; i < 4.0; i++)
    {
        float t = i * 0.08;
        float d = map(p + sn * t);

        occ += t - d;
    }

    return clamp(1.0 - occ, 0.0, 1.0);
}

float3 getNormal(float3 p)
{
    float2 e = float2(0.5773, -0.5773) * 0.001;

    return normalize(e.xyy * map(p + e.xyy) + e.yyx * map(p + e.yyx) + e.yxy * map(p + e.yxy) + e.xxx * map(p + e.xxx));
}

float3 trace(float3 ro, float3 rd)
{
    float3 color = float3(0.0, 0.0, 0.0);
    float3 throughput = float3(1.0, 1.0, 1.0);

    for (int bounce = 0; bounce < 2; bounce++)
    {
        float d = raymarch(ro, rd);

        if (d > FAR)
        {
            break;
        }

        float fog = 1.0 - exp(-0.008 * d * d);
        color += throughput * fog * float3(0.0, 0.0, 0.0);
        throughput *= 1.0 - fog;

        float3 p = ro + rd * d;
        float3 sn = normalize(getNormal(p) + pow(abs(cos(p * 64.0)), float3(16.0, 16.0, 16.0)) * 0.1);

        float3 lp = float3(10.0, -10.0, -10.0 + ro.z);
        float3 ld = normalize(lp - p);
        float diff = max(0.0, 0.5 + 2.0 * dot(sn, ld));
        float diff2 = pow(length(sin(sn * 2.0) * 0.5 + 0.5), 2.0);
        float diff3 = max(0.0, 0.5 + 0.5 * dot(sn, float3(0.0, 1.0, 0.0)));

        float spec = max(0.0, dot(reflect(-ld, sn), -rd));
        float fres = 1.0 - max(0.0, dot(-rd, sn));
        float3 col = float3(0.0, 0.0, 0.0);
        float3 alb = float3(0.0, 0.0, 0.0);

        col += float3(0.4, 0.6, 0.9) * diff;
        col += float3(0.5, 0.1, 0.1) * diff2;
        col += float3(0.9, 0.1, 0.4) * diff3;
        col += float3(0.3, 0.25, 0.25) * pow(spec, 4.0) * 8.0;

        float freck = dot(cos(p * 23.0), float3(1.0, 1.0, 1.0));

        if (m == 1)
        {
            alb = float3(0.2, 0.1, 0.9);
            alb *= max(0.6, step(2.5, freck));
        }

        if (m == 2)
        {
            alb = float3(0.6, 0.3, 0.1);
            alb *= max(0.8, step(-2.5, freck));
        }

        col *= alb;
        col *= getAO(p, sn);
        color += throughput * col;

        rd = reflect(rd, sn);
        ro = p + sn * 0.01;
        throughput *= 0.9 * pow(fres, 1.0);
    }

    return color;
}

float4 PSMain(Vertex input) : SV_TARGET
{
    float2 resolution = constants.Resolution;
    float2 fragCoord = input.UV * resolution;
    float time = constants.TotalTime;

    float2 uv = (fragCoord - resolution * 0.5) / resolution.y;

    float3 ro = float3(PI / 2.0, 0.0, -time * 0.5);
    float3 rd = normalize(float3(uv, -0.5));

    rd.xy = mul(rot(sin(time * 0.2)), rd.xy);
    float3 ta = float3(cos(time * 0.4), sin(time * 0.4), 4.0);
    rd = mul(lookAt(normalize(ta)), rd);

    float3 color = trace(ro, rd);

    color *= smoothstep(0.0, 1.0, 1.2 - length(uv * 0.9));
    color = pow(color, float3(0.4545, 0.4545, 0.4545));

    return float4(color, 1.0);
}
