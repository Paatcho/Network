// Bonus : effet d'outline
// (ne fonctionne pas avec les cristaux, fonctionne sur une sphère simple)
void outline_float(float3 position, float3 normal, float outline_width, out float3 new_position)
{
    new_position = position + normalize(normal) * outline_width;
}