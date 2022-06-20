using ILRuntime.CLR.TypeSystem;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// ��GameObject�Ϲ��ص������Ľű�����MonoProxy
/// ����ʱ������߼�ִ��ת�����󶨵Ķ�Ӧ�ȸ��ű�����ScriptObject
/// </summary>
public class MonoProxy : MonoBehaviour
{
    /// <summary>
    /// ��ǰ���MonoProxy����ӳ����ȸ��ű��������ַ���
    /// </summary>
    public string ScriptName;

    /// <summary>
    /// ӳ����ȸ��ű������͵Ķ���
    /// </summary>
    public object ScriptObject;

    /// <summary>
    /// ����MonoProxy�����һ���ȸ��ű�����һ��
    /// </summary>
    /// <param name="scriptName"></param>
    public void Bind(string scriptName)
    {
        ScriptName = "Hotfix." + scriptName;

        ScriptObject = Startup.appdomain.Instantiate(ScriptName);

        IType scriptIType = Startup.appdomain.LoadedTypes[ScriptName];
        FieldInfo goField = scriptIType.ReflectionType.GetField("gameObject");
        goField.SetValue(ScriptObject, gameObject);

        Startup.appdomain.Invoke(ScriptName, "Awake", ScriptObject, null);
    }

    void Start()
    {
        Startup.appdomain.Invoke(ScriptName, "Start", ScriptObject, null);
    }

    void Update()
    {
        Startup.appdomain.Invoke(ScriptName, "Update", ScriptObject, null);
    }

    private void OnDestroy()
    {
        Startup.appdomain.Invoke(ScriptName, "OnDestroy", ScriptObject, null);
    }
}