#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Texture.hlsl"

#ifndef SHADERFUNCTIONSTEST_INCLUDE
#define SHADERFUNCTIONSTEST_INCLUDE

void base_multiply_float(float3 input_value, out float3 output_value)
{
    output_value = input_value * 2.0;
}

void spiral_blur_float(float distance, float distance_steps, UnityTexture2D tex2d, float radial_steps, float kernel_power, float2 uv, float radial_offset, out float3 spiral_color)
{
    spiral_color = 0;
    float2 new_uv = uv;
    float step_size = distance / (int) distance_steps;
    float2 spiral_offset = 0;

    if (distance_steps < 1)
    {
        spiral_color = SAMPLE_TEXTURE2D_LOD(tex2d.tex, tex2d.samplerstate, uv, 0); 
    }
    else
    {
        float accumulated_distance = 0;
        float sub_offset = 0;
        float spiral_distance = 0;
        int i = 0;
        while (i < distance_steps)
        {
            spiral_distance += step_size;
            
            for (int j = 0; j < radial_steps; j++)
            {
                float two_pi = 6.283185;
                sub_offset++;
                spiral_offset.x = cos(two_pi * (sub_offset / radial_steps));
                spiral_offset.y = sin(two_pi * (sub_offset / radial_steps));
                new_uv.x = uv.x + spiral_offset.x * spiral_distance;
                new_uv.y = uv.y + spiral_offset.y * spiral_distance;
                float dist_pow = pow(spiral_distance, kernel_power);
                spiral_color += SAMPLE_TEXTURE2D_LOD(tex2d.tex, tex2d.samplerstate, new_uv, 0).xyz * dist_pow;
                accumulated_distance += dist_pow;
            }
            
            sub_offset += radial_offset;
            i++;
        }
        
        spiral_color /= accumulated_distance;
    }
}

#endif
