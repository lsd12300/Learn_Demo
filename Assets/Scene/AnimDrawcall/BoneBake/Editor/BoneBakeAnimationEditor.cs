using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;


namespace AnimDrawcall
{

    /// <summary>
    ///  每帧采样所有骨骼的矩阵,  存储在 二进制文件中
    ///     [图片不能存储 负数值,  所以存成二进制文件, 运行时 转化贴图]
    /// </summary>
    public class BoneBakeAnimationEditor : EditorWindow
    {

        private GameObject _obj;
        private SkinnedMeshRenderer _render;
        private Animation _anim;

        // 生成的二进制文件路径,  最终文件名   模型名#动作名#帧数#骨骼数#贴图宽#贴图高
        private const string ByteFilePath = "Assets/Scene/AnimDrawcall/BoneBake/BakeBytes/{0}.bytes";

        // 所有导出的动画数据
        private const string AnimInfoFilePath = "Assets/Scene/AnimDrawcall/BoneBake/BakeBytes/AllBakeData.bytes";


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

                MeshBoneWeightUV(_render);      // 写入骨骼权重数据
            }
        }

        #endregion


        #region 采样
        
        /// <summary>
        ///  写入骨骼权重数据
        /// </summary>
        void MeshBoneWeightUV(SkinnedMeshRenderer render)
        {
            if (render == null)
            {
                Debug.LogError("SkinnedMeshRenderer 不能为空");
                return;
            }

            List<Vector4> uv1 = new List<Vector4>();
            List<Vector4> uv2 = new List<Vector4>();

            var ws = render.sharedMesh.boneWeights;         // 权重数据
            for (int i = 0; i < ws.Length; i++)
            {
                var w = ws[i];
                uv1.Add(new Vector4(w.boneIndex0, w.weight0, w.boneIndex1, w.weight1));
                uv2.Add(new Vector4(w.boneIndex2, w.weight2, w.boneIndex3, w.weight3));
            }
            render.sharedMesh.SetUVs(1, uv1);
            render.sharedMesh.SetUVs(2, uv2);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }

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

            // bindposes -- mesh空间 变换到 骨骼空间的矩阵
            Matrix4x4[] bindPos = render.sharedMesh.bindposes;
            var bones = render.bones;

            anim.Play(name);
            int frameCount = Mathf.CeilToInt(animState.length * animState.clip.frameRate);
            int texWidth = 1, texHeight = 1;
            GetTexWidth(frameCount, bones.Length, out texWidth, out texHeight);
            Texture2D tex = new Texture2D(texWidth, texHeight, TextureFormat.RGBAHalf, false, true);
            tex.filterMode = FilterMode.Point;
            tex.wrapMode = TextureWrapMode.Clamp;
            var pixels = tex.GetPixels();

            int index = 0;
            for (int i = 0; i < frameCount; i++)
            {
                float rate = (float)i / (frameCount - 1);
                animState.normalizedTime = rate;    // 当前采样时间
                anim.Sample();     // 立即生效动画, 而不是等到 LateUpdate()

                for (int j = 0; j < bones.Length; j++)
                {
                    // 计算蒙皮矩阵
                    Matrix4x4 m = _render.transform.worldToLocalMatrix * bones[j].localToWorldMatrix * bindPos[j];
                    for (int k = 0; k < 3; k++)
                    {
                        pixels[index] = new Color(m[k, 0], m[k, 1], m[k, 2], m[k, 3]);
                        index++;
                    }
                }

            }
            tex.SetPixels(pixels);
            tex.Apply();

            //var path = string.Format("Assets/Scene/AnimDrawcall/BoneBake/BakeBytes/{0}.bytes", 
            var path = string.Format(ByteFilePath,
                Utils.GenByteFileName(anim.gameObject, name, frameCount, bones.Length, texWidth, texHeight));
            File.WriteAllBytes(path, tex.GetRawTextureData());              // 存储字节文件
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }




        /// <summary>
        ///  贴图宽高为 2幂时   效率高
        /// </summary>
        /// <param name="frameCount"></param>
        /// <param name="boneCount"></param>
        /// <returns></returns>
        static void GetTexWidth(int frameCount, int boneCount, out int w, out int h)
        {
            int pixelCount = frameCount * boneCount * 3;
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


        #region 数据

        // 动画信息,  <字节文件名, 宽#高#字节文件名>
        private Dictionary<string, string> _animInfos;

        void InitAnimInfo()
        {
            if (_animInfos == null)
            {
                _animInfos = new Dictionary<string, string>();
                var lines = File.ReadAllLines(AnimInfoFilePath);
                foreach (var line in lines)
                {
                    
                }
            }
        }



        #endregion

        #region 编辑器
        [MenuItem("Lsd/BoneBakeAnimtionWin &w")]
        static void Init()
        {
            BoneBakeAnimationEditor window = (BoneBakeAnimationEditor)EditorWindow.GetWindow(typeof(BoneBakeAnimationEditor));
            window.titleContent = new GUIContent("骨骼烘焙");
            window.Show();
        }

        [MenuItem("Lsd/LogPath &l")]
        static void LogPath()
        {
            var obj = Selection.objects[0];
            Debug.Log(AssetDatabase.GetAssetPath(obj));
        }

        #endregion
    }
}