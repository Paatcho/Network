// Effet de glow qui oscille en intensité
void ice_glow_float(float3 world_normal, float3 world_view_dir, float time, float glow_intensity, float pulse_speed,
                   float pulse_amount, float3 glow_color, out float3 glow_out)
{
    float3 n = normalize(world_normal);
    float3 v = normalize(world_view_dir);

    // Utiliser fresnel pour afficher le glow uniquement sur les côtés qui ne font pas face à la caméra, 
    // pour l'afficher au bord des modèles.
    float n_dot_v = saturate(dot(n, v));
    float fresnel = pow(1.0 - n_dot_v, 2.0);

    // Effet de scintillement qui évolue avec le temps.
    float pulse = 1.0 + sin(time * pulse_speed) * pulse_amount;

    glow_out = glow_color * fresnel * glow_intensity * pulse;
}
