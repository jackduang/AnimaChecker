using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;

public class ABEditor : MonoBehaviour
{
    public static string rootPath = Application.dataPath + "/GAssets";
    /// <summary>
    ///  ������Ҫ�����ab����Ϣ��һ��AssetBundle�ļ���Ӧ��һ��AssetBundleBuild����
    /// </summary>
    public static List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();
    
    [MenuItem("ABEditor/BuildAssetBundle")]
    public static void BulidAssetBoundle()
    {
        print("BuildAssetBoundle");
        ScanChildDireations(new DirectoryInfo(rootPath));
        //foreach (AssetBundleBuild build in bundles)
        //{
        //    Debug.Log("AB�����֣�" + build.assetBundleName);
        //}
    }

    public static void ScanChildDireations(DirectoryInfo directoryInfo)
    {
        //#region [�ռ���ǰĿ¼�µ������ļ�]
        List<string> AssetNames = new List<string>();
        FileInfo[] fileInfoList = directoryInfo.GetFiles();
        foreach (FileInfo fileInfo in fileInfoList)
        {
            if (fileInfo.FullName.EndsWith(".meta"))
            {
                continue;
            }
            // ��ʽ���� "Assets/GAssets/Prefabs/Sphere.prefab"
            
            string assetName = fileInfo.FullName.Substring(Application.dataPath.Length - "Assets".Length).Replace('\\', '/');
            AssetNames.Add(assetName);
            Debug.Log(assetName);
            
        }
        if (AssetNames.Count > 0)
        {
            // ��ʽ���� gassets_prefabs
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
        #region [�ݹ������ǰ�ļ����µ����ļ���]

        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        foreach (DirectoryInfo info in dirs)
        {
            ScanChildDireations(info);
        }

        #endregion
    }
}
