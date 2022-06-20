using LitJson;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// 内存中的单个bundle对象
/// </summary>
public class BundleRef
{
    /// <summary>
    /// 这个 bundle 的静态配置信息
    /// </summary>
    public BundleInfo bundleInfo;

    /// <summary>
    /// 加载到内存的Bundle对象
    /// </summary>
    public AssetBundle bundle;

    /// <summary>
    /// 这些BundleRef对象被哪些AssetRef对象依赖
    /// </summary>
    public List<AssetRef> children;

    /// <summary>
    /// BundleRef的构造函数
    /// </summary>
    /// <param name="bundleInfo"></param>
    public BundleRef(BundleInfo bundleInfo)
    {
        this.bundleInfo = bundleInfo;
    }
}

/// <summary>
/// 内存中的单个资源对象
/// </summary>
public class AssetRef
{
    /// <summary>
    /// 这个资源的配置信息
    /// </summary>
    public AssetInfo assetInfo;

    /// <summary>
    /// 这个资源所属的BundleRef对象
    /// </summary>
    public BundleRef bundleRef;

    /// <summary>
    /// 这个资源所依赖的BundleRef对象列表
    /// </summary>
    public BundleRef[] dependencies;

    /// <summary>
    /// 从bundle文件中提取出来的资源对象
    /// </summary>
    public Object asset;
    /// <summary>
    /// 是否是prefab
    /// </summary>
    public bool isGameobject;
    /// <summary>
    /// 这个AssetRef对象被哪些GameObject依赖
    /// </summary>
    public List<GameObject> children;

    /// <summary>
    /// AssetRef对象的构造函数
    /// </summary>
    /// <param name="assetInfo"></param>
    public AssetRef(AssetInfo assetInfo)
    {
        this.assetInfo = assetInfo;
    }
}
public class AssetsLoader
{
    /// <summary>
    /// 这个参数时调试专用，表明在编辑器模式下是否采用bundle方式加载，默认是不采用，直接使用编辑器API加载资源
    /// </summary>
    public bool ASSET_BUNDLE = false;
    public static AssetsLoader Instance
    {
        get
        {
            if (instance != null)
            {
                return instance;
            }
            else
            {
                instance = new AssetsLoader();
                return instance;
            }
        }
    }

    private static AssetsLoader instance = null;


    /// <summary>
    /// 内部工具函数 通过路径填充一个AssetRef对象并返回
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    private AssetRef LoadAssetRef_Runtime<T>(string path) where T : Object
    {
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        AssetRef assetRef = (AssetRef)Path2AssetRef[path];
        if (assetRef == null)
        {
            return null;
        }

        if (assetRef.asset == null)
        {
            // 1. 对于assetRef所依赖哪些bundle，如果不存在则先加载
            // 2. 建立assetRef.asset和这些所依赖的bundleRef之间的关系
            foreach (BundleRef bundleRef in assetRef.dependencies)
            {
                if (bundleRef.bundle == null)
                {
                    bundleRef.bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + bundleRef.bundleInfo.bundle_name);
                }
                if (bundleRef.children == null)
                {
                    bundleRef.children = new List<AssetRef>();
                }
                bundleRef.children.Add(assetRef);
            }

            // 1. 对于assetRef所属的这个bundle，如果不存在则先加载
            // 2. 建立assetRef.asset和这个bundleRef之间的关系
            if (assetRef.bundleRef.bundle == null)
            {
                assetRef.bundleRef.bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + assetRef.bundleRef.bundleInfo.bundle_name);
            }
            if (assetRef.bundleRef.children == null)
            {
                assetRef.bundleRef.children = new List<AssetRef>();
            }
            assetRef.bundleRef.children.Add(assetRef);

