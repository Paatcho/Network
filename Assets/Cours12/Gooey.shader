Shader "Custom/Gooey"
{
    Properties
    {
        _Power("Fresnel Power", Float) = 2.0
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
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionHCS = TransformObjectToHClip(IN.positionOS.xyz);
                OUT.worldPos = mul(unity_ObjectToWorld, IN.positionOS);
                OUT.viewDir = GetWorldSpaceNormalizeViewDir(OUT.worldPos);
                OUT.norm2f = TransformObjectToWorldNormal(IN.norm);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float mask = saturate(dot(IN.norm2f, IN.viewDir));
                mask += pow(1.0 - mask, _Power);
                return color;
            }
            ENDHLSL
        }
    }
}
