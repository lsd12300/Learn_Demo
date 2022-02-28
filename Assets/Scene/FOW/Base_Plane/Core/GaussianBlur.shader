Shader "Fow/GaussianBlur"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
		_BlurSize ("BlurSize", Float) = 1
    }
    SubShader
    {
        Cull Off ZWrite Off ZTest Always

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                half4 vertex : POSITION;
                half2 uv : TEXCOORD0;
            };

            struct v2f
            {
                half2 uv : TEXCOORD0;
                half4 vertex : SV_POSITION;
            };

			// 卷积因子
			static const half BlurKernels3x3[9] = {
				0.077847, 0.123317, 0.077847,
				0.123317, 0.195346, 0.123317,
				0.077847, 0.123317, 0.077847,
				//0.0947416, 0.118318, 0.0947416,
				//0.118318, 0.147761, 0.118318,
				//0.0947416, 0.118318, 0.0947416,
			};
			static const half2 BlurTexelOffset3x3[9] = {
				half2(-1,1), half2(0,1), half2(1,1),
				half2(-1,0), half2(0,0), half2(1,0),
				half2(-1,-1), half2(0,-1), half2(1,-1),
			};
            sampler2D _MainTex;
            //half4 _MainTex_ST;
			//XX_TexelSize，XX纹理的像素相关大小width，height对应纹理的分辨率，x = 1/width, y = 1/height, z = width, w = height
			half4 _MainTex_TexelSize;
			half _BlurSize;

            v2f vert (appdata v)
            {
                v2f o;
                //o.vertex = UnityObjectToClipPos(v.vertex);
                //o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.vertex = mul(unity_ObjectToWorld, v.vertex);
				o.uv = v.uv.xy;
                return o;
            }

            fixed4 frag (v2f input) : SV_Target
            {
				fixed4 col = 0;
				for (int i = 0; i < 9; i++) {
					// #define TRANSFORM_TEX(tex,name) (tex.xy * name##_ST.xy + name##_ST.zw)
					// 取纹理周围的像素 按卷积因子进行叠加
					//col += tex2D(_MainTex, input.uv + BlurTexelOffset3x3[i] * _MainTex_ST.xy * _BlurSize) * BlurKernels3x3[i];
					//col += tex2D(_MainTex, input.uv + BlurTexelOffset3x3[i] * _MainTex_TexelSize.xy * _BlurSize) * BlurKernels3x3[i];
					col += tex2D(_MainTex, input.uv.xy + BlurTexelOffset3x3[i] * _MainTex_TexelSize * _BlurSize) * BlurKernels3x3[i];
				}
                return col;
            }
            ENDCG
        }
    }
}
