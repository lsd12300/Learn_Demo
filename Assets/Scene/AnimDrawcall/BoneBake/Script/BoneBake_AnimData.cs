using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;


namespace AnimDrawcall
{
    /// <summary>
    ///  骨骼烘焙 动画数据
    /// </summary>
    public class BoneBake_AnimData
    {

        public int FrameCount { get; set; }

        public int BoneCount { get; set; }

        // 贴图尺寸
        public int TexWidth { get; set; }

        public int TexHeight { get; set; }

        // 字节文件名
        public string ByteFileName { get; set; }

        public string ModelName { get; set; }

        public string ClipName { get; set; }


        // 字节文件名 ===  模型名#动作名#帧数#骨骼数#贴图尺寸
        public BoneBake_AnimData(string fileName)
        {
            ByteFileName = fileName;

            var s = fileName.Split('#');
            ModelName = s[0];
            ClipName = s[1];
            FrameCount = int.Parse(s[2]);
            BoneCount = int.Parse(s[3]);
            TexWidth = int.Parse(s[4]);
            TexHeight = int.Parse(s[5]);
        }

    }
}