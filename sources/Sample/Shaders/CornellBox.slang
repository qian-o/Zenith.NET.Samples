import "Common/Constants";
import "Common/Vertex";

uniform Constants constants;

// =============================================================================
// Configuration
// =============================================================================

static const float PI = 3.14159265359;
static const float EPSILON = 0.001;
static const float MAX_DIST = 2000.0;
static const int MAX_ITER = 128;
static const float EXPOSURE = 34.0;
static const float GAMMA = 2.1;

// =============================================================================
// Object IDs
// =============================================================================

static const float OBJ_FLOOR = 1.0;
static const float OBJ_CEILING = 2.0;
static const float OBJ_BACKWALL = 3.0;
static const float OBJ_LEFTWALL = 4.0;
static const float OBJ_RIGHTWALL = 5.0;
static const float OBJ_LIGHT = 6.0;
static const float OBJ_BLOCK = 7.0;
static const float OBJ_SPHERE = 8.0;

// =============================================================================
// Scene Parameters
// =============================================================================

static const float3 LIGHT_COLOR = float3(16.86, 10.76, 3.7);
static const float3 LIGHT_CENTER = float3(278.0, 548.8, 320.0);
static const float LIGHT_ORBIT_RADIUS = 120.0;
static const float LIGHT_ORBIT_SPEED = 0.8;
static const float3 LEFT_WALL_COLOR = float3(0.611, 0.0555, 0.062);
static const float3 RIGHT_WALL_COLOR = float3(0.117, 0.4125, 0.115);
static const float3 WHITE_COLOR = float3(0.73, 0.735, 0.73);

static const float3 SPHERE_POS = float3(186.0, 248.0, 169.5);
static const float SPHERE_RAD = 80.0;
static const float SPHERE_IOR = 1.5;

static const float3 CAM_POS = float3(278.0, 273.0, -801.4);
static const float3 CAM_TARGET = float3(278.0, 274.4, 279.6);

// =============================================================================
// Utility Functions
// =============================================================================

float3 rotateY(float3 p, float a)
{
    float c = cos(a), s = sin(a);
    return float3(c * p.x + s * p.z, p.y, -s * p.x + c * p.z);
}

float3 rotateX(float3 p, float a)
{
    float c = cos(a), s = sin(a);
    return float3(p.x, c * p.y - s * p.z, s * p.y + c * p.z);
}

float sdBox(float3 p, float3 b)
{
    float3 d = abs(p) - b;
    return min(max(d.x, max(d.y, d.z)), 0.0) + length(max(d, 0.0));
}

float3 getLightPos(float time)
{
    float angle = time * LIGHT_ORBIT_SPEED;
    return LIGHT_CENTER + float3(cos(angle) * LIGHT_ORBIT_RADIUS, 0.0, sin(angle) * LIGHT_ORBIT_RADIUS);
}

// =============================================================================
// Scene SDF
// =============================================================================

float sdfBlocks(float3 p)
{
    float shortBlock = sdBox(rotateY(p - float3(186.0, 82.5, 169.5), -0.29718), float3(83.5, 83.5, 82.5));
    float tallBlock = sdBox(rotateY(p - float3(368.5, 165.0, 351.5), 0.30072), float3(87.0, 165.0, 83.5));
    return min(shortBlock, tallBlock);
}

float sdfSphere(float3 p)
{
    return length(p - SPHERE_POS) - SPHERE_RAD;
}

float2 sdfScene(float3 p, bool includeSphere, float3 lightPos)
{
    float2 res = float2(OBJ_FLOOR, p.y);

    float ceiling = 548.8 - p.y;
    if (ceiling < res.y)
    {
        res = float2(OBJ_CEILING, ceiling);
    }

    float back = 559.2 - p.z;
    if (back < res.y)
    {
        res = float2(OBJ_BACKWALL, back);
    }

    float left = 556.0 - p.x;
    if (left < res.y)
    {
        res = float2(OBJ_LEFTWALL, left);
    }

    float right = p.x;
    if (right < res.y)
    {
        res = float2(OBJ_RIGHTWALL, right);
    }

    float light = sdBox(p - lightPos, float3(65.0, 0.05, 65.0));
    if (light < res.y)
    {
        res = float2(OBJ_LIGHT, light);
    }

    float blocks = sdfBlocks(p);
    if (blocks < res.y)
    {
        res = float2(OBJ_BLOCK, blocks);
    }

    if (includeSphere)
    {
        float sphere = sdfSphere(p);
        if (sphere < res.y)
        {
            res = float2(OBJ_SPHERE, sphere);
        }
    }

    return res;
}

