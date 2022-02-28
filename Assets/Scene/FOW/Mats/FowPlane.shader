Shader "Lsd/Fow/FowPlane"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        //LOD 100

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
                float4 vertex : SV_POSITION;
				float4 uv : TEXCOORD0;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;

			// 不需要在编辑器中显示的属性, 可以不用在 Properties 块内声明
			float4x4 _OrthMatrix;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);

				o.uv = mul(mul(_OrthMatrix, unity_ObjectToWorld), v.vertex);
				o.uv.xy = (o.uv.xy + o.uv.w) * 0.5f;
				//o.uv.y = 1 - o.uv.y;
#if UNITY_UV_STARTS_AT_TOP
				o.uv.y = 1 - o.uv.y;
#endif
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
