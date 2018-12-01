using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
//此管理器将确保在加载新级别时，messenger的eventTable将被清除。 
/// </summary>
public sealed class MessengerHelper : MonoBehaviour
{
    void Awake()
    {
        DontDestroyOnLoad(gameObject);
    }

    //每次关卡载入时都清除eventTable。
    public void OnLevelWasLoaded(int unused)
    {
        Messenger.Cleanup();
    }
}
