using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.IO;
using LitJson;
using Newtonsoft;
using Newtonsoft.Json;

public class ABEditor : MonoBehaviour
{
    public static string rootPath = Application.dataPath + "/GAssets";
    /// <summary>
    ///  ������Ҫ�����ab����Ϣ��һ��AssetBundle�ļ���Ӧ��һ��AssetBundleBuild����
    /// </summary>
    public static List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();

    /// <summary>
    /// bundle �ļ����·��
    /// </summary>
    public static string abOutputPath = Application.streamingAssetsPath;
    /// <summary>
    /// ��¼�ĸ�asset��Դ�����ĸ�bundle�ļ�
    /// </summary>
    public static Dictionary<string, string> asset2bundle = new Dictionary<string, string>();

    /// <summary>
    /// ��¼ÿ��asset��Դ������bundle�ļ��б�
    /// </summary>
    public static Dictionary<string, List<string>> asset2Dependencies = new Dictionary<string, List<string>>();

    [MenuItem("ABEditor/BuildAssetBundle")]
    public static void BulidAssetBoundle()
    {
        print("BuildAssetBoundle");
        asset2bundle.Clear();
        asset2Dependencies.Clear();

        ScanChildDireations(new DirectoryInfo(rootPath));
        //foreach (AssetBundleBuild build in bundles)
        //{
        //    Debug.Log("AB�����֣�" + build.assetBundleName);
        //}
        CalculateDependencies();
        if (Directory.Exists(abOutputPath) == true)
        {
            Directory.Delete(abOutputPath, true);
        }
        Directory.CreateDirectory(abOutputPath);

        BuildPipeline.BuildAssetBundles(abOutputPath, bundles.ToArray(), BuildAssetBundleOptions.None, EditorUserBuildSettings.activeBuildTarget);
        SaveByBuilds();
        AssetDatabase.Refresh();
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
           // Debug.Log(assetName);
            
        }
        if (AssetNames.Count > 0)
        {
            // ��ʽ���� gassets_prefabs
            string assetbundleName = directoryInfo.FullName.Substring(Application.dataPath.Length + 1).Replace('\\', '_').ToLower();
            //Debug.Log(assetbundleName);
            AssetBundleBuild build = new AssetBundleBuild();
            build.assetBundleName = assetbundleName;
            build.assetNames = new string[AssetNames.Count];
            for (int i = 0; i < AssetNames.Count; i++)
            {
                build.assetNames[i] = AssetNames[i];
                asset2bundle.Add(AssetNames[i], assetbundleName);
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
    /// <summary>
    /// ����ÿ����Դ��������ab���ļ��б�
    /// </summary>
    public static void CalculateDependencies()
    {
        foreach (string asset in asset2bundle.Keys)
        {
            // �����Դ�Լ����ڵ�bundle
            string assetBundle = asset2bundle[asset];

            string[] dependencies = AssetDatabase.GetDependencies(asset);
            List<string> assetList = new List<string>();
            if (dependencies != null && dependencies.Length > 0)
            {
                foreach (string oneAsset in dependencies)
                {
                    if (oneAsset == asset || oneAsset.EndsWith(".cs"))
                    {
                        continue;
                    }
                    assetList.Add(oneAsset);
                }
            }

            if (assetList.Count > 0)
            {
                List<string> abList = new List<string>();
                foreach (string oneAsset in assetList)
                {
                    bool result = asset2bundle.TryGetValue(oneAsset, out string bundle);
                    if (result == true)
                    {
                        if (bundle != assetBundle)
                        {
                            abList.Add(bundle);
                        }
                    }
                }
                asset2Dependencies.Add(asset, abList);
            }
        }
    }

    /// <summary>
    /// ����Դ������ϵ���ݱ����json��ʽ���ļ�
    /// </summary>
    private static void SaveByBuilds()
    {
        BundleInfoSet bundleInfoSet = new BundleInfoSet(bundles.Count, asset2bundle.Count);

        // ��¼AB����Ϣ
        int id = 0;
        foreach (AssetBundleBuild build in bundles)
        {
            BundleInfo bundleInfo = new BundleInfo();
            bundleInfo.bundle_name = build.assetBundleName;

            bundleInfo.assets = new List<string>();
            foreach (string asset in build.assetNames)
            {
                bundleInfo.assets.Add(asset);
            }

            bundleInfo.bundle_id = id;

            bundleInfoSet.AddBundle(id, bundleInfo);

            id++;
        }

        // ��¼ÿ����Դ��������ϵ
        int assetIndex = 0;
        foreach (var item in asset2bundle)
        {
            AssetInfo assetInfo = new AssetInfo();
            assetInfo.asset_path = item.Key;
            assetInfo.bundle_id = GetBundleID(bundleInfoSet, item.Value);
            assetInfo.dependencies = new List<int>();

            bool result = asset2Dependencies.TryGetValue(item.Key, out List<string> dependencies);
            if (result == true)
            {
                for (int i = 0; i < dependencies.Count; i++)
                {
                    assetInfo.dependencies.Add(GetBundleID(bundleInfoSet, dependencies[i]));
                }
            }

            bundleInfoSet.AddAsset(assetIndex, assetInfo);

            assetIndex++;
        }

        string jsonPath = abOutputPath + "/asset_bundle_config.json";
        if (File.Exists(jsonPath) == true)
        {
            File.Delete(jsonPath);
        }
        File.Create(jsonPath).Dispose();

        string jsonData = JsonMapper.ToJson(bundleInfoSet);
        jsonData = ConvertJsonString(jsonData);
        File.WriteAllText(jsonPath, jsonData);
    }

    /// <summary>
    /// ����һ��bundle�����֣�������bundle_id
    /// </summary>
    /// <param name="bundleInfoSet"></param>
    /// <param name="bundleName"></param>
    /// <returns></returns>
    private static int GetBundleID(BundleInfoSet bundleInfoSet, string bundleName)
    {
        foreach (BundleInfo bundleInfo in bundleInfoSet.bundles)
        {
            if (bundleName == bundleInfo.bundle_name)
            {
                return bundleInfo.bundle_id;
            }
        }

        return -1;
    }
    /// <summary>
    /// ��ʽ��json
    /// </summary>
    /// <param name="str">����json�ַ���</param>
    /// <returns>���ظ�ʽ������ַ���</returns>
    private static string ConvertJsonString(string str)
    {
        JsonSerializer serializer = new JsonSerializer();
        TextReader tr = new StringReader(str);
        JsonTextReader jtr = new JsonTextReader(tr);
        object obj = serializer.Deserialize(jtr);
        if (obj != null)
        {
            StringWriter textWriter = new StringWriter();
            JsonTextWriter jsonWriter = new JsonTextWriter(textWriter)
            {
                Formatting = Formatting.Indented,
                Indentation = 4,
                IndentChar = ' '
            };
            serializer.Serialize(jsonWriter, obj);
            return textWriter.ToString();
        }
        else
        {
            return str;
        }
    }
}
