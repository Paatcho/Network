// Bonus : Effet additionel de réfraction (qui marche mieux en tant que effet de roughness en fait)
void faceted_normal_float(float3 normal, float steps, out float3 faceted_out)
{
    float3 n = normalize(normal);
    faceted_out = normalize(round(n * steps) / steps);
}