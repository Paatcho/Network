// Effet de sparkle/scintillememnt.
void sparkle_float(float2 uv, float3 world_normal, float3 world_view_dir, float time,
                   float scale, float speed, float sharpness, float3 sparkle_color,
                   out float3 sparkle_out, out float sparkle_mask
)
{
    float2 scaled_uv = uv * scale;
    float2 cell_id = floor(scaled_uv);
    float2 cell_uv = frac(scaled_uv) - 0.5;

    // Valeurs trouvées sur un tutoriel.
    float h1 = frac(sin(dot(cell_id, float2(127.1, 311.7))) * 43758.5453);
    float h2 = frac(sin(dot(cell_id, float2(269.5, 183.3))) * 35478.6521);
    float h3 = frac(sin(dot(cell_id, float2(419.2, 371.9))) * 53742.3571);

    float2 point_offset = float2(
        sin(time * speed + h3 * 6.28) * 0.2,
        cos(time * speed + h3 * 6.28) * 0.2
    );

    float dist = length(cell_uv - point_offset);
    float star = pow(saturate(1.0 - dist * 2.5), sharpness);
    float flicker = 0.5 + 0.5 * sin(time * speed * 2.0 + h1 * 6.28);
    
    flicker = pow(flicker, 3.0);
    star *= flicker;
    star *= step(0.5, h2);
    
    float n_dot_v = saturate(dot(normalize(world_normal), normalize(world_view_dir)));
    float fresnel = pow(1.0 - n_dot_v, 2.0);
    
    star *= 0.6 + 0.4 * (1.0 + fresnel);

    sparkle_mask = saturate(star);
    sparkle_out = sparkle_color * sparkle_mask;
}
