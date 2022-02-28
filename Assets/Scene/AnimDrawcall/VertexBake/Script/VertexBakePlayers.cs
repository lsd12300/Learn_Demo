using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using AnimDrawcall;


public class VertexBakePlayers : MonoBehaviour
{
    public TextAsset byteFile;

    private MeshRenderer[] _render;
    private MaterialPropertyBlock[] _props;
    private BoneBake_AnimData _data;
    private Texture2D _tex;

    private int _childCount;


    // Start is called before the first frame update
    void Start()
    {
        Application.targetFrameRate = 30;

        _data = new BoneBake_AnimData(byteFile.name);
        _tex = new Texture2D(_data.TexWidth, _data.TexHeight, TextureFormat.RGBAHalf, false, true);
        _tex.filterMode = FilterMode.Point;
        _tex.wrapMode = TextureWrapMode.Repeat;
        _tex.LoadRawTextureData(byteFile.bytes);
        _tex.Apply(false, false);

        _childCount = transform.childCount;
        _render = new MeshRenderer[_childCount];
        _props = new MaterialPropertyBlock[_childCount];
        for (int i = 0; i < _childCount; i++)
        {
            var child = transform.GetChild(i);
            _render[i] = child.GetComponent<MeshRenderer>();

            child.localPosition = new Vector3(-8.0f + 0.8f * i, 0, 0);
        }

        _render[0].sharedMaterial.SetTexture("_AnimMap", _tex);
        _render[0].sharedMaterial.SetFloat("_VertexCount", _data.BoneCount);
        _render[0].sharedMaterial.SetFloat("_FrameCount", _data.FrameCount);
        _render[0].sharedMaterial.SetFloat("_TexWidth", _data.TexWidth);
        _render[0].sharedMaterial.SetFloat("_TexHeight", _data.TexHeight);
    }

    private float logTime = 0;
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            for (int i = 0; i < _childCount; i++)
            {
                var time = Random.Range(0.0f, 1.0f);
                time = 0;
                SetPropPreTime(i, Time.time + time);
            }
            logTime += 0.6f;
        }
    }


    private void SetPropPreTime(int index, float time)
    {
        if (_props[index] == null)
        {
            _props[index] = new MaterialPropertyBlock();
        }
        _props[index].SetFloat("_PreTime", time);
        _render[index].SetPropertyBlock(_props[index]);
    }
}
