using UnityEngine;


namespace Hotfix
{
    public class Main
    {
        public static void Startup()
        {
            Debug.Log("Startup");
            GameObject go = new GameObject("HelloGo");
            //MonoProxy monoProxy = go.AddComponent<MonoProxy>();
            //monoProxy.Bind("HelloComponent");
            go.Bind("HelloComponent");
        }
    }
}