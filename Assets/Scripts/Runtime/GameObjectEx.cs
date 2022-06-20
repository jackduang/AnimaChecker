using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectEx
{
    /// <summary>
    /// 给GameObject类型，扩展一个新函数Bind，就是给GameObject绑定一个热更的脚本对象
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="scriptName"></param>
    public static void Bind(this GameObject gameObject, string scriptName)
    {
        MonoProxy monoProxy = gameObject.AddComponent<MonoProxy>();
        monoProxy.Bind(scriptName);
    }
}