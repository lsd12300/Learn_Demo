using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

public class AnimMapBakerWindow : EditorWindow {

    private GameObject _obj;
    private SkinnedMeshRenderer _render;
    private Animation _anim;

    // 生成的二进制文件路径,  最终文件名   模型名#动作名#帧数#顶点数#贴图宽#贴图高
    private const string ByteFilePath = "Assets/Scene/AnimDrawcall/VertexBake/BakeBytes/{0}.bytes";

    // 所有导出的动画数据
    private const string AnimInfoFilePath = "Assets/Scene/AnimDrawcall/VertexBake/BakeBytes/AllBakeData.bytes";

    private const string LogFilePath = "Assets/Scene/AnimDrawcall/VertexBake/BakeBytes/LogBake.bytes";


    #region UI

    private string[] _popAnimNames;
    private bool[] _toggles;


    void OnGUI()
    {
        GUILayout.Space(50);
        _obj = (GameObject)EditorGUILayout.ObjectField(_obj, typeof(GameObject), true, GUILayout.Height(30));

        GUILayout.Space(30);

        if (GUILayout.Button("运行", GUILayout.Width(100), GUILayout.Height(30)))
        {
            EditorApplication.isPlaying = true;
        }

        GUILayout.Space(20);

        if (_obj == null)
        {
            return;
        }

        if (_obj == null)
        {
            return;
        }
        if ((_anim == null && _obj != null) || (_anim.name != _obj.name))
        {
            _render = _obj.GetComponentInChildren<SkinnedMeshRenderer>();
            _anim = _obj.GetComponent<Animation>();
            _popAnimNames = new string[_anim.GetClipCount()];
            _toggles = new bool[_anim.GetClipCount()];
            int index = 0;
            foreach (AnimationState state in _anim)
            {
                _popAnimNames[index] = state.name;
                _toggles[index] = false;
                index++;
            }
        }


        if (_anim != null && _popAnimNames != null)
        {
            EditorGUILayout.BeginVertical();
            for (int i = 0; i < _popAnimNames.Length; i++)
            {
                EditorGUILayout.BeginHorizontal();
                _toggles[i] = EditorGUILayout.Toggle(_toggles[i], GUILayout.Width(40));
                EditorGUILayout.LabelField(_popAnimNames[i]);
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndVertical();
        }

        GUILayout.Space(20);
        if (GUILayout.Button("采样", GUILayout.Width(100), GUILayout.Height(30)))
        {
            if (!EditorApplication.isPlaying)
            {
                if (EditorUtility.DisplayDialog("错误", "先运行游戏", "好的"))
                {
                    EditorApplication.isPlaying = true;
                }
                return;
            }

            for (int i = 0; i < _popAnimNames.Length; i++)
            {
                SampleClip(_render, _anim, _popAnimNames[i]);
            }
        }
    }

    #endregion


    #region 采样


    /// <summary>
    ///  采样 动画
    /// </summary>
    /// <param name="render"></param>
    /// <param name="anim"></param>
    /// <param name="name"></param>
    void SampleClip(SkinnedMeshRenderer render, Animation anim, string name)
    {
        if (render == null)
        {
            Debug.LogError("SkinnedMeshRenderer 不能为空");
            return;
        }
        if (anim == null)
        {
            Debug.LogError("Animation 不能为空");
            return;
        }
        if (string.IsNullOrEmpty(name))
        {
            Debug.LogError("导出的动画名 不能为空");
            return;
        }
        var animState = anim[name];
        if (animState == null)
        {
            Debug.LogError("动画[" + name + "]不存在");
            return;
        }

        var vertexCount = render.sharedMesh.vertexCount;
        anim.Play(name);
        int frameCount = Mathf.CeilToInt(animState.length * animState.clip.frameRate);
        int texWidth = 1, texHeight = 1;
        GetTexWidth(frameCount, vertexCount, out texWidth, out texHeight);
        Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false, true);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        var pixels = tex.GetPixels();

        StringBuilder sb = new StringBuilder();
        int index = 0;
        Vector3[] vertices;
        Mesh mesh = new Mesh();
        for (int i = 0; i < frameCount; i++)
        {
            float rate = (float)i / (frameCount - 1);
            animState.normalizedTime = rate;    // 当前采样时间
            anim.Sample();     // 立即生效动画, 而不是等到 LateUpdate()


            render.BakeMesh(mesh);          // 获取当前  模型数据
            vertices = mesh.vertices;
            for (int j = 0; j < vertexCount; j++)
            {
                var pos = vertices[j];
                pixels[index] = new Color(pos.x, pos.y, pos.z, 0);
                index++;
                sb.Append(string.Format("\n帧数:  {0},  顶点编号:  {1}", i, j));
                sb.Append(string.Format("({0}, {1}, {2})", pos.x, pos.y, pos.z));
            }

        }
        tex.SetPixels(pixels);
        tex.Apply();

        File.WriteAllText(LogFilePath, sb.ToString());

        var path = string.Format(ByteFilePath,
            Utils.GenByteFileName(anim.gameObject, name, frameCount, vertexCount, texWidth, texHeight));
        File.WriteAllBytes(path, tex.GetRawTextureData());              // 存储字节文件
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }




    /// <summary>
    ///  贴图宽高为 2幂时   效率高
    /// </summary>
    /// <param name="frameCount"></param>
    /// <param name="vertexCount"></param>
    /// <returns></returns>
    static void GetTexWidth(int frameCount, int vertexCount, out int w, out int h)
    {
        int pixelCount = frameCount * vertexCount;
        w = 1;
        h = 1;
        while (w * h < pixelCount)
        {
            if (h >= w)
                w *= 2;
            else
                h *= 2;
        }
    }
    #endregion


    #region 编辑器
    [MenuItem("Lsd/VertexBakeAnimtionWin &q")]
    static void Init()
    {
        AnimMapBakerWindow window = (AnimMapBakerWindow)EditorWindow.GetWindow(typeof(AnimMapBakerWindow));
        window.titleContent = new GUIContent("顶点烘焙");
        window.Show();
    }


    #endregion
}
