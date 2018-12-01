#define DEBUG
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public static class  MyLog
{

    public static void Log<T>(List<T> list)
    {
#if DEBUG
        for (int i = 0; i < list.Count; i++)
        {
            Debug.Log(Time.timeSinceLevelLoad + "--> 打印数据list[i]:" + list[i]);
        }
#endif

    }

    public static void Log<T1, T2>(Dictionary<T1, T2> dict)
    {
#if DEBUG
        foreach (KeyValuePair<T1, T2> item in dict)
        {
            Debug.Log(Time.timeSinceLevelLoad + "--> 打印key值:" + item.Key + "+ 打印value值:" + item.Value);
        }
#endif
    }

    public static void Log(Hashtable ht)
    {
#if DEBUG
        IDictionaryEnumerator myEnumerator = ht.GetEnumerator();
        bool flag = myEnumerator.MoveNext();
        while (flag)
        {
            Debug.Log(Time.timeSinceLevelLoad + "--> 打印key值:" + myEnumerator.Key + "+ 打印value值:" + myEnumerator.Value);
            flag = myEnumerator.MoveNext();
        }
#endif
    }

    public static void Log<T>(T[] array)
    {
#if DEBUG
        for (int i = 0; i < array.Length; i++)
        {
            Debug.Log(Time.timeSinceLevelLoad + "--> 打印数据array[i]:" + array[i]);

        }
#endif
    }

    public static void Log(object message)
    {
#if DEBUG
        Debug.Log(Time.timeSinceLevelLoad + "打印输出:" + message);
#endif
    }

}
