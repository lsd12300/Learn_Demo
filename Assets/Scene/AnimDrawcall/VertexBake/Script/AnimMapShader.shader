Shader "Lsd/VertexBakeAnim"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		//_AnimMap ("AnimMap", 2D) ="white" {}

		_VertexCount("顶点数量", int) = 21
		_FrameCount("帧数", int) = 30
		_TexWidth("宽度", int) = 1
		_TexHeight("高度", int) = 1
		_PreTime("上次播放时间", Float) = 0
		_Interval("每帧时间(s)", Float) = 0.0333
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100
			Cull off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			//开启gpu instancing
			#pragma multi_compile_instancing


			#include "UnityCG.cginc"

			struct appdata
			{
				float2 uv : TEXCOORD0;
				float4 pos : POSITION;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				float4 vertex : SV_POSITION;
			};

			// 定义实例化 不同的属性
			UNITY_INSTANCING_BUFFER_START(Props)
				UNITY_DEFINE_INSTANCED_PROP(float, _PreTime)
			UNITY_INSTANCING_BUFFER_END(Props)

			sampler2D _MainTex;
			float4 _MainTex_ST;
			uniform sampler2D _AnimMap;
			float _Interval;
			int _VertexCount;
			int _FrameCount;
			int _TexWidth;
			int _TexHeight;


			float2 ToUV(int index) {
				float2 uv = 0;
				uv.y = floor(index / _TexWidth);
				uv.x = index - uv.y * _TexWidth;

				uv.x = uv.x / _TexWidth;
				uv.y = uv.y / _TexHeight;
				return uv;
			}


			// 语义 uint vid : SV_VertexID  获取 顶点ID
			v2f vert(appdata v, uint vid : SV_VertexID)
			{
				v2f o;

				UNITY_SETUP_INSTANCE_ID(v);

				int frame = floor((_Time.y - UNITY_ACCESS_INSTANCED_PROP(Props, _PreTime)) / _Interval);
				frame = clamp(0, _FrameCount - 1, frame);

				float4 pos = tex2Dlod(_AnimMap, float4(ToUV(frame * _VertexCount + vid), 0, 0));

				o.vertex = UnityObjectToClipPos(pos);
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
	}
}
