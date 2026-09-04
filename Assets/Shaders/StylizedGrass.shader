Shader "Custom/StylizedGrass"
{
    Properties
    {
        _BaseColor ("Base Color (Bottom)", Color) = (0.1, 0.35, 0.1, 1)
        _TipColor ("Tip Color (Top)", Color) = (0.2, 0.6, 0.2, 1)
        _Smoothness ("Smoothness", Range(0, 1)) = 0.1
        
        [Header(Wind Settings)]
        _WindSpeed ("Wind Speed", Float) = 2.0
        _WindStrength ("Wind Strength", Float) = 0.2
        _WindFrequency ("Wind Frequency", Float) = 1.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" }
        LOD 100

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes
            {
                float4 positionOS   : POSITION;
                float3 normalOS     : NORMAL;
                float2 uv           : TEXCOORD0;
            };

            struct Varyings
            {
                float4 positionHCS  : SV_POSITION;
                float3 positionWS   : TEXCOORD0;
                float3 normalWS     : TEXCOORD1;
                float heightFraction : TEXCOORD3;
            };

            CBUFFER_START(UnityPerMaterial)
                half4 _BaseColor;
                half4 _TipColor;
                half _Smoothness;
                float _WindSpeed;
                float _WindStrength;
                float _WindFrequency;
            CBUFFER_END

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                
                // Keep track of original local height (Y coordinate) for gradient coloring
                // Let's assume the grass blade is drawn from Y=0 to Y=1 in local space
                OUT.heightFraction = saturate(IN.positionOS.y);
                
                // Wind Animation: Only displace vertices above the ground (Y > 0)
                float3 worldPos = TransformObjectToWorld(IN.positionOS.xyz);
                float windWave = sin(_Time.y * _WindSpeed + worldPos.x * _WindFrequency + worldPos.z * _WindFrequency);
                
                // Displace local position based on wind wave, scaled by height fraction
                float3 displacedPos = IN.positionOS.xyz;
                displacedPos.x += windWave * _WindStrength * OUT.heightFraction;
                displacedPos.z += windWave * _WindStrength * 0.5 * OUT.heightFraction;

                OUT.positionWS = TransformObjectToWorld(displacedPos);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                // Simple lighting
                float3 normal = normalize(IN.normalWS);
                Light mainLight = GetMainLight();
                
                half NdotL = saturate(dot(normal, mainLight.direction));
                
                // Color gradient based on vertex height
                half4 grassColor = lerp(_BaseColor, _TipColor, IN.heightFraction);
                
                // Diffuse lighting + ambient
                half3 diffuse = grassColor.rgb * mainLight.color * NdotL;
                half3 ambient = grassColor.rgb * half3(0.25, 0.25, 0.25); // Ambient light
                
                return half4(diffuse + ambient, 1.0);
            }
            ENDHLSL
        }
    }
}
