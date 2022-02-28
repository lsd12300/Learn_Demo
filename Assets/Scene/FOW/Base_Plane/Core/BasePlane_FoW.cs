using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Fow
{
    [RequireComponent(typeof(Camera))]
    public class BasePlane_FoW : MonoBehaviour
    {

        public string warEyeLayer = "WarEye";
        public float halfWidth;
        public float halfHeight;

        public Color fogColor = Color.cyan;
        public bool mark = true;

        Transform mCenter;
        RenderTexture mFogBuff;
        RenderTexture mFogBuff2;
        Camera mCamForFog;


        void Start()
        {
            mCenter = this.transform;
            mCenter.forward = -Vector3.up;
            mCamForFog = GetComponent<Camera>();
            mCamForFog.orthographic = true;
            mCamForFog.orthographicSize = halfHeight;
            if (mark)
            {
                mCamForFog.clearFlags = CameraClearFlags.Nothing;
            }
            else
            {
                mCamForFog.clearFlags = CameraClearFlags.SolidColor;
                mCamForFog.backgroundColor = Color.black;
            }
            mFogBuff = new RenderTexture(384 * (int)(halfWidth / halfHeight), 384, 0, RenderTextureFormat.R8);
            mFogBuff2 = new RenderTexture(mFogBuff);
            mCamForFog.targetTexture = mFogBuff;
            mCamForFog.cullingMask = LayerMask.GetMask(warEyeLayer);

            // 变换到 相机空间的矩阵
            var matrixP = GL.GetGPUProjectionMatrix(mCamForFog.projectionMatrix, false);
            Shader.SetGlobalMatrix("g_WarFogVP", matrixP * mCamForFog.worldToCameraMatrix);
            Shader.SetGlobalTexture("g_WarFog", mFogBuff2);
            Shader.SetGlobalColor("g_WarFogCol", fogColor);
        }

        private void OnValidate()
        {
            Shader.SetGlobalColor("g_WarFogCol", fogColor);
            if (mCamForFog != null)
            {
                if (mark)
                {
                    mCamForFog.clearFlags = CameraClearFlags.Nothing;
                }
                else
                {
                    mCamForFog.clearFlags = CameraClearFlags.SolidColor;
                    mCamForFog.backgroundColor = Color.black;
                }
            }
        }

        private void OnEnable()
        {
            Shader.EnableKeyword("WAR_FOG_ON");
        }

        private void OnDisable()
        {
            Shader.DisableKeyword("WAR_FOG_ON");
        }

        public Material blurMat;
        public int blurIteration = 2;       // 模糊次数

        private void OnPreRender()
        {
            // 降低分辨率
            int RTLevel = 2;
            RenderTexture src = RenderTexture.GetTemporary(mFogBuff.width >> RTLevel, mFogBuff.height >> RTLevel, 0, mFogBuff.format);
            RenderTexture dest = RenderTexture.GetTemporary(mFogBuff.width >> RTLevel, mFogBuff.height >> RTLevel, 0, mFogBuff.format);
            Graphics.Blit(mFogBuff, src);
            for (int i = 0; i < blurIteration; i++)
            {
                Graphics.Blit(src, dest, blurMat);
                RenderTexture temp = src;
                src = dest;
                dest = temp;
            }
            Graphics.Blit(src, mFogBuff2);
            RenderTexture.ReleaseTemporary(src);
            RenderTexture.ReleaseTemporary(dest);
        }
    }
}