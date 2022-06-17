using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// һ��Bundle���� ���������л�Ϊjson�ļ� asset_bundle_config.json��
/// </summary>
public class BundleInfo
{
    /// <summary>
    /// ���bundle��id
    /// </summary>
    public int bundle_id;

    /// <summary>
    /// ���bundle������
    /// </summary>
    public string bundle_name;

    /// <summary>
    /// ���bundle����������Դ��·���б�
    /// </summary>
    public List<string> assets;
}
/// <summary>
/// һ��Asset���� ���������л�Ϊjson�ļ� asset_bundle_config.json��
/// </summary>
public class AssetInfo
{
    /// <summary>
    /// �����Դ�����·��
    /// </summary>
    public string asset_path;

    /// <summary>
    /// �����Դ������bundle��id
    /// </summary>
    public int bundle_id;

    /// <summary>
    /// �����Դ��������bundle�б��id
    /// </summary>
    public List<int> dependencies;
}
/// <summary>
/// BundleInfoSet���� ��Ӧ ����json�ļ� asset_bundle_config.json
/// </summary>
public class BundleInfoSet
{
    public BundleInfoSet(int bundleCount, int assetCount)
    {
        bundles = new BundleInfo[bundleCount];
        assetInfos = new AssetInfo[assetCount];
    }

    public BundleInfoSet() { }

    /// <summary>
    /// bundle���飬�����index��bundle��Id
    /// </summary>
    public BundleInfo[] bundles;

    /// <summary>
    /// asset ����
    /// </summary>
    public AssetInfo[] assetInfos;

    /// <summary>
    /// ����һ��bundle��¼
    /// </summary>
    /// <param name="bundleId">bundle��id</param>
    /// <param name="bundleInfo">bundle����</param>
    public void AddBundle(int bundleId, BundleInfo bundleInfo)
    {
        bundles[bundleId] = bundleInfo;
    }

    /// <summary>
    /// ����һ����Դ��¼
    /// </summary>
    /// <param name="index"></param>
    /// <param name="assetInfo"></param>
    public void AddAsset(int index, AssetInfo assetInfo)
    {
        assetInfos[index] = assetInfo;
    }
    public class ABMgr : MonoBehaviour
    {
        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
