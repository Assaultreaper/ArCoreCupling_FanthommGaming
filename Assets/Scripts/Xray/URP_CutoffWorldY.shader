Shader "Custom/XRayCutoff_Mobile"
{
    Properties
    {
        _MainTex ("Albedo", 2D) = "white" {}
        _CutoffZ ("Cutoff Z", Float) = -999
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard alpha:fade addshadow

        sampler2D _MainTex;
        float _CutoffZ;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            INTERNAL_DATA
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float3 viewDir = WorldSpaceViewDir(float4(IN.worldPos, 1.0));

            if (IN.worldPos.z < _CutoffZ)
            {
                o.Albedo = 0;
                o.Alpha = 0;
                return;
            }

            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
    }

    FallBack "Diffuse"
}
