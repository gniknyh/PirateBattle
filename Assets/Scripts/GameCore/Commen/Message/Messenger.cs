/*
 * Usage examples:
 	1. Messenger.AddListener<GameObject>("prop collected", PropCollected);
 	   Messenger.Broadcast<GameObject>("prop collected", prop);
 	2. Messenger.AddListener<float>("speed changed", SpeedChanged);
 	   Messenger.Broadcast<float>("speed changed", 0.5f);
 * 
 * Messenger在加载新关卡时自动清除它的evenTable

 * 不想被清理的消息，应该调用Messenger.MarkAsPermanent(string)
 * 
 */

//#define LOG_ALL_MESSAGES
//#define LOG_ADD_LISTENER
//#define LOG_BROADCAST_MESSAGE
#define REQUIRE_LISTENER

using System;
using System.Collections.Generic;
using UnityEngine;

public static class Messenger
{
   
    //禁用未使用的变量警告
#pragma warning disable 0414
    //确保在游戏开始时自动创建messenger。.
    private static MessengerHelper messengerHelper = (new GameObject("MessengerHelper")).AddComponent<MessengerHelper>();
#pragma warning restore 0414

    public static  Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();
    //不应该删除Message handlers，除非Cleanup
    public static  List<string> permanentMessages = new List<string>();

    #region Helper methods

    /// <summary>
    /// 将某条信息标记为永久的
    /// </summary>
    /// <param name="eventType"></param>
    public static  void MarkAsPermanent(string eventType)
    {
#if LOG_ALL_MESSAGES
		Debug.Log("Messenger MarkAsPermanent \t\"" + eventType + "\"");
#endif
        permanentMessages.Add(eventType);
    }


    public static void Cleanup()
    {
#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER Cleanup. 删除没必要的监听事件");
#endif

        List<string> messagesToRemove = new List<string>();
        foreach (KeyValuePair<string, Delegate> et in eventTable)
        {
            if (permanentMessages.Contains(et.Key))
            {
                messagesToRemove.Add(et.Key);
            }             
        }

        foreach (string message in messagesToRemove)
        {
            eventTable.Remove(message);
        }

    }

    public static void PrintEventTable()
    {
        Debug.Log("\t\t\t=== MESSENGER PrintEventTable ===");

        foreach (KeyValuePair<string, Delegate> et in eventTable)
        {
            Debug.Log("\t\t\t" + et.Key + "\t\t" + et.Value);
        }

        Debug.Log("\n");
    }

    #endregion

    #region 消息日志记录和异常抛出
    
    public static void OnListenerAdding(string eventType, Delegate listenerBeingAdded)
    {
#if LOG_ALL_MESSAGES || LOG_ADD_LISTENER
		Debug.Log("MESSENGER OnListenerAdding \t\"" + eventType + "\"\t{" + listenerBeingAdded.Target + " -> " + listenerBeingAdded.Method + "}");
#endif

        if (!eventTable.ContainsKey(eventType))
        {
            eventTable.Add(eventType, null);
        }

        Delegate d = eventTable[eventType];
        if (d != null && d.GetType() != listenerBeingAdded.GetType())
        {
            throw new ListenerException(string.Format("尝试为事件类型添加具有不一致签名的侦听器{0}. Current listeners have type {1} and listener being added has type {2}", eventType, d.GetType().Name, listenerBeingAdded.GetType().Name));
        }
    }


    public static void OnListenerRemoving(string eventType, Delegate listenerBeingRemoved)
    {
#if LOG_ALL_MESSAGES
		Debug.Log("MESSENGER OnListenerRemoving \t\"" + eventType + "\"\t{" + listenerBeingRemoved.Target + " -> " + listenerBeingRemoved.Method + "}");
#endif

        if (eventTable.ContainsKey(eventType))
        {
            Delegate d = eventTable[eventType];

            if (d == null)
            {
                throw new ListenerException(string.Format("Attempting to remove listener with for event type \"{0}\" but current listener is null.", eventType));
            }
            else if (d.GetType() != listenerBeingRemoved.GetType())
            {
                throw new ListenerException(string.Format("Attempting to remove listener with inconsistent signature for event type {0}. Current listeners have type {1} and listener being removed has type {2}", eventType, d.GetType().Name, listenerBeingRemoved.GetType().Name));
            }
        }
        else
        {
            throw new ListenerException(string.Format("Attempting to remove listener for type \"{0}\" but Messenger doesn't know about this event type.", eventType));
        }
    }

