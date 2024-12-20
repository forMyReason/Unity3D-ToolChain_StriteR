Shader "Hidden/Imposter_AlbedoAlpha"
{
    Properties
    {
        [HDR]_Color                  ("Main Color", Color) = (1,1,1,1)
        _Cutoff("Alpha cutoff", Range(0.0, 1.0)) = 0.5
        _MainTex("_MainTex",2D) = "white"
    }
    SubShader
    {
		Tags{"LightMode" = "UniversalForward"}
        Pass
        {
            Blend Off
            Cull Back
            ZWrite On
            ZTest LEqual
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "Assets/Shaders/Library/Common.hlsl"
            #include "../Imposter.hlsl"

            struct a2v
            {
                float3 positionOS : POSITION;
                float2 uv : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 positionCS : SV_POSITION;
                float2 uv:TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);
            INSTANCING_BUFFER_START
                INSTANCING_PROP(float4,_MainTex_ST)
                INSTANCING_PROP(float,_Cutoff)
                INSTANCING_PROP(float4,_Color)
            INSTANCING_BUFFER_END
            
            v2f vert (a2v v)
            {
                v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_TRANSFER_INSTANCE_ID(v, o);
                o.positionCS = TransformObjectToHClip(v.positionOS);
                o.uv = TRANSFORM_TEX(v.uv,_MainTex);
                return o;
            }

            float4 frag (v2f i) : SV_Target
            {
				UNITY_SETUP_INSTANCE_ID(i);
            	clip(SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv).a - INSTANCE(_Cutoff));
                return SAMPLE_TEXTURE2D(_MainTex,sampler_MainTex,i.uv) * INSTANCE(_Color);
            }
            ENDHLSL
        }
    }
}
