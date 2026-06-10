void Fresnel_float(float3 worldNml, float3 viewDir, float power, float bias, float scale, out float fresnelMask)
{
    float3 nml = normalize(worldNml);
    float3 vd = normalize(viewDir);
    
    float nmlDotVd = dot(nml, vd);
    
    fresnelMask = bias + scale * pow(1 - nmlDotVd, power);
}