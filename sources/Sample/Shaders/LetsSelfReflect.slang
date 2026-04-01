/*
    "Let's self reflect" by mrange

    shadertoy: https://www.shadertoy.com/view/XfyXRV
*/

import "Common/Constants";
import "Common/Vertex";

uniform Constants constants;

static const float PI = 3.141592654;
static const float ROTATION_SPEED = 0.25;

static const float POLY_U = 1.0;
static const float POLY_V = 0.5;
static const float POLY_W = 1.0;
static const int POLY_TYPE = 3;
static const float POLY_ZOOM = 2.0;
static const float INNER_SPHERE = 1.0;
static const float POLY_COSPIN = cos(PI / float(POLY_TYPE));
static const float POLY_SCOSPIN = sqrt(0.75 - POLY_COSPIN * POLY_COSPIN);
static const float3 POLY_NC = float3(-0.5, -POLY_COSPIN, POLY_SCOSPIN);
static const float3 POLY_PAB = float3(0.0, 0.0, 1.0);
static const float3 POLY_PBC_BASE = float3(POLY_SCOSPIN, 0.0, 0.5);
static const float3 POLY_PCA_BASE = float3(0.0, POLY_SCOSPIN, POLY_COSPIN);

static const float REFRACTION_INDEX = 0.9;
static const float INV_REFRACTION_INDEX = 1.0 / REFRACTION_INDEX;

static const int MAX_BOUNCES = 6;
static const float TOLERANCE_INNER = 0.0005;
static const int MAX_MARCHES_INNER = 50;
static const float NORMAL_OFFSET_INNER = 0.005;
static const float TOLERANCE_OUTER = 0.0005;
static const float MAX_RAY_LENGTH_OUTER = 10.0;
static const int MAX_MARCHES_OUTER = 90;
static const float NORMAL_OFFSET_OUTER = 0.005;

static const float3 RAY_ORIGIN = float3(0.0, 1.0, -5.0);
static const float4 HSV2RGB_K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);

static float3x3 gRotation;
static float2 gGlowDistance;

float3 hsv2rgb(float3 c)
{
    float3 p = abs(frac(c.xxx + HSV2RGB_K.xyz) * 6.0 - HSV2RGB_K.www);
    return c.z * lerp(HSV2RGB_K.xxx, clamp(p - HSV2RGB_K.xxx, 0.0, 1.0), c.y);
}

float3 sunDirection()
{
    float t = constants.TotalTime * 0.18;
    return normalize(float3(0.35 * sin(t), 0.85, 1.0 + 0.25 * cos(t * 1.7)));
}

float3 sunColor()
{
    return hsv2rgb(float3(0.06, 0.90, 1e-2));
}

float3 bottomBoxColor()
{
    return hsv2rgb(float3(0.66, 0.80, 0.5));
}

float3 topBoxColor()
{
    return hsv2rgb(float3(0.60, 0.90, 1.0));
}

float3 glowColor0()
{
    return hsv2rgb(float3(0.05, 0.7, 1e-3));
}

float3 glowColor1()
{
    return hsv2rgb(float3(0.95, 0.7, 1e-3));
}

float3 beerColor()
{
    return -hsv2rgb(float3(0.65, 0.7, 2.0));
}

float scenePulse()
{
    return 0.5 + 0.5 * sin(constants.TotalTime * 0.7);
}

float3 prismTint(float phase)
{
    float t = constants.TotalTime;
    return 0.8 + 0.35 * cos(phase + float3(0.0, 1.8, 3.6) + t * float3(0.21, 0.17, 0.13));
}

float3 cameraOrigin()
{
    float t = constants.TotalTime * 0.28;
    float radius = 5.0 + 0.15 * sin(t * 1.3);
    return float3(0.55 * sin(t), 1.0 + 0.18 * sin(t * 0.7), -radius + 0.35 * cos(t));
}

float3 cameraTarget()
{
    float t = constants.TotalTime;
    return float3(0.0, 0.12 * sin(t * 0.33), 0.0);
}

float polyCosPin()
{
    return POLY_COSPIN;
}

float polySCosPin()
{
    return POLY_SCOSPIN;
}

float3 polyNC()
{
    return POLY_NC;
}

float3 polyPAB()
{
    return POLY_PAB;
}

