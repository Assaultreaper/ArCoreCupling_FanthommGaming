Shader "Custom/URP_CrossSectionPBR_XYZCut"
{
    Properties
    {
        _MyBaseMap ("Base Map", 2D) = "white" {}
        _MyNormalMap ("Normal Map", 2D) = "bump" {}
        _MyHeightMap ("Height Map", 2D) = "black" {}
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Smoothness ("Smoothness", Range(0,1)) = 0.5
        _CutoffY ("World Y Cutoff", Float) = 0.0
        _CutoffX ("World X Cutoff", Float) = -999.0
        _CutoffZ ("World Z Cutoff", Float) = -999.0
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            Name "ForwardLit"
            Tags { "LightMode"="UniversalForward" }

            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 3.0
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/SurfaceInput.hlsl"

            struct Attributes
            {
                float4 positionOS : POSITION;
                float3 normalOS : NORMAL;
                float2 uv : TEXCOORD0;
                float4 tangentOS : TANGENT;
            };

            struct Varyings
            {
                float4 positionHCS : SV_POSITION;
                float3 positionWS : TEXCOORD0;
                float3 normalWS : TEXCOORD1;
                float2 uv : TEXCOORD2;
                float3 viewDirWS : TEXCOORD3;
                float4 tangentWS : TEXCOORD4;
            };

            sampler2D _MyBaseMap;
            sampler2D _MyNormalMap;
            sampler2D _MyHeightMap;
            float4 _MyBaseMap_ST;
            float _Metallic;
            float _Smoothness;
            float _CutoffY;
            float _CutoffX;
            float _CutoffZ;

            Varyings vert(Attributes IN)
            {
                Varyings OUT;

                float3 positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionWS = positionWS;
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _MyBaseMap);
                OUT.positionHCS = TransformWorldToHClip(positionWS);
                OUT.viewDirWS = _WorldSpaceCameraPos - positionWS;
                OUT.tangentWS = float4(TransformObjectToWorldDir(IN.tangentOS.xyz), IN.tangentOS.w);

                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                float3 pos = IN.positionWS;

                // Cutoff checks for X, Y, Z
                if (pos.y < _CutoffY || pos.x < _CutoffX || pos.z < _CutoffZ)
                    discard;

                float3 albedo = tex2D(_MyBaseMap, IN.uv).rgb;

                float3 normalTS = UnpackNormal(tex2D(_MyNormalMap, IN.uv));
                float3 bitangent = cross(IN.normalWS, IN.tangentWS.xyz) * IN.tangentWS.w;
                float3x3 TBN = float3x3(IN.tangentWS.xyz, bitangent, IN.normalWS);
                float3 normalWS = normalize(mul(normalTS, TBN));

                InputData inputData = (InputData)0;
                inputData.positionWS = IN.positionWS;
                inputData.normalWS = normalWS;
                inputData.viewDirectionWS = normalize(IN.viewDirWS);

                SurfaceData surfaceData = (SurfaceData)0;
                surfaceData.albedo = albedo;
                surfaceData.metallic = _Metallic;
                surfaceData.smoothness = _Smoothness;
                surfaceData.normalTS = normalTS;
                surfaceData.occlusion = 1.0;
                surfaceData.alpha = 1.0;

                return UniversalFragmentPBR(inputData, surfaceData);
            }

            ENDHLSL
        }
    }

    FallBack "Hidden/InternalErrorShader"
}