float3 calcNormal(float3 p, bool includeSphere, float3 lightPos)
{
    float2 e = float2(0.0002, 0.0);
    return normalize(
        float3(sdfScene(p + e.xyy, includeSphere, lightPos).y - sdfScene(p - e.xyy, includeSphere, lightPos).y,
               sdfScene(p + e.yxy, includeSphere, lightPos).y - sdfScene(p - e.yxy, includeSphere, lightPos).y,
               sdfScene(p + e.yyx, includeSphere, lightPos).y - sdfScene(p - e.yyx, includeSphere, lightPos).y));
}

// =============================================================================
// Ray Marching
// =============================================================================

float2 raymarch(float3 ro, float3 rd, bool includeSphere, float3 lightPos)
{
    float t = 0.0;
    for (int i = 0; i < MAX_ITER; i++)
    {
        float2 res = sdfScene(ro + rd * t, includeSphere, lightPos);
        if (res.y < EPSILON)
        {
            return float2(res.x, t);
        }
        t += res.y;
        if (t > MAX_DIST)
        {
            break;
        }
    }
    return float2(-1.0, t);
}

// =============================================================================
// Lighting
// =============================================================================

float calcSoftShadow(float3 ro, float3 rd, float maxDist)
{
    float res = 1.0;
    float t = 0.5;
    float ph = 1e10;

    for (int i = 0; i < 32; i++)
    {
        float h = sdfBlocks(ro + rd * t);
        if (h < EPSILON)
        {
            return 0.0;
        }

        float y = h * h / (2.0 * ph);
        float d = sqrt(h * h - y * y);
        res = min(res, 8.0 * d / max(0.0, t - y));
        ph = h;

        t += h * 0.5;
        if (t > maxDist)
        {
            break;
        }
    }
    return saturate(res);
}

float calcAO(float3 p, float3 n, float3 lightPos)
{
    float occ = 0.0;
    for (int i = 1; i <= 3; i++)
    {
        float d = 8.0 * float(i);
        occ += (d - sdfScene(p + n * d, true, lightPos).y) / d;
    }
    return saturate(1.0 - occ * 0.12);
}

float3 getMaterialColor(float id)
{
    if (id == OBJ_LEFTWALL)
    {
        return LEFT_WALL_COLOR;
    }
    if (id == OBJ_RIGHTWALL)
    {
        return RIGHT_WALL_COLOR;
    }
    return WHITE_COLOR;
}

float3 shade(float3 p, float3 n, float id, float3 lightPos)
{
    if (id == OBJ_LIGHT)
    {
        return WHITE_COLOR * LIGHT_COLOR;
    }

    float3 albedo = getMaterialColor(id);

    // Area light approximation
    float3 lp = lightPos;
    lp.x = clamp(p.x, lp.x - 50.0, lp.x + 50.0);
    lp.z = clamp(p.z, lp.z - 50.0, lp.z + 50.0);
    if (id == OBJ_CEILING)
    {
        lp.y = 0.0;
    }

    float3 L = normalize(lp - p);
    float dist = length(lp - p);
    float atten = 0.5 / dist + 0.5 / (dist * dist);
    float NdotL = max(0.0, dot(n, L));

    // Shadow
    float shadow = 1.0;
    if (id != OBJ_CEILING)
    {
        shadow = calcSoftShadow(p + n * 1.0, L, dist);
        shadow = shadow * 0.75 + 0.25;
    }

    // Direct lighting
    float3 direct = albedo * LIGHT_COLOR * NdotL * atten * shadow;

    // Indirect lighting (color bleeding)
    float3 indirect = float3(0.0, 0.0, 0.0);
    indirect += albedo * RIGHT_WALL_COLOR * LIGHT_COLOR * max(0.0, -n.x) * 0.06 / (p.x + 1.0);
    indirect += albedo * LEFT_WALL_COLOR * LIGHT_COLOR * max(0.0, n.x) * 0.06 / (556.0 - p.x + 1.0);

    // Ambient occlusion
    float ao = calcAO(p, n, lightPos);

    return (direct + indirect + albedo * 0.001) * ao;
}

// =============================================================================
// Glass Sphere
// =============================================================================

float fresnel(float cosTheta)
{
    float r0 = (1.0 - SPHERE_IOR) / (1.0 + SPHERE_IOR);
    r0 *= r0;
    return r0 + (1.0 - r0) * pow(1.0 - cosTheta, 5.0);
}