float3 polyPBCBase()
{
    return POLY_PBC_BASE;
}

float3 polyPCABase()
{
    return POLY_PCA_BASE;
}

float3 polyP()
{
    return normalize(POLY_U * polyPAB() + POLY_V * polyPBCBase() + POLY_W * polyPCABase());
}

float3 polyPBC()
{
    return normalize(polyPBCBase());
}

float3 polyPCA()
{
    return normalize(polyPCABase());
}

float3x3 rot(float3 d, float3 z)
{
    float3 v = cross(z, d);
    float c = dot(z, d);
    float k = 1.0 / max(1.0 + c, 1e-4);

    return float3x3(
        v.x * v.x * k + c,     v.y * v.x * k - v.z, v.z * v.x * k + v.y,
        v.x * v.y * k + v.z,   v.y * v.y * k + c,   v.z * v.y * k - v.x,
        v.x * v.z * k - v.y,   v.y * v.z * k + v.x, v.z * v.z * k + c
    );
}

float3 acesApprox(float3 v)
{
    v = max(v, 0.0);
    v *= 0.6;

    const float a = 2.51;
    const float b = 0.03;
    const float c = 2.43;
    const float d = 0.59;
    const float e = 0.14;

    return clamp((v * (a * v + b)) / (v * (c * v + d) + e), 0.0, 1.0);
}

float sphere(float3 p, float r)
{
    return length(p) - r;
}

float box(float2 p, float2 b)
{
    float2 d = abs(p) - b;
    return length(max(d, 0.0)) + min(max(d.x, d.y), 0.0);
}

void polyFold(inout float3 pos)
{
    float3 p = pos;
    float3 nc = polyNC();

    for (int i = 0; i < POLY_TYPE; i++)
    {
        p.xy = abs(p.xy);
        p -= 2.0 * min(0.0, dot(p, nc)) * nc;
    }

    pos = p;
}

float polyPlane(float3 pos)
{
    float d = dot(pos, polyPAB());
    d = max(d, dot(pos, polyPBC()));
    d = max(d, dot(pos, polyPCA()));
    return d;
}

float polyCorner(float3 pos)
{
    return length(pos) - 0.0125;
}

float dot2(float3 p)
{
    return dot(p, p);
}

float polyEdge(float3 pos)
{
    float3 nc = polyNC();
    float dla = dot2(pos - min(0.0, pos.x) * float3(1.0, 0.0, 0.0));
    float dlb = dot2(pos - min(0.0, pos.y) * float3(0.0, 1.0, 0.0));
    float dlc = dot2(pos - min(0.0, dot(pos, nc)) * nc);

    return sqrt(min(min(dla, dlb), dlc)) - 2e-3;
}

float3 shape(float3 pos)
{
    pos = mul(pos, gRotation);
    pos /= POLY_ZOOM;
    polyFold(pos);
    pos -= polyP();

    return float3(polyPlane(pos), polyEdge(pos), polyCorner(pos)) * POLY_ZOOM;
}

float3 renderBackground(float3 ro, float3 rd)
{
    float3 color = float3(0.0, 0.0, 0.0);
    float srd = sign(rd.y);
    float tp = -(ro.y - 6.0) / max(abs(rd.y), 1e-4);
    float3 sunDir = sunDirection();
    float3 sunCol = sunColor();
    float horizon = smoothstep(-0.35, 0.45, rd.y);
    float3 sky = lerp(float3(0.025, 0.03, 0.05), float3(0.09, 0.13, 0.2), horizon);
    float streakCoord = rd.x * 0.7 + rd.z * 0.45 + 0.15 * sin(constants.TotalTime * 0.45 + rd.y * 5.0);
    float streak = pow(saturate(1.0 - abs(streakCoord)), 18.0);
    color += sky;
    color += prismTint(1.2) * streak * 0.08;

    if (srd < 0.0)
    {
        color += bottomBoxColor() * exp(-0.5 * length((ro + tp * rd).xz));
    }

    if (srd > 0.0)
    {
        float3 pos = ro + tp * rd;
        float2 pp = pos.xz;
        float db = box(pp, float2(5.0, 9.0)) - 3.0;

        color += topBoxColor() * rd.y * rd.y * smoothstep(0.25, 0.0, db);
        color += 0.2 * topBoxColor() * exp(-0.5 * max(db, 0.0));
        color += 0.05 * sqrt(topBoxColor()) * max(-db, 0.0);
    }

    color += sunCol / max(1.001 - dot(sunDir, rd), 1e-4);
    color += 0.04 * prismTint(2.4) * pow(saturate(dot(rd, sunDir)), 24.0);
    return color;
}

