using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GameObjectEx
{
    /// <summary>
    /// ��GameObject���ͣ���չһ���º���Bind�����Ǹ�GameObject��һ���ȸ��Ľű�����
    /// </summary>
    /// <param name="gameObject"></param>
    /// <param name="scriptName"></param>
    public static void Bind(this GameObject gameObject, string scriptName)
    {
        MonoProxy monoProxy = gameObject.AddComponent<MonoProxy>();
        monoProxy.Bind(scriptName);
    }
}