using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public abstract class Singleton<T> where T : new()
{
    private Singleton()
    {

    }

    private static T instance = new T();

    public static T Instance
    {
        get { return instance; }
    }

}

public class UnitySingleton<T> : MonoBehaviour where T : Component
{
    private static T instance;

    public static T Instance
    {
        get
        {
            if (!instance)
            {
                instance = FindObjectOfType<T>();
                if (!instance)
                {
                    GameObject go = new GameObject();
                    go.hideFlags = HideFlags.HideAndDontSave;
                    instance = go.AddComponent<T>();

                }
            }
            return instance; 

        }
    }

    public virtual void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        if (!instance)
            instance = this as T;
        else
            Destroy(gameObject);
    }


}