float distanceInner(float3 p)
{
    float3 ds = shape(p);
    float d2 = ds.y - 5e-3;
    float d0 = min(-ds.x, d2);
    float d1 = sphere(p, INNER_SPHERE);

    gGlowDistance = min(gGlowDistance, float2(d2, d1));
    return min(d0, d1);
}

float rayMarchInner(float3 ro, float3 rd, float tinit)
{
    float t = tinit;
    float2 bestHit = float2(1e10, 0.0);
    int i = 0;

    for (; i < MAX_MARCHES_INNER; i++)
    {
        float d = distanceInner(ro + rd * t);

        if (d < bestHit.x)
        {
            bestHit = float2(d, t);
        }

        if (d < TOLERANCE_INNER)
        {
            break;
        }

        t += d;
    }

    if (i == MAX_MARCHES_INNER)
    {
        t = bestHit.y;
    }

    return t;
}

float3 normalInner(float3 pos)
{
    float2 eps = float2(NORMAL_OFFSET_INNER, 0.0);
    float3 nor;
    nor.x = distanceInner(pos + eps.xyy) - distanceInner(pos - eps.xyy);
    nor.y = distanceInner(pos + eps.yxy) - distanceInner(pos - eps.yxy);
    nor.z = distanceInner(pos + eps.yyx) - distanceInner(pos - eps.yyx);
    return normalize(nor);
}

float3 renderInner(float3 ro, float3 rd, float db)
{
    float3 accum = float3(0.0, 0.0, 0.0);
    float throughput = 1.0;
    float travel = 0.0;

    for (int bounce = 0; bounce < MAX_BOUNCES; bounce++)
    {
        if (throughput < 0.1)
        {
            break;
        }

        gGlowDistance = float2(1e3, 1e3);
        float t2 = rayMarchInner(ro, rd, min(db + 0.05, 0.3));
        float2 gd2 = gGlowDistance;
        travel += t2;

        float3 p2 = ro + rd * t2;
        float3 n2 = normalInner(p2);
        float3 reflected = reflect(rd, n2);
        float3 refracted = refract(rd, n2, INV_REFRACTION_INDEX);
        float fresnel = saturate(1.0 + dot(n2, rd));
        float edgeGlow = exp(-18.0 * max(gd2.x, 0.0));
        float innerShellGlow = exp(-10.0 * max(gd2.y, 0.0));

        float safeThroughput = max(throughput, 1e-3);
        float3 beer = throughput * exp(0.2 * beerColor() * travel);
        float3 spectral = prismTint(travel * 0.25 + float(bounce) * 0.8);
        float glowScale = (1.0 + travel * travel * 4e-2) * 6.0 / max(gd2.x, 5e-4 + travel * travel * 2e-4 / safeThroughput);
        accum += glowColor1() * spectral * beer * glowScale;
        accum += 0.12 * spectral * beer * edgeGlow * (0.3 + 0.7 * innerShellGlow);

        float caustic = pow(saturate(dot(reflected, sunDirection())), 10.0);
        float3 outside = 0.2 * beer * renderBackground(p2, refracted);
        outside += spectral * beer * caustic * (0.08 + 0.04 * scenePulse());

        if (gd2.y <= TOLERANCE_INNER)
        {
            throughput = max(throughput * (1.0 - 0.9 * fresnel), 0.0);
        }
        else
        {
            accum += outside;
            throughput *= 0.8;
        }

        ro = p2;
        rd = reflected;
        db = gd2.x;
    }

    return accum;
}

float distanceOuter(float3 p)
{
    float3 ds = shape(p);
    gGlowDistance = min(gGlowDistance, ds.yz);

    const float shellWidth = 0.02;
    float d0 = min(min(ds.x, ds.y), ds.z);
    float d1 = min(ds.y, ds.z) - shellWidth;

    return min(d0, d1);
}

