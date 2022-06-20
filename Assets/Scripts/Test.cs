using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Test : MonoBehaviour
{
    private GameObject oneSphere;

    IEnumerator Start()
    {
        yield return AssetsLoader.Instance.LoadAssetBoundleConfig();

        oneSphere = AssetsLoader.Instance.Clone("Assets/GAssets/Prefabs/Capsule.prefab");

        yield return new WaitForSeconds(5.0f);

        Destroy(oneSphere);
    }

    public void Update()
    {
        AssetsLoader.Instance.Update();
    }
}
