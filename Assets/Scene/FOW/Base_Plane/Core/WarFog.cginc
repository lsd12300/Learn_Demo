#ifndef WAR_FOG_CGINC
#define WAR_FOG_CGINC

half4 g_WarFogCol;
sampler2D g_WarFog;
half4x4 g_WarFogVP;

#if WAR_FOG_ON
#define WAR_FOG_COORD(idx) half4 warfogUV : TEXCOORD##idx;

#define TRANSFER_WAR_FOG(o, vertex) o.warfogUV = mul(mul(g_WarFogVP, unity_ObjectToWorld), vertex);\
o.warfogUV.xy = (o.warfogUV.xy + o.warfogUV.w) * 0.5f;
// 正交矩阵 w = 1,  所以 o.warfogUV.xy = (o.warfogUV.xy + 1) * 0.5f;
//    将 [-1,1] -->> 变换到屏幕空间 [0,1]

#define APPLY_WAR_FOG(i, col) half2 warfogUV = i.warfogUV.xy / i.warfogUV.w;\
half warfog = 1 - tex2D(g_WarFog, warfogUV).r;\
col.rgb = lerp(col.rgb, g_WarFogCol, warfog);\
col.a = 1 - warfog;
//col.rgb = g_WarFogCol;\

#else
#define WAR_FOG_COORD(idx)

#define TRANSFER_WAR_FOG(o, vertex)

#define APPLY_WAR_FOG(i, col)
#endif

#endif