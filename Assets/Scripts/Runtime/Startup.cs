using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public class Startup : MonoBehaviour
{
    public static ILRuntime.Runtime.Enviorment.AppDomain appdomain;

    void Start()
    {
        StartCoroutine(LoadILRuntime());
    }

    IEnumerator LoadILRuntime()
    {
        appdomain = new ILRuntime.Runtime.Enviorment.AppDomain();

        UnityWebRequest webRequest = UnityWebRequest.Get(StreamingAssetsPath("Hotfix.dll.txt"));
        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            yield break;
        }
        byte[] dll = webRequest.downloadHandler.data;
        webRequest.Dispose();

        webRequest = UnityWebRequest.Get(StreamingAssetsPath("Hotfix.pdb.txt"));
        yield return webRequest.SendWebRequest();
        if (webRequest.result != UnityWebRequest.Result.Success)
        {
            yield break;
        }
        byte[] pdb = webRequest.downloadHandler.data;
        webRequest.Dispose();

        appdomain.LoadAssembly(new MemoryStream(dll), new MemoryStream(pdb), new ILRuntime.Mono.Cecil.Pdb.PdbReaderProvider());
        //зЂВс
        appdomain.RegisterCrossBindingAdaptor(new CoroutineAdapter());

        OnILRuntimeInitialized();
    }

    void OnILRuntimeInitialized()
    {
        appdomain.Invoke("Hotfix.Main", "Startup", null, null);
    }

    public string StreamingAssetsPath(string fileName)
    {
        string path = Application.streamingAssetsPath + "/" + fileName;
        return path;
    }

}