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
    ///  所有需要打包的ab包信息：一个AssetBundle文件对应了一个AssetBundleBuild对象
    /// </summary>
    public static List<AssetBundleBuild> bundles = new List<AssetBundleBuild>();

    /// <summary>
    /// bundle 文件输出路径
    /// </summary>
    public static string abOutputPath = Application.streamingAssetsPath;
    /// <summary>
    /// 记录哪个asset资源属于哪个bundle文件
    /// </summary>
    public static Dictionary<string, string> asset2bundle = new Dictionary<string, string>();

    /// <summary>
    /// 记录每个asset资源依赖的bundle文件列表
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
        //    Debug.Log("AB包名字：" + build.assetBundleName);
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
           // Debug.Log(assetName);
            
        }
        if (AssetNames.Count > 0)
        {
            // 格式类似 gassets_prefabs
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
        #region [递归遍历当前文件夹下的子文件夹]

        DirectoryInfo[] dirs = directoryInfo.GetDirectories();
        foreach (DirectoryInfo info in dirs)
        {
            ScanChildDireations(info);
        }

        #endregion
    }
    /// <summary>
    /// 计算每个资源所依赖的ab包文件列表
    /// </summary>
    public static void CalculateDependencies()
    {
        foreach (string asset in asset2bundle.Keys)
        {
            // 这个资源自己所在的bundle
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
    /// 将资源依赖关系数据保存成json格式的文件
    /// </summary>
    private static void SaveByBuilds()
    {
        BundleInfoSet bundleInfoSet = new BundleInfoSet(bundles.Count, asset2bundle.Count);

        // 记录AB包信息
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

        // 记录每个资源的依赖关系
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
    /// 根据一个bundle的名字，返回其bundle_id
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
    /// 格式化json
    /// </summary>
    /// <param name="str">输入json字符串</param>
    /// <returns>返回格式化后的字符串</returns>
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
