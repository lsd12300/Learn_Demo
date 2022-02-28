using AnimDrawcall;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BoneAnim : MonoBehaviour
{

    public TextAsset byteFile;

    private MeshRenderer _render;
    private BoneBake_AnimData _data;
    private Texture2D _tex;

    private MaterialPropertyBlock _props;


    public int logFrame = 0;
    public int logBone = 0;

    void Start()
    {
        _render = GetComponent<MeshRenderer>();
        _props = new MaterialPropertyBlock();

        _data = new BoneBake_AnimData(byteFile.name);
        //_tex = new Texture2D(_data.BoneCount, _data.FrameCount * 3 + 3, TextureFormat.RGBAHalf, false, true);
        _tex = new Texture2D(_data.TexWidth, _data.TexHeight, TextureFormat.RGBAHalf, false, true);
        _tex.filterMode = FilterMode.Point;
        _tex.wrapMode = TextureWrapMode.Repeat;
        _tex.LoadRawTextureData(byteFile.bytes);
        _tex.Apply(false, false);

        _render.sharedMaterial.SetTexture("_BoneBake", _tex);
        _render.sharedMaterial.SetFloat("_BoneCount", _data.BoneCount);
        _render.sharedMaterial.SetFloat("_FrameCount", _data.FrameCount);
        _render.sharedMaterial.SetFloat("_TexWidth", _data.TexWidth);
        _render.sharedMaterial.SetFloat("_TexHeight", _data.TexHeight);
    }


    void Update()
    {

        if (Input.GetKeyDown(KeyCode.A))
        {

            Matrix4x4 m = new Matrix4x4();
            //for (int id = 0; id < _data.BoneCount; id++)
            //{
            //    for (int i = 0; i < 3; i++)
            //    {
            //        m.SetRow(i, _tex.GetPixel(id, logFrame * 3 + i));
            //    }
            //    Utils.Log(m);
            //}
            var pis = _tex.GetPixels();
            for (int i = 0; i < 3; i++)
            {
                int index = _data.BoneCount * 3 * logFrame + logBone * 3;
                m.SetRow(i, pis[index + i]);
            }
            Utils.Log(m);
        }


        if (Input.GetKeyDown(KeyCode.Space))
        {
            SetPropPreTime(Time.time);
        }
    }

    private void SetPropPreTime(float time)
    {
        _props.SetFloat("_PreTime", time);
        _render.SetPropertyBlock(_props);
    }
}
