Shader "Custom/ZAxisCrossSectionPBR_AR"
{
    Properties
    {
        _BaseColor("Base Color", 2D) = "white" {}
        _Metallic("Metallic", 2D) = "black" {}
        _Roughness("Roughness", 2D) = "black" {}
        _NormalMap("Normal Map", 2D) = "bump" {}

        _Cutoffx ("X Cutoff", Float) = 0.0
        _FadeRange ("Fade Range", Float) = 0.1
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _BaseColor;
        sampler2D _Metallic;
        sampler2D _Roughness;
        sampler2D _NormalMap;

        float _Cutoffx;
        float _FadeRange;

        struct Input
        {
            float2 uv_BaseColor;
            float3 worldPos;
            float2 uv_Metallic;
            float2 uv_Roughness;
            float2 uv_NormalMap;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Convert world position to local object space
            float3 localPos = mul(unity_WorldToObject, float4(IN.worldPos, 1.0)).xyz;

            // Fade calculation for X-axis clip
            float xDiff = localPos.x - _Cutoffx;
            float fade = saturate(xDiff / _FadeRange);
            clip(fade - 0.01); // Cull if fully below threshold

            // Base Color (Albedo)
            fixed4 baseColor = tex2D(_BaseColor, IN.uv_BaseColor);
            o.Albedo = baseColor.rgb;

            // Metallic & Smoothness
            float metallic = tex2D(_Metallic, IN.uv_Metallic).r;
            float roughness = tex2D(_Roughness, IN.uv_Roughness).r;
            o.Metallic = metallic;
            o.Smoothness = 1.0 - roughness;

            // Normal map
            o.Normal = UnpackNormal(tex2D(_NormalMap, IN.uv_NormalMap));

            // Optional alpha (not required unless transparency is used)
            o.Alpha = baseColor.a;
        }
        ENDCG
    }

    FallBack "Standard"
}
