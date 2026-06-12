Shader "Custom/Gooey"
{
    Properties
    {
        _Power("Fresnel Power", Float) = 2.0
        _Melting("Melting", Range(0.0, 1.0)) = 1.0
        _MeltPuddleScale("Melt Puddle Scale", Float) = 1.0
        _MeltPuddlePower("Melt Puddle Power", Float) = 2.0
        _TinyBloat("Tiny Bloat", Float) = 0.0
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline" }

        Pass
        {
            HLSLPROGRAM

            #pragma vertex vert
            #pragma fragment frag

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/SpaceTransforms.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float2 uv : TEXCOORD0;
                float3 norm : NORMAL;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 worldPos : TEXCOORD1;
                float3 viewDir : TEXCOORD2;
                float norm2f : NORMAL;
            };

            CBUFFER_START(UnityPerMaterial)
                float _Power;
                float _Melting;
                float _MeltPuddleScale;
                float _MeltPuddlePower;
                float _TinyBloat;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                // partie vertex Melt
                float3 finalPosition = IN.positionOS;
                finalPosition.y += min(0, lerp(_Melting - 0.5 - IN.positionOS.y, IN.positionOS.y, _TinyBloat * IN.positionOS.y));
                finalPosition.xy *= 1 + pow((1 - _Melting) * _MeltPuddleScale, _MeltPuddlePower);
                OUT.positionHCS = TransformObjectToHClip(finalPosition.xyz);
                
                // mapping pour le fragment shader (fresnel)
                OUT.worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.viewDir = GetWorldSpaceNormalizeViewDir(OUT.worldPos);
                OUT.norm2f = TransformObjectToWorldNormal(IN.norm);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float mask = saturate(dot(IN.norm2f, IN.viewDir));
                float color = pow(1.0 - mask, _Power);
                return color;
            }
            ENDHLSL
        }
    }
}
