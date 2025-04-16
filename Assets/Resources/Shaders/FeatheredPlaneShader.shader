Shader "Custom/ARFoundation/FeatheredPlane_BuiltIn"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _TexTintColor("Texture Tint Color", Color) = (1,1,1,1)
        _PlaneColor("Plane Color", Color) = (1,1,1,1)
        _ShortestUVMapping("Shortest UV Mapping", Float) = 0.0
    }

    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        LOD 200

        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        Cull Off
        Lighting Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0

            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _TexTintColor;
            float4 _PlaneColor;
            float _ShortestUVMapping;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1; // x = feathering fade
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
                float3 uv2 : TEXCOORD1;
            };

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv2 = v.uv2;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 texColor = tex2D(_MainTex, i.uv);
                fixed4 tinted = texColor * _TexTintColor;
                fixed4 result = lerp(_PlaneColor, tinted, texColor.a);

                // Feathering based on uv2.x
                result.a *= 1.0 - smoothstep(1.0, _ShortestUVMapping, i.uv2.x);
                return result;
            }
            ENDCG
        }
    }

    FallBack "Unlit/Transparent"
}
