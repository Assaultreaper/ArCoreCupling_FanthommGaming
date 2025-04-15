Shader "Custom/CrossSectionGlassURP"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _MetallicGlossMap ("Metallic", 2D) = "black" {}
        _BumpMap ("Normal Map", 2D) = "bump" {}
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Transparency ("Transparency", Range(0,1)) = 0.5
        _CuttingPosition ("Cutting Position", Vector) = (0, 0, 0, 0)
        _CuttingRadius ("Cutting Radius", Float) = 1.0
        _EnableXRay ("Enable X-ray", Range(0,1)) = 0.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            Name "ForwardBase"
            Tags { "LightMode" = "UniversalForward" }

            HLSLPROGRAM
            #pragma multi_compile_fog
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            // Declare the global properties
            float3 _CuttingPosition;
            float _CuttingRadius;
            float _EnableXRay;

            // Texture samplers
            sampler2D _MainTex;
            sampler2D _MetallicGlossMap;
            sampler2D _BumpMap;
            float _Glossiness;
            float _Metallic;

            // Struct for vertex input
            struct Attributes
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float2 uv : TEXCOORD0;
            };

            // Struct for fragment input
            struct Varyings
            {
                float3 worldPos : TEXCOORD0;
                float2 uv : TEXCOORD1;
                float3 normal : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            // Vertex function
            Varyings vert(Attributes v)
            {
                Varyings o;
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                o.normal = mul((float3x3)unity_ObjectToWorld, v.normal);
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            // Fragment function
            half4 frag(Varyings i) : SV_Target
            {
                // Check distance from cutting position and discard outside radius
                float dist = distance(i.worldPos, _CuttingPosition);
                if (dist > _CuttingRadius)
                    discard;

                // Apply transparency and textures if X-ray is enabled
                half4 albedo = tex2D(_MainTex, i.uv);
                half4 metallicGloss = tex2D(_MetallicGlossMap, i.uv);
                half3 normalTS = UnpackNormal(tex2D(_BumpMap, i.uv));

                half4 outputColor;

                if (_EnableXRay > 0.5)
                {
                    // Apply transparency when X-ray is enabled
                    outputColor = albedo;
                    outputColor.a = 0.3; // Transparency level
                }
                else
                {
                    // No transparency when X-ray is disabled
                    outputColor = albedo;
                    outputColor.a = 1.0;
                }

                return outputColor;
            }
            ENDHLSL
        }
    }

    Fallback "UniversalForward"
}
