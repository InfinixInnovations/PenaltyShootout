Shader "Custom/FootballPitch"
{
    Properties
    {
        [Header(Grass Texture)]
        _GrassTex ("Grass Albedo Texture", 2D) = "white" {}
        _NormalMap ("Grass Normal Map (optional)", 2D) = "bump" {}
        _NormalStrength ("Normal Map Strength", Range(0,2)) = 0.5

        [Header(Mowing Stripes)]
        _StripeCount ("Number of Stripes", Float) = 10
        _DarkColor ("Dark Stripe Color", Color) = (0.15, 0.38, 0.12, 1)
        _LightColor ("Light Stripe Color", Color) = (0.22, 0.52, 0.18, 1)
        _StripeBlend ("Stripe Edge Softness", Range(0.001, 0.1)) = 0.02
        _StripeContrast ("Stripe Contrast", Range(0, 1)) = 0.3

        [Header(Surface)]
        _Smoothness ("Smoothness", Range(0, 1)) = 0.05
        _SpecularColor ("Specular Color", Color) = (0.05, 0.05, 0.05, 1)
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS   : NORMAL;
                float4 tangentOS  : TANGENT;
                float2 uv         : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float2 uv          : TEXCOORD0;
                float3 positionWS  : TEXCOORD1;
                float3 normalWS    : TEXCOORD2;
                float3 tangentWS   : TEXCOORD3;
                float3 bitangentWS : TEXCOORD4;
                float4 shadowCoord : TEXCOORD5;
            };

            TEXTURE2D(_GrassTex);       SAMPLER(sampler_GrassTex);
            TEXTURE2D(_NormalMap);      SAMPLER(sampler_NormalMap);

            CBUFFER_START(UnityPerMaterial)
                float4  _GrassTex_ST;
                half4   _DarkColor;
                half4   _LightColor;
                float   _StripeCount;
                float   _StripeBlend;
                float   _StripeContrast;
                float   _NormalStrength;
                half    _Smoothness;
                half4   _SpecularColor;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                VertexPositionInputs posInputs = GetVertexPositionInputs(IN.positionOS.xyz);
                VertexNormalInputs normalInputs = GetVertexNormalInputs(IN.normalOS, IN.tangentOS);

                OUT.positionHCS = posInputs.positionCS;
                OUT.positionWS  = posInputs.positionWS;
                OUT.normalWS    = normalInputs.normalWS;
                OUT.tangentWS   = normalInputs.tangentWS;
                OUT.bitangentWS = normalInputs.bitangentWS;
                OUT.uv          = TRANSFORM_TEX(IN.uv, _GrassTex);
                OUT.shadowCoord = GetShadowCoord(posInputs);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // ─── 1. Grass Texture ─────────────────────────────────────────
                // Tile the grass texture densely so individual blades are visible
                float2 grassUV = IN.uv * float2(60.0, 60.0);
                half4 grassSample = SAMPLE_TEXTURE2D(_GrassTex, sampler_GrassTex, grassUV);

                // ─── 2. Mowing Stripes ────────────────────────────────────────
                // UV.y runs 0..1 across the pitch (sideline to sideline).
                // Multiply by stripe count to get number of stripes.
                float stripeUV = IN.uv.y * _StripeCount;
                // Triangle wave between 0 and 1 — creates the alternating pattern
                float wave = abs(frac(stripeUV) * 2.0 - 1.0);
                // Smooth edges
                float stripe = smoothstep(0.5 - _StripeBlend, 0.5 + _StripeBlend, wave);
                // Blend between two turf colors
                half3 pitchColor = lerp(_DarkColor.rgb, _LightColor.rgb, stripe);

                // ─── 3. Combine grass texture detail with stripe color ─────────
                // Use grass texture as a detail overlay — it modulates the stripe color
                // Remap texture so 0.5 = neutral, keeping the stripe color as base
                half grassDetail = (grassSample.r - 0.5) * _StripeContrast;
                half3 finalColor = pitchColor + grassDetail;

                // ─── 4. Normal map ────────────────────────────────────────────
                half4 normalSample = SAMPLE_TEXTURE2D(_NormalMap, sampler_NormalMap, grassUV);
                half3 normalTS = UnpackNormalScale(normalSample, _NormalStrength);
                half3 normalWS = TransformTangentToWorld(normalTS,
                    half3x3(IN.tangentWS, IN.bitangentWS, IN.normalWS));
                normalWS = normalize(normalWS);

                // ─── 5. Lighting ──────────────────────────────────────────────
                Light mainLight = GetMainLight(IN.shadowCoord);
                half NdotL = saturate(dot(normalWS, mainLight.direction));
                half3 diffuse = finalColor * mainLight.color * (NdotL * 0.8 + 0.2); // 0.2 min ambient

                // Very subtle specular (grass is not shiny)
                half3 viewDir = normalize(GetCameraPositionWS() - IN.positionWS);
                half3 halfDir = normalize(mainLight.direction + viewDir);
                half NdotH = saturate(dot(normalWS, halfDir));
                half spec = pow(NdotH, 32.0) * _Smoothness;
                half3 specular = _SpecularColor.rgb * spec;

                half3 result = diffuse + specular;
                return half4(result, 1.0);
            }
            ENDHLSL
        }

        // Shadow caster pass
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ColorMask 0

            HLSLPROGRAM
            #pragma vertex ShadowPassVertex
            #pragma fragment ShadowPassFragment
            #include "Packages/com.unity.render-pipelines.universal/Shaders/ShadowCasterPass.hlsl"
            ENDHLSL
        }
    }
}
