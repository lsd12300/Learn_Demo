using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace Fow
{

    public class FowPlane : MonoBehaviour
    {

        [SerializeField]
        private Camera _orthCamera;

        private Material _mat;
        private RenderTexture _rt;
        [SerializeField]
        private GameObject _plane;

        
        void Start()
        {
            _rt = _orthCamera.targetTexture;
            _rt.memorylessMode = RenderTextureMemoryless.Depth;

            _mat = _plane.GetComponent<MeshRenderer>().sharedMaterial;
            _mat.SetMatrix("_OrthMatrix", GL.GetGPUProjectionMatrix(_orthCamera.projectionMatrix, true) * _orthCamera.worldToCameraMatrix);
            //_mat.SetMatrix("_OrthMatrix", _orthCamera.projectionMatrix);
        }

        private void OnPreRender()
        {
            //_rt.Release();
            //Debug.Log("OnPreRender");
            //_rt.Release();
            //_rt = RenderTexture.GetTemporary(256, 256);
            //_orthCamera.targetTexture = _rt;


            //_rt.Release();
        }
    }
}