using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;

public class Utils
{
    private static StringBuilder _sb = new StringBuilder();

    public static void Log(params object[] objs)
    {
        _sb.Length = 0;
        for (int i = 0; i < objs.Length; i++)
        {
            if (i > 0)
            {
                _sb.Append(",  ");
            }
            _sb.Append(objs[i].ToString());
        }


        Debug.Log(_sb.ToString());
    }



    /// <summary>
    ///  字节文件名   模型名#动作名#帧数#骨骼数#贴图宽#贴图高
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="clipName"></param>
    /// <returns></returns>
    public static string GenByteFileName(GameObject obj, string clipName, int frameCount, int boneCount, int width, int height)
    {
        return string.Format("{0}#{1}#{2}#{3}#{4}#{5}", obj.name, clipName, frameCount, boneCount, width, height);
    }

    /// <summary>
    ///  字节文件名   模型名#动作名#帧数#骨骼数#贴图尺寸
    /// </summary>
    /// <param name="obj"></param>
    /// <param name="clipName"></param>
    /// <returns></returns>
    public static AnimDrawcall.BoneBake_AnimData ByteFileNameToInfo(string fileName)
    {
        return new AnimDrawcall.BoneBake_AnimData(fileName);
    }

}
