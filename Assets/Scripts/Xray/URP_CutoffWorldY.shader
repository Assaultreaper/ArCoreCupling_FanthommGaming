Shader "Custom/XRayCutoff_Advanced"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _NormalMap ("Normal Map", 2D) = "bump" {}
        _MetallicMap ("Metallic Map", 2D) = "black" {}
        _HeightMap ("Height Map", 2D) = "black" {}
        _Color ("Color Tint", Color) = (1,1,1,1)
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
        _Metallic ("Metallic", Range(0,1)) = 0.5
        _Parallax ("Parallax Height", Range(0.005, 0.08)) = 0.02
        _CutoffZ ("Cutoff Z", Float) = -999
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 300

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _MainTex;
        sampler2D _NormalMap;
        sampler2D _MetallicMap;
        sampler2D _HeightMap;

        fixed4 _Color;
        float _Glossiness;
        float _Metallic;
        float _Parallax;
        float _CutoffZ;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_NormalMap;
            float2 uv_MetallicMap;
            float2 uv_HeightMap;
            float3 worldPos;
            float3 viewDir;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Cutoff logic (X-Ray)
            if (IN.worldPos.z < _CutoffZ)
                discard;

            // Parallax Mapping (from Height Map)
            float height = tex2D(_HeightMap, IN.uv_HeightMap).r;
            float2 offset = IN.viewDir.xy * (height * _Parallax);
            float2 uvParallax = IN.uv_MainTex + offset;

            // Albedo
            fixed4 c = tex2D(_MainTex, uvParallax) * _Color;
            o.Albedo = c.rgb;

            // Normal Map
            fixed3 normalTex = UnpackNormal(tex2D(_NormalMap, uvParallax));
            o.Normal = normalTex;

            // Metallic and Smoothness from map (modulated by slider)
            fixed metallicSample = tex2D(_MetallicMap, uvParallax).r;
            o.Metallic = metallicSample * _Metallic;
            o.Smoothness = _Glossiness;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
