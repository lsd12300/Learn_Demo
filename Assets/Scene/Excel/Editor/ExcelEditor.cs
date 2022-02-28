using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using NPOI.HSSF.UserModel;
using NPOI.XSSF.UserModel;

public class ExcelEditor : EditorWindow
{
    private string DirName;

    [MenuItem("Lsd/ExcelOut")]
    static void Init()
    {
        ExcelEditor window = (ExcelEditor)EditorWindow.GetWindow(typeof(ExcelEditor));

        window.DirName = Path.Combine(Application.dataPath, "../数据表");

        window.Show();

        foreach (var path in Directory.GetFiles(window.DirName))
        {
            if (Path.GetFileName(path).StartsWith("~"))
            {
                continue;
            }
            Debug.Log(path + ",  " + File.Exists(path));

            var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            XSSFWorkbook wk = new XSSFWorkbook(fs);
            for (int i = 0; i < wk.NumberOfSheets; i++)
            {
                var st = wk.GetSheetAt(i);
                Debug.Log(st.SheetName);
            }
            fs.Close();
            fs.Dispose();
        }


        
    }

    private void OnGUI()
    {
        
    }
}
