using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FogRender
{
    private Material m_EffectMaterial;

    private Material m_BlurMaterial;

    /// <summary>
    /// 世界空间到迷雾投影空间矩阵
    /// </summary>
    private Matrix4x4 m_WorldToProjector;

    private int m_BlurInteration;


    public FogRender(Shader effectShader, Shader blurShader, Shader miniMapRenderShader, Vector3 position, float xSize, float zSize, Color fogColor, float blurOffset, int blurInteration)
    {
        m_EffectMaterial = new Material(effectShader);
        //m_EffectMaterial.SetFloat("_BlurOffset", blurOffset);
        m_EffectMaterial.SetColor("_FogColor", fogColor);

        m_WorldToProjector = default(Matrix4x4);
        m_WorldToProjector.m00 = 1.0f / xSize;
        m_WorldToProjector.m03 = -1.0f / xSize * position.x + 0.5f;
        m_WorldToProjector.m12 = 1.0f / zSize;
        m_WorldToProjector.m13 = -1.0f / zSize * position.z + 0.5f;
        m_WorldToProjector.m33 = 1.0f;

        m_EffectMaterial.SetMatrix("internal_WorldToProjector", m_WorldToProjector);

        if (blurShader && blurInteration > 0 && blurOffset > 0)
        {
            m_BlurMaterial = new Material(blurShader);
            m_BlurMaterial.SetFloat("_Offset", blurOffset);
        }
        m_BlurInteration = blurInteration;
    }

    /// <summary>
    /// 渲染战争迷雾
    /// </summary>
    /// <param name="camera"></param>
    /// <param name="src"></param>
    /// <param name="dst"></param>
    public void RenderFogOfWar(Camera camera, RenderTexture fogTexture, RenderTexture src, RenderTexture dst)
    {
        Matrix4x4 frustumCorners = Matrix4x4.identity;

        float fovWHalf = camera.fieldOfView * 0.5f;

        Vector3 toRight = camera.transform.right * camera.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad) * camera.aspect;
        Vector3 toTop = camera.transform.up * camera.nearClipPlane * Mathf.Tan(fovWHalf * Mathf.Deg2Rad);

        Vector3 topLeft = (camera.transform.forward * camera.nearClipPlane - toRight + toTop);
        float camScale = topLeft.magnitude * camera.farClipPlane / camera.nearClipPlane;

        topLeft.Normalize();
        topLeft *= camScale;

        Vector3 topRight = (camera.transform.forward * camera.nearClipPlane + toRight + toTop);
        topRight.Normalize();
        topRight *= camScale;

        Vector3 bottomRight = (camera.transform.forward * camera.nearClipPlane + toRight - toTop);
        bottomRight.Normalize();
        bottomRight *= camScale;

        Vector3 bottomLeft = (camera.transform.forward * camera.nearClipPlane - toRight - toTop);
        bottomLeft.Normalize();
        bottomLeft *= camScale;

        frustumCorners.SetRow(0, topLeft);
        frustumCorners.SetRow(1, topRight);
        frustumCorners.SetRow(2, bottomRight);
        frustumCorners.SetRow(3, bottomLeft);
        m_EffectMaterial.SetMatrix("_FrustumCorners", frustumCorners);


        if (m_BlurMaterial && fogTexture)
        {
            RenderTexture rt = RenderTexture.GetTemporary(fogTexture.width, fogTexture.height, 0);
            Graphics.Blit(fogTexture, rt, m_BlurMaterial);
            for (int i = 0; i <= m_BlurInteration; i++)
            {
                RenderTexture rt2 = RenderTexture.GetTemporary(fogTexture.width / 2, fogTexture.height / 2, 0);
                Graphics.Blit(rt, rt2, m_BlurMaterial);
                RenderTexture.ReleaseTemporary(rt);
                rt = rt2;
            }
            m_EffectMaterial.SetTexture("_FogTex", rt);
            CustomGraphicsBlit(src, dst, m_EffectMaterial);
            RenderTexture.ReleaseTemporary(rt);
        }
        else
        {
            m_EffectMaterial.SetTexture("_FogTex", fogTexture);
            CustomGraphicsBlit(src, dst, m_EffectMaterial);
        }
    }

    public void Release()
    {
        if (m_EffectMaterial)
            Object.Destroy(m_EffectMaterial);
        if (m_BlurMaterial)
            Object.Destroy(m_BlurMaterial);
        m_EffectMaterial = null;
        m_BlurMaterial = null;
    }

    private static void CustomGraphicsBlit(RenderTexture source, RenderTexture dest, Material fxMaterial)
    {
        //Graphics.Blit(source, dest, fxMaterial);
        //return;
        RenderTexture.active = dest;

        // dest为空时   所有的渲染结果将会绘制到 GameWindow

        fxMaterial.SetTexture("_MainTex", source);

        // 在 相机近裁剪面处  绘制一个面片

        GL.PushMatrix();
        GL.LoadOrtho();                 // 正交投影

        fxMaterial.SetPass(0);          // 指定 Pass 通道进行渲染

        GL.Begin(GL.QUADS);             // 绘制面片

        GL.MultiTexCoord2(0, 0.0f, 0.0f);       // 设置 TEXCOORD0 的 uv坐标
        // xy 平面正好  盖住整个屏幕
        //  z值 代表视椎体矩阵的 第几行,  重建 世界坐标时直接获取
        GL.Vertex3(0.0f, 0.0f, 3.0f); // BL ,   相机近裁剪面 左下角

        GL.MultiTexCoord2(0, 1.0f, 0.0f);
        GL.Vertex3(1.0f, 0.0f, 2.0f); // BR

        GL.MultiTexCoord2(0, 1.0f, 1.0f);
        GL.Vertex3(1.0f, 1.0f, 1.0f); // TR

        GL.MultiTexCoord2(0, 0.0f, 1.0f);
        GL.Vertex3(0.0f, 1.0f, 0.0f); // TL

        GL.End();
        GL.PopMatrix();
    }

}