float rayMarchOuter(float3 ro, float3 rd, float tinit, out int iter)
{
    float t = tinit;
    int i = 0;

    for (; i < MAX_MARCHES_OUTER; i++)
    {
        float d = distanceOuter(ro + rd * t);

        if (d < TOLERANCE_OUTER || t > MAX_RAY_LENGTH_OUTER)
        {
            break;
        }

        t += d;
    }

    iter = i;
    return t;
}

float3 normalOuter(float3 pos)
{
    float2 eps = float2(NORMAL_OFFSET_OUTER, 0.0);
    float3 nor;
    nor.x = distanceOuter(pos + eps.xyy) - distanceOuter(pos - eps.xyy);
    nor.y = distanceOuter(pos + eps.yxy) - distanceOuter(pos - eps.yxy);
    nor.z = distanceOuter(pos + eps.yyx) - distanceOuter(pos - eps.yyx);
    return normalize(nor);
}

float3 renderScene(float3 ro, float3 rd)
{
    int iter = 0;
    float3 skyColor = renderBackground(ro, rd);
    float3 color = skyColor;

    gGlowDistance = float2(1e3, 1e3);
    float t1 = rayMarchOuter(ro, rd, 0.1, iter);
    float2 gd1 = gGlowDistance;
    float3 p1 = ro + t1 * rd;
    float3 n1 = normalOuter(p1);
    float3 reflected = reflect(rd, n1);
    float3 refracted = refract(rd, n1, REFRACTION_INDEX);
    float fresnel = 1.0 + dot(rd, n1);
    fresnel *= fresnel;
    float shellGlow = exp(-24.0 * max(gd1.x, 0.0));
    float cornerGlow = exp(-32.0 * max(gd1.y, 0.0));
    float rim = pow(1.0 - saturate(dot(-rd, n1)), 3.0);

    float ifo = lerp(0.5, 1.0, smoothstep(1.0, 0.9, float(iter) / float(MAX_MARCHES_OUTER)));

    if (t1 < MAX_RAY_LENGTH_OUTER)
    {
        float3 shellTint = prismTint(0.6 + p1.y * 1.7);
        color = renderBackground(p1, reflected) * (0.45 + 0.55 * fresnel) * ifo;
        color += shellTint * shellGlow * (0.08 + 0.16 * rim);

        if (gd1.x > TOLERANCE_OUTER && gd1.y > TOLERANCE_OUTER && any(abs(refracted) > 0.0))
        {
            float3 insideColor = renderInner(p1, refracted, gd1.x);
            color += insideColor * (1.0 - 0.75 * fresnel) * ifo;
        }
    }

    color += (glowColor0() + fresnel * glowColor0()) / max(gd1.x, 3e-4);
    color += prismTint(3.1) * (0.03 + 0.12 * rim) * cornerGlow;
    return color;
}

float3 effect(float2 p)
{
    const float fov = 2.0;
    const float3 up = float3(0.0, 1.0, 0.0);
    float3 ro = cameraOrigin();
    float3 lookAt = cameraTarget();

    float3 ww = normalize(lookAt - ro);
    float3 uu = normalize(cross(up, ww));
    float3 vv = cross(ww, uu);
    float3 rd = normalize(-p.x * uu + p.y * vv + fov * ww);

    float3 color = renderScene(ro, rd);
    color -= 2e-2 * float3(2.0, 3.0, 1.0) * (length(p) + 0.25);
    color = acesApprox(color);
    color *= saturate(1.0 - 0.1 * dot(p, p));

    return sqrt(color);
}

float4 PSMain(Vertex input) : SV_TARGET
{
    float2 q = input.UV;
    float2 p = -1.0 + 2.0 * q;
    p.x *= constants.Resolution.x / constants.Resolution.y;

    float a = constants.TotalTime * ROTATION_SPEED;
    float2 phase0 = float2(sqrt(0.5), 1.0) * a;
    float2 phase1 = float2(sqrt(0.5), 1.0) * (0.913 * a);
    float3 r0 = float3(1.0, sin(phase0));
    float3 r1 = float3(cos(phase1), 1.0);
    gRotation = rot(normalize(r0), normalize(r1));

    float3 color = effect(p);
    return float4(color, 1.0);
}