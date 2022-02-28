/*
	平面阴影:
		使用两个Pass
			一个基本的模型渲染
			一个用于渲染阴影
				假设模型脚下有一块平面, 将阴影渲染到该平面上, 阴影会和其他模型穿插
*/
Shader "Lsd/PlanarShadow"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}


		[Header(Shadow)]
		_GroundHeight("地面高度", Float) = 0
		_ShadowColor ("阴影颜色", Color) = (0,0,0,1)
		_ShadowFalloff ("阴影衰减", Range(0,1)) = 0.05

    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                return col;
			}
				ENDCG
		}

		// 渲染平面阴影
		Pass{
			Name "Planar Shadow"

			// 模板测试,  保证 alpha 显示正确
			stencil{
				Ref 0
				Comp equal
				Pass incrWrap
				Fail keep
				ZFail keep
			}

			Cull Off
			Blend SrcAlpha OneMinusSrcAlpha		// 透明混合
			ZWrite Off
			Offset -1, 0		// 深度偏移, 防止与地面穿插

			CGPROGRAM
			#include "UnityCG.cginc"

			#pragma vertex vert
			#pragma fragment frag

			float4 _ShadowColor;
			float _ShadowFalloff;
			float4 _LightDir;
			float _GroundHeight;

			struct appdata{
				float4 vertex : POSITION;
			};

			struct v2f{
				float4 vertex : SV_POSITION;
				float4 color : COLOR;
			};

			float3 ShadowProjectPos(float4 vertPos){
				float3 shadowPos;

				// 顶点的 世界空间坐标
				float3 worldPos = mul(unity_ObjectToWorld, vertPos).xyz;

				// 灯光方向
				//float3 lightDir = normalize(_LightDir.xyz);
				float3 lightDir = normalize(_WorldSpaceLightPos0.xyz);

				// 阴影 的世界空间坐标
				shadowPos.y = min(worldPos.y, _GroundHeight);
				shadowPos.xz = worldPos.xz - lightDir.xz * max(0, worldPos.y - _GroundHeight) / lightDir.y;
				//shadowPos.y = min(worldPos.y, _LightDir.w);
				//shadowPos.xz = worldPos.xz - lightDir.xz * max(0, worldPos.y - _LightDir.w) / lightDir.y;
				
				return shadowPos;
			}


			v2f vert(appdata v){
				v2f o;

				// 阴影 世界空间坐标
				float3 shadowPos = ShadowProjectPos(v.vertex);

				// 转换到 裁剪空间
				o.vertex = UnityWorldToClipPos(shadowPos);

				// 中心点世界坐标
				//  矩阵 unity_ObjectToWorld 中 每一行的 w分量 分别为 Transform的 xyz值
				//float3 center = float3(unity_ObjectToWorld[0].w, _LightDir.w, unity_ObjectToWorld[2].w);
				float3 center = float3(unity_ObjectToWorld[0].w, _GroundHeight, unity_ObjectToWorld[2].w);

				// 阴影衰减
				float falloff = 1 - saturate(distance(shadowPos, center) * _ShadowFalloff);

				// 阴影颜色
				o.color = _ShadowColor;
				o.color.a *= falloff;

				return o;
			}

			fixed4 frag(v2f i) : SV_Target{
				return i.color;
			}

			ENDCG
		}
    }
}
