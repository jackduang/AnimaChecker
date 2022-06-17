using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ABEditor : MonoBehaviour
{
    public static string rootPath = Application.dataPath + "/GAssets";
    /// <summary>
    ///  所有需要打包的ab包信息：一个AssetBundle文件对应了一个AssetBundleBuild对象
    /// </summary>
    public static List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
    
    [MenuItem("ABEditor/BuildAssetBundle")]
    public static void BulidAssetBoundle()
    {
        print("BuildAssetBoundle");
        ScanChildDireations(new DirectoryInfo(rootPath));
        //foreach (AssetBundleBuild build in bundles)
        //{
        //    Debug.Log("AB包名字：" + build.assetBundleName);
        //}
    }

    public static void ScanChildDireations(DirectoryInfo directoryInfo)
    {
        //#region [收集当前目录下的所有文件]
        List<string> AssetNames = new List<string>();
        FileInfo[] fileInfoList = directoryInfo.GetFiles();
        foreach (FileInfo fileInfo in fileInfoList)
        {
            if (fileInfo.FullName.EndsWith(".meta"))
            {
                continue;
            }
            // 格式类似 "Assets/GAssets/Prefabs/Sphere.prefab"
            
            string assetName = fileInfo.FullName.Substring(Application.dataPath.Length - "Assets".Length).Replace('\\', '/');
            AssetNames.Add(assetName);
            Debug.Log(assetName);
            
        }
        if (AssetNames.Count > 0)
        {
            // 格式类似 gassets_prefabs
            string assetbundleName = directoryInfo.FullName.Substring(Application.dataPath.Length + 1).Replace('\\', '_').ToLower();
            Debug.Log(assetbundleName);
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = assetbundleName;
            build.assetNames = new string[AssetNames.Count];
            for (int i = 0; i < AssetNames.Count; i++)
            {
                build.assetNames[i] = AssetNames[i];
            }
            bundles.Add(build);
        }
        #region [递归遍历当前文件夹下的子文件夹]

        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        foreach (DirectoryInfo info in dirs)
        {
            ScanChildDireations(info);
        }

        #endregion
    }
}