            // 这个assetRef依赖的bundleRef列表和自己所属的bundleRef都准备好后，就可以提取资源对象了
            assetRef.asset = assetRef.bundleRef.bundle.LoadAsset<T>(assetRef.assetInfo.asset_path);
            if (typeof(T) == typeof(GameObject) && assetRef.assetInfo.asset_path.EndsWith(".prefab"))
            {
                assetRef.isGameobject = true;
            }
            else
            {
                assetRef.isGameobject = false;
            }
        }

        return assetRef;
    }
    /// <summary>
    /// 在开发者模式下，加载资源对象
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path"></param>
    /// <returns></returns>
    private AssetRef LoadAssetRef_Editor<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        AssetRef oneAssetRef = new AssetRef(null);
        oneAssetRef.asset = AssetDatabase.LoadAssetAtPath<T>(path);
        return oneAssetRef;
#else
        return null;
#endif

    }

    /// <summary>
    /// 加载资源对象函数
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <param name="path">资源的路径</param>
    /// <returns></returns>
    private AssetRef LoadAssetRef<T>(string path) where T : Object
    {
#if UNITY_EDITOR
        if (!ASSET_BUNDLE)
        {
            return LoadAssetRef_Editor<T>(path);
        }
        else
        {
            return LoadAssetRef_Runtime<T>(path);
        }
#else
        return LoadAssetRef_Runtime<T>(path);
#endif
    }
    /// <summary>
    /// 根据资源路径创建一个GameObject对象
    /// </summary>
    /// <param name="path">这个资源的路径</param>
    /// <param name="name">给这个GameObject对象命个名字，如果传递null表明GameObject使用生成的默认名字</param>
    /// <returns></returns>
    public GameObject Clone(string path, string name = null)
    {
        AssetRef assetRef1 = LoadAssetRef<GameObject>(path);
        if(assetRef1 == null || assetRef1.asset == null)
        {
            return null;
        }

        if (string.IsNullOrEmpty(path))
        {
            return null;
        }

        AssetRef assetRef = (AssetRef)Path2AssetRef[path];
        if (assetRef == null)
        {
            return null;
        }

        if (assetRef.asset == null)
        {
            // 1. 对于assetRef所依赖哪些bundle，如果不存在则先加载
            // 2. 建立assetRef.asset和这些所依赖的bundleRef之间的关系
            foreach (BundleRef bundleRef in assetRef.dependencies)
            {
                if (bundleRef.bundle == null)
                {
                    bundleRef.bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + bundleRef.bundleInfo.bundle_name);
                }
                if (bundleRef.children == null)
                {
                    bundleRef.children = new List<AssetRef>();
                }
                bundleRef.children.Add(assetRef);
            }

            // 1. 对于assetRef所属的这个bundle，如果不存在则先加载
            // 2. 建立assetRef.asset和这个bundleRef之间的关系
            if (assetRef.bundleRef.bundle == null)
            {
                assetRef.bundleRef.bundle = AssetBundle.LoadFromFile(Application.streamingAssetsPath + "/" + assetRef.bundleRef.bundleInfo.bundle_name);
            }
            if (assetRef.bundleRef.children == null)
            {
                assetRef.bundleRef.children = new List<AssetRef>();
            }
            assetRef.bundleRef.children.Add(assetRef);

            // 这个assetRef依赖的bundleRef列表和自己所属的bundleRef都准备好后，就可以提取资源对象了
            assetRef.asset = assetRef.bundleRef.bundle.LoadAsset<GameObject>(assetRef.assetInfo.asset_path);
        }

        // 根据资源对象创建GameObject对象,并建立这个GameObject对象和assetRef之间的联系
        GameObject gameObject = Object.Instantiate(assetRef.asset) as GameObject;
        if (assetRef.children == null)
        {
            assetRef.children = new List<GameObject>();
        }
        assetRef.children.Add(gameObject);

        if (name != null)
        {
            gameObject.name = name;
        }

        return gameObject;
    }


    /// <summary>
    /// asset_bundle_config.json 配置文件路径
    /// </summary>
    public string assetBundleConfigPath = Application.streamingAssetsPath + "/asset_bundle_config.json";
    public Hashtable Path2AssetRef;
    /// <summary>
    /// 读取资源配置
    /// </summary>
    /// <returns></returns>
    public IEnumerator LoadAssetBoundleConfig()
    {
        UnityWebRequest request = UnityWebRequest.Get(assetBundleConfigPath);
        yield return request.SendWebRequest();
        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError(request.result.ToString());
            yield break;
        }

        string jsonData = request.downloadHandler.text;
        BundleInfoSet bundleInfoSet = JsonMapper.ToObject<BundleInfoSet>(jsonData);
        int bundleCount = bundleInfoSet.bundles.Length;
        BundleRef[] Id2BundleRef = new BundleRef[bundleCount];
        for (int i = 0; i < bundleCount; i++)
        {
            BundleInfo bundleInfo = bundleInfoSet.bundles[i];
            Id2BundleRef[bundleInfo.bundle_id] = new BundleRef(bundleInfo);
        }
        Path2AssetRef = new Hashtable();
        for (int i = 0; i < bundleInfoSet.assetInfos.Length; i++)
        {
            AssetInfo assetInfo = bundleInfoSet.assetInfos[i];

            // 装配一个AssetRef对象
            AssetRef assetRef = new AssetRef(assetInfo);
            assetRef.bundleRef = Id2BundleRef[assetInfo.bundle_id];
            int count = assetInfo.dependencies.Count;
            assetRef.dependencies = new BundleRef[count];
            for (int index = 0; index < count; index++)
            {
                int bundleId = assetInfo.dependencies[index];
                assetRef.dependencies[index] = Id2BundleRef[bundleId];
            }

            // 装配好了放到Path2AssetRef容器中
            Path2AssetRef.Add(assetInfo.asset_path, assetRef);
        }

        Debug.Log("Path2AssetRef.Count: " + Path2AssetRef.Count);
    }

    /// <summary>
    /// 加载非prefab资源 例如图片，音频，SkeletonDataAsset文件等
    /// </summary>
    /// <typeparam name="T">资源类型</typeparam>
    /// <param name="path">资源路径</param>
    /// <returns></returns>
    public T LoadAsset<T>(string path) where T : Object
    {
        if (typeof(T) == typeof(GameObject) || (!string.IsNullOrEmpty(path) && path.EndsWith(".prefab")))
        {
            Debug.LogError("不可以加载GameObject类型，请直接使用AssetLoader.Instance.Clone接口，path：" + path);
            return null;
        }

        AssetRef assetRef = LoadAssetRef<T>(path);
        if (assetRef == null || assetRef.asset == null)
        {
            return null;
        }

        return assetRef.asset as T;
    }

    /// <summary>
    /// 自动卸载Bundle
    /// </summary>
    public void Update()
    {
        if (Path2AssetRef != null)
        {
            foreach (AssetRef assetRef in Path2AssetRef.Values)
            {
                if(assetRef.isGameobject)
                if (assetRef.children != null && assetRef.children.Count > 0)
                {
                    for (int i = assetRef.children.Count - 1; i >= 0; i--)
                    {
                        GameObject go = assetRef.children[i];
                        if (go == null)
                        {
                            assetRef.children.RemoveAt(i);
                        }
                    }

                    // 如果这个资源assetRef已经没有GameObject依赖它了，那么此assetRef就可以卸载了
                    if (assetRef.children.Count == 0)
                    {
                        assetRef.asset = null;

                        Resources.UnloadUnusedAssets();

                        // 对于assetRef所属的这个bundle，解除关系
                        assetRef.bundleRef.children.Remove(assetRef);
                        if (assetRef.bundleRef.children.Count == 0)
                        {
                            assetRef.bundleRef.bundle.Unload(true);
                        }

                        // 对于assetRef所依赖哪些bundle，解除关系
                        foreach (BundleRef bundleRef in assetRef.dependencies)
                        {
                            bundleRef.children.Remove(assetRef);
                            if (bundleRef.children.Count == 0)
                            {
                                bundleRef.bundle.Unload(true);
                            }
                        }
                    }
                }
            }
        }
    }
}