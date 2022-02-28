using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fow
{

    /// <summary>
    ///  屏幕后处理
    ///     迷雾模糊
    /// </summary>
    [ExecuteInEditMode]
    [RequireComponent (typeof(Camera))]
    public class FowBlit : MonoBehaviour
    {

        [SerializeField]
        private Material _fowBlurMat;



        //private void OnRenderImage(RenderTexture source, RenderTexture destination)
        //{
        //    Graphics.Blit(source, destination, _fowBlurMat);
        //}

        //private void OnPreRender()
        //{
            
        //}
    }
}