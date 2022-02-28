
/*
	GPUSkin
		预处理:
			1. 将每帧每个骨骼的矩阵变换存储到 贴图中
			2. 顶点受影响的骨骼权重 存储到 mesh的 uv2和uv3 通道中
		运行时:
			每个顶点 根据骨骼索引编号 从预处理的贴图中还原出 变换矩阵
*/
Shader "Lsd/BoneBakeAnim"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

		_BoneCount ("骨骼数量", int) = 21
		_FrameCount ("帧数", int) = 30
		_TexWidth("宽度", int) = 1
		_TexHeight("高度", int) = 1
		_PreTime("上次播放时间", Float) = 0
		_Interval("每帧时间(s)", Float) = 0.0333
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
			#pragma multi_compile_instancing

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
				float4 uv1 : TEXCOORD1;
				float4 uv2 : TEXCOORD2;

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
			uniform sampler2D _BoneBake;
			float _Interval;
			int _BoneCount;
			int _FrameCount;
			int _TexWidth;
			int _TexHeight;
			//float _PreTime;


			float2 ToUV(int index) {
				float2 uv = 0;
				uv.y = floor(index / _TexWidth);
				uv.x = index - uv.y * _TexWidth;

				uv.x = uv.x / _TexWidth;
				uv.y = uv.y / _TexHeight;
				return uv;
			}

			// 每个骨骼存矩阵前三行, 也就是 3个四维向量
			float4x4 GetBoneMatrix(int frame, int boneIndex) {
				int index = _BoneCount * 3 * frame + boneIndex * 3;
				float4 line0 = tex2Dlod(_BoneBake, float4(ToUV(index),     0, 0));
				float4 line1 = tex2Dlod(_BoneBake, float4(ToUV(index + 1), 0, 0));
				float4 line2 = tex2Dlod(_BoneBake, float4(ToUV(index + 2), 0, 0));
				float4x4 m = float4x4(line0, line1, line2, float4(0, 0, 0, 1));

				return m;
			}


            v2f vert (appdata v)
            {
                v2f o;

				UNITY_SETUP_INSTANCE_ID(v);

				int frame = floor(abs(_Time.y - UNITY_ACCESS_INSTANCED_PROP(Props, _PreTime)) / _Interval);
				frame = clamp(0, _FrameCount - 1, frame);


				// 骨骼变换矩阵 * 模型空间坐标 * 骨骼权重
				float4 pos = mul(GetBoneMatrix(frame, v.uv1.x), v.vertex) * v.uv1.y + \
					mul(GetBoneMatrix(frame, v.uv1.z), v.vertex) * v.uv1.w + \
					mul(GetBoneMatrix(frame, v.uv2.x), v.vertex) * v.uv2.y + \
					mul(GetBoneMatrix(frame, v.uv2.z), v.vertex) * v.uv2.w;

                o.vertex = UnityObjectToClipPos(pos);
				//o.vertex = UnityObjectToClipPos(v.vertex);
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
