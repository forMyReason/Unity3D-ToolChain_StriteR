﻿// Upgrade NOTE: replaced '_Object2World' with 'unity_ObjectToWorld'

Shader "Game/Effect/Depth/VolumetricVerticalFog"
{
    Properties
    {
        _Color("Color",Color) = (1,1,1,1)
        _Density("Density",Range(0,5)) = 1
    }
    SubShader
    {
        Tags{"Queue"="Transparent"}
        Pass
        {
            Tags { "IgnoreProjector" = "True" "PreviewType" = "Box"}
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off
            Cull Back
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "../../CommonInclude.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 pos:TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float3 viewDir:TEXCOORD2;
            };

            float4 _Color;
            float _Density;
            sampler2D _CameraDepthTexture;
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.pos = v.vertex;
                o.viewDir = ObjSpaceViewDir(v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                return o;
            }


            fixed4 frag(v2f i) : SV_Target
            {
                half3 viewDir = -normalize(i.viewDir);
                half viewDst = AABBRayDistance(-.5,.5,i.pos, viewDir).y;
                half worldDepthDst = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, i.screenPos)).r - i.screenPos.w;
                half depthDst = length(mul(unity_WorldToObject, float3(0, worldDepthDst, 0)));
                half maxDst = min(depthDst, viewDst);

                half3 maxViewPos = i.pos + viewDir * maxDst;
                half yParam = min(i.pos.y, maxViewPos.y);
                
                half dstParam = saturate( smoothstep(.5,-.5, yParam)*_Density);
                
                return _Color*  dstParam;
            }
            ENDCG
        }
    }
}