bool intersectSphere(float3 ro, float3 rd, out float tNear, out float tFar)
{
    float3 oc = ro - SPHERE_POS;
    float b = dot(oc, rd);
    float c = dot(oc, oc) - SPHERE_RAD * SPHERE_RAD;
    float h = b * b - c;
    if (h < 0.0)
    {
        return false;
    }
    h = sqrt(h);
    tNear = -b - h;
    tFar = -b + h;
    return true;
}

float3 renderGlassSphere(float3 p, float3 rd, float3 lightPos)
{
    float3 col = float3(0.0, 0.0, 0.0);
    float3 n = normalize(p - SPHERE_POS);
    float cosI = max(0.0, -dot(rd, n));
    float fr = fresnel(cosI);

    // Reflection
    float3 reflDir = reflect(rd, n);
    float2 reflHit = raymarch(p + n * 0.1, reflDir, false, lightPos);
    if (reflHit.x > 0.0)
    {
        float3 rp = p + n * 0.1 + reflDir * reflHit.y;
        if (rp.z >= 0.0)
        {
            float3 rn = calcNormal(rp, false, lightPos);
            col += fr * shade(rp, rn, reflHit.x, lightPos);
        }
    }

    // Refraction
    float3 refrDir = refract(rd, n, 1.0 / SPHERE_IOR);
    if (dot(refrDir, refrDir) > 0.01)
    {
        float tNear, tFar;
        if (intersectSphere(p + refrDir * 0.1, refrDir, tNear, tFar))
        {
            float3 exitP = p + refrDir * (0.1 + tFar);
            float3 exitN = -normalize(exitP - SPHERE_POS);
            float3 exitDir = refract(refrDir, exitN, SPHERE_IOR);

            if (dot(exitDir, exitDir) > 0.01)
            {
                float2 exitHit = raymarch(exitP + exitN * 0.1, exitDir, false, lightPos);
                if (exitHit.x > 0.0)
                {
                    float3 ep = exitP + exitN * 0.1 + exitDir * exitHit.y;
                    if (ep.z >= 0.0)
                    {
                        float3 en = calcNormal(ep, false, lightPos);
                        col += (1.0 - fr) * shade(ep, en, exitHit.x, lightPos) * float3(0.97, 0.98, 1.0);
                    }
                }
            }
        }
    }

    return col;
}

// =============================================================================
// Main Render
// =============================================================================

float3 render(float3 ro, float3 rd, float3 lightPos)
{
    float2 hit = raymarch(ro, rd, true, lightPos);
    if (hit.x < 0.0)
    {
        return float3(0.0, 0.0, 0.0);
    }

    float3 p = ro + rd * hit.y;
    if (p.z < 0.0)
    {
        return float3(0.0, 0.0, 0.0);
    }

    if (hit.x == OBJ_SPHERE)
    {
        return renderGlassSphere(p, rd, lightPos);
    }

    float3 n = calcNormal(p, true, lightPos);
    return shade(p, n, hit.x, lightPos);
}

float3 setupCamera(float3 ro, float3 rd)
{
    rd.x = -rd.x;
    float3 dir = normalize(CAM_TARGET - ro);
    float angY = atan2(dir.z, dir.x);
    rd = rotateY(rd, PI * 0.5 - angY);
    float angX = atan2(dir.y, dir.z);
    rd = rotateX(rd, -angX);
    return rd;
}

// =============================================================================
// Entry Point
// =============================================================================

float4 PSMain(Vertex input) : SV_TARGET
{
    float2 resolution = constants.Resolution;
    float2 fragCoord = input.UV * resolution;
    float time = constants.TotalTime;

    float3 lightPos = getLightPos(time);
    float3 color = float3(0.0, 0.0, 0.0);

    // 4x rotated grid anti-aliasing
    float angleStep = PI * 0.5;
    float angle = angleStep * 0.333;

    for (int i = 0; i < 4; i++)
    {
        float2 offset = float2(cos(angle), sin(angle)) * 0.4;
        float2 uv = (fragCoord + offset - resolution * 0.5) / resolution.y;
        float3 rd = setupCamera(CAM_POS, normalize(float3(uv, 0.0) - float3(0.0, 0.0, -1.4)));
        color += render(CAM_POS, rd, lightPos);
        angle += angleStep;
    }
    color *= 0.25;

    // Tone mapping
    color *= EXPOSURE;
    color = pow(saturate(color), 1.0 / GAMMA);

    return float4(color, 1.0);
}
