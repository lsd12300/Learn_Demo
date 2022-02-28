Shader "Fow/scene_customLight"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1,1,1,1)
		_Ambient ("Ambient", Color) = (0.36, 0.36, 0.36, 1)

		_FogNoise("雾流动噪声", 2D) = "white" {}
		_Speed("速度", Float) = 1.0
    }
    SubShader
    {
		Tags{ 
			"RenderType" = "Transparent"
			"Queue" = "Transparent"
			"IgnoreProjector" = "True"
		}
		ZWrite Off
		Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
			Tags{"LightMode" = "ForwardBase"}

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
			#include "UnityLightingCommon.cginc"
			#include "Lighting.cginc"

			//#pragma multi_compile_fwdbase nolightmap nodirlightmap nodynlightmap novertexlight
			#include "AutoLight.cginc"
			#include "WarFog.cginc"
			#pragma multi_compile WAR_FOG_ON _


            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
				half3 normal : NORMAL;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
				half3 normalW : TEXCOORD1;
				WAR_FOG_COORD(2)
                //SHADOW_COORD(3)
				float2 uv_noise : TEXCOORD3;
            };

			half4 _Color;
            sampler2D _MainTex;
            half4 _MainTex_ST;
			sampler2D _FogNoise;
			half4 _FogNoise_ST;
			half4 _Ambient;
			float _Speed;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.normalW = UnityObjectToWorldNormal(v.normal);
				//TRANSFER_SHADOW(o);
				TRANSFER_WAR_FOG(o, v.vertex);

				o.uv_noise.xy = TRANSFORM_TEX(v.uv, _FogNoise) + frac(float2(_Speed, 0.0) * _Time.x);
				//o.uv_noise.xy = TRANSFORM_TEX(v.uv, _FogNoise);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
				//fixed4 cosL = max(0, dot(_WorldSpaceLightPos0.xyz, normalize(i.normalW))) * _LightColor0;
				//fixed4 diffuseAlbedo = tex2D(_MainTex, i.uv) * _Color;
				//fixed4 shadow = SHADOW_ATTENUATION(i);
				//fixed4 col = diffuseAlbedo * cosL * shadow + diffuseAlbedo * _Ambient;
				//fixed4 col = diffuseAlbedo * cosL + diffuseAlbedo * _Ambient;
				//col.a = diffuseAlbedo.a;

				//fixed4 col = tex2D(_MainTex, i.uv) * _Color;
				fixed4 col = _Color;
				APPLY_WAR_FOG(i, col);

				fixed4 noise_col = tex2D(_FogNoise, i.uv_noise.xy);
				//col *= noise_col;
				col.a *= noise_col.r;

                return col;
            }
            ENDCG
        }

		//Pass {
		//	Tags{"LightMode" = "ShadowCaster"}

		//	CGPROGRAM
		//	#pragma vertex vert
		//	#pragma fragment frag

		//	#include "UnityCG.cginc"
		//	#pragma multi_compile_shadowcaster

		//	struct v2f {
		//		V2F_SHADOW_CASTER;
		//	};

		//	v2f vert(appdata_base v) {
		//		v2f o;
		//		TRANSFERR_SHADOW_CASTER_NORMALOFFSET(o)
		//		return o;
		//	}

		//	fixed4 frag(v2f i) : SV_Target{
		//		SHADOW_CASTER_FRAGMENT(i)
		//	}

		//	ENDCG
		//}
    }
}