    public static void OnListenerRemoved(string eventType)
    {
        if (eventTable[eventType] == null)
        {
            eventTable.Remove(eventType);
        }
    }

    public static void OnBroadcasting(string eventType)
    {
#if REQUIRE_LISTENER
        if (!eventTable.ContainsKey(eventType))
        {
            throw new BroadcastException(string.Format("Broadcasting message \"{0}\" but no listener found. Try marking the message with Messenger.MarkAsPermanent.", eventType));
        }
#endif
    }

    public static BroadcastException CreateBroadcastSignatureException(string eventType)
    {
        return new BroadcastException(string.Format("Broadcasting message \"{0}\" but listeners have a different signature than the broadcaster.", eventType));
    }

    public class BroadcastException : Exception
    {
        public BroadcastException(string msg)
            : base(msg)
        {
        }
    }

    public class ListenerException : Exception
    {
        public ListenerException(string msg)
            : base(msg)
        {
        }
    }
    #endregion

    #region AddListener
   
    static public void AddListener(string eventType, Callback handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback + handler;
    }

    static public void AddListener<T>(string eventType, Callback<T> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback < T > + handler;
    }

    static public void AddListener<T, U>(string eventType, Callback<T, U> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback<T, U> + handler;
    }

    static public void AddListener<T, U, V>(string eventType, Callback<T, U, V> handler)
    {
        OnListenerAdding(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback<T, U, V> + handler;
    }

    #endregion

    #region RemoveListener

    static public void RemoveListener(string eventType, Callback handler)
    {
        OnListenerRemoving(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback - handler;
        OnListenerRemoved(eventType);
    }

    static public void RemoveListener<T>(string eventType, Callback<T> handler)
    {
        OnListenerRemoving(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback<T> - handler;
        OnListenerRemoved(eventType);
    }

    static public void RemoveListener<T, U>(string eventType, Callback<T, U> handler)
    {
        OnListenerRemoving(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback<T, U> - handler;
        OnListenerRemoved(eventType);
    }

    static public void RemoveListener<T, U, V>(string eventType, Callback<T, U, V> handler)
    {
        OnListenerRemoving(eventType, handler);
        eventTable[eventType] = eventTable[eventType] as Callback < T, U, V > - handler;
        OnListenerRemoved(eventType);
    }
    #endregion

    #region Broadcast

    static public void Broadcast(string eventType)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback callback = d as Callback;
            if (callback != null)
                callback();
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    static public void Broadcast<T>(string eventType, T arg1)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T> callback = d as Callback<T>;
            if (callback != null)            
                callback(arg1);
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    static public void Broadcast<T, U>(string eventType, T arg1, U arg2)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U> callback = d as Callback<T, U>;
            if (callback != null)          
                callback(arg1, arg2);
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    static public void Broadcast<T, U, V>(string eventType, T arg1, U arg2, V arg3)
    {
#if LOG_ALL_MESSAGES || LOG_BROADCAST_MESSAGE
		Debug.Log("MESSENGER\t" + System.DateTime.Now.ToString("hh:mm:ss.fff") + "\t\t\tInvoking \t\"" + eventType + "\"");
#endif
        OnBroadcasting(eventType);

        Delegate d;
        if (eventTable.TryGetValue(eventType, out d))
        {
            Callback<T, U, V> callback = d as Callback<T, U, V>;
            if (callback != null)
                callback(arg1, arg2, arg3);
            else
                throw CreateBroadcastSignatureException(eventType);
        }
    }

    #endregion
}