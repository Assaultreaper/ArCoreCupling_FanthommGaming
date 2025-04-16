Shader "Custom/ARCore_ZOffset_MetallicRoughness_Normal"
{
    Properties
    {
        _MainTex ("Base Color", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _RoughnessMap ("Roughness (Inverted Smoothness)", 2D) = "black" {}
        _MetallicMap ("Metallic Map", 2D) = "black" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _CutoffZ ("Z Cutoff (X-ray)", Float) = -999.0
    }

    SubShader
    {
        Tags { "Queue"="Geometry" "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _RoughnessMap;
        sampler2D _MetallicMap;
        fixed4 _Color;
        float _CutoffZ;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float2 uv_NormalMap;
            float2 uv_RoughnessMap;
            float2 uv_MetallicMap;
        };

        void surf(Input IN, inout SurfaceOutputStandard o)
        {
            // X-Ray Z-Cutoff (in world space)
            if (IN.worldPos.z < _CutoffZ)
                discard;

            // Base color and tint
            fixed4 albedoTex = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = albedoTex.rgb;

            // Metallic from texture map
            fixed metallicValue = tex2D(_MetallicMap, IN.uv_MetallicMap).r;
            o.Metallic = saturate(metallicValue);

            // Invert roughness to get smoothness (Unity expects smoothness)
            fixed roughnessValue = tex2D(_RoughnessMap, IN.uv_RoughnessMap).r;
            o.Smoothness = 1.0 - saturate(roughnessValue);

            // Normal map
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));
        }
        ENDCG
    }

    FallBack "Standard"
}
