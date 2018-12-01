#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using UnityEditor;

public static class ScriptableObjectUtil
{
    public static void CreateAsset<T>() where T : ScriptableObject
    {
        ScriptableObject asset = ScriptableObject.CreateInstance<T>();
        ProjectWindowUtil.CreateAsset(asset, "New " + typeof(T).Name + ".asset");
    } 

}

#endif