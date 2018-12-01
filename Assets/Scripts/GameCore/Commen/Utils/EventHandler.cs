using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class EventHandler : MonoBehaviour
{

    private static Dictionary<object, Dictionary<string, Delegate>> s_EventTable = new Dictionary<object, Dictionary<string, Delegate>>();

    private static Dictionary<string, Delegate> s_GlobalEventTable = new Dictionary<string, Delegate>();

    private void Awake()
    {
        this.ClearTable();
    }

    private void OnDisable()
    {
        this.ClearTable();
    }

    private void OnDestroy()
    {
        this.ClearTable();
    }

    private void ClearTable()
    {
        EventHandler.s_EventTable.Clear();
        EventHandler.ExecuteEvent("OnEventHandlerClear");
    }

    private static void RegisterEvent(string eventName, Delegate handler)
    {
        Delegate a;
        if (EventHandler.s_GlobalEventTable.TryGetValue(eventName, out a))
        {
            EventHandler.s_GlobalEventTable[eventName] = Delegate.Combine(a, handler);
        }
        else
        {
            EventHandler.s_GlobalEventTable.Add(eventName, handler);
        }
    }

    private static void RegisterEvent(object obj, string eventName, Delegate handler)
    {
        if (obj == null)
        {
            Debug.LogError("EventHandler.RegisterEvent error: target object cannot be null.");
            return;
        }
        Dictionary<string, Delegate> dictionary;
        if (!EventHandler.s_EventTable.TryGetValue(obj, out dictionary))
        {
            dictionary = new Dictionary<string, Delegate>();
            EventHandler.s_EventTable.Add(obj, dictionary);
        }
        Delegate a;
        if (dictionary.TryGetValue(eventName, out a))
        {
            dictionary[eventName] = Delegate.Combine(a, handler);
        }
        else
        {
            dictionary.Add(eventName, handler);
        }
    }

    private static Delegate GetDelegate(string eventName)
    {
        Delegate result;
        if (EventHandler.s_GlobalEventTable.TryGetValue(eventName, out result))
        {
            return result;
        }
        return null;
    }

    private static Delegate GetDelegate(object obj, string eventName)
    {
        Dictionary<string, Delegate> dictionary;
        Delegate result;
        if (EventHandler.s_EventTable.TryGetValue(obj, out dictionary) && dictionary.TryGetValue(eventName, out result))
        {
            return result;
        }
        return null;
    }

    private static void UnregisterEvent(string eventName, Delegate handler)
    {
        Delegate source;
        if (EventHandler.s_GlobalEventTable.TryGetValue(eventName, out source))
        {
            EventHandler.s_GlobalEventTable[eventName] = Delegate.Remove(source, handler);
        }
    }

    private static void UnregisterEvent(object obj, string eventName, Delegate handler)
    {
        if (obj == null)
        {
            Debug.LogError("EventHandler.UnregisterEvent error: target object cannot be null.");
            return;
        }
        Dictionary<string, Delegate> dictionary;
        Delegate source;
        if (EventHandler.s_EventTable.TryGetValue(obj, out dictionary) && dictionary.TryGetValue(eventName, out source))
        {
            dictionary[eventName] = Delegate.Remove(source, handler);
        }
    }

    public static void RegisterEvent(string eventName, Action handler)
    {
        EventHandler.RegisterEvent(eventName, handler);
    }

    public static void RegisterEvent(object obj, string eventName, Action handler)
    {
        EventHandler.RegisterEvent(obj, eventName, handler);
    }

    public static void RegisterEvent<T>(string eventName, Action<T> handler)
    {
        EventHandler.RegisterEvent(eventName, handler);
    }

    public static void RegisterEvent<T>(object obj, string eventName, Action<T> handler)
    {
        EventHandler.RegisterEvent(obj, eventName, handler);
    }

    public static void RegisterEvent<T, U>(string eventName, Action<T, U> handler)
    {
        EventHandler.RegisterEvent(eventName, handler);
    }

    public static void RegisterEvent<T, U>(object obj, string eventName, Action<T, U> handler)
    {
        EventHandler.RegisterEvent(obj, eventName, handler);
    }

    public static void RegisterEvent<T, U, V>(string eventName, Action<T, U, V> handler)
    {
        EventHandler.RegisterEvent(eventName, handler);
    }

    public static void RegisterEvent<T, U, V>(object obj, string eventName, Action<T, U, V> handler)
    {
        EventHandler.RegisterEvent(obj, eventName, handler);
    }

    public static void RegisterEvent<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
    {
        EventHandler.RegisterEvent(eventName, handler);
    }

    public static void RegisterEvent<T, U, V, W>(object obj, string eventName, Action<T, U, V, W> handler)
    {
        EventHandler.RegisterEvent(obj, eventName, handler);
    }

    public static void ExecuteEvent(string eventName)
    {
        Action action = EventHandler.GetDelegate(eventName) as Action;
        if (action != null)
        {
            action();
        }
    }

    public static void ExecuteEvent(object obj, string eventName)
    {
        Action action = EventHandler.GetDelegate(obj, eventName) as Action;
        if (action != null)
        {
            action();
        }
    }

    public static void ExecuteEvent<T>(string eventName, T arg1)
    {
        Action<T> action = EventHandler.GetDelegate(eventName) as Action<T>;
        if (action != null)
        {
            action(arg1);
        }
    }

    public static void ExecuteEvent<T>(object obj, string eventName, T arg1)
    {
        Action<T> action = EventHandler.GetDelegate(obj, eventName) as Action<T>;
        if (action != null)
        {
            action(arg1);
        }
    }

    public static void ExecuteEvent<T, U>(string eventName, T arg1, U arg2)
    {
        Action<T, U> action = EventHandler.GetDelegate(eventName) as Action<T, U>;
        if (action != null)
        {
            action(arg1, arg2);
        }
    }

    public static void ExecuteEvent<T, U>(object obj, string eventName, T arg1, U arg2)
    {
        Action<T, U> action = EventHandler.GetDelegate(obj, eventName) as Action<T, U>;
        if (action != null)
        {
            action(arg1, arg2);
        }
    }

    public static void ExecuteEvent<T, U, V>(string eventName, T arg1, U arg2, V arg3)
    {
        Action<T, U, V> action = EventHandler.GetDelegate(eventName) as Action<T, U, V>;
        if (action != null)
        {
            action(arg1, arg2, arg3);
        }
    }

    public static void ExecuteEvent<T, U, V>(object obj, string eventName, T arg1, U arg2, V arg3)
    {
        Action<T, U, V> action = EventHandler.GetDelegate(obj, eventName) as Action<T, U, V>;
        if (action != null)
        {
            action(arg1, arg2, arg3);
        }
    }

    public static void ExecuteEvent<T, U, V, W>(string eventName, T arg1, U arg2, V arg3, W arg4)
    {
        Action<T, U, V, W> action = EventHandler.GetDelegate(eventName) as Action<T, U, V, W>;
        if (action != null)
        {
            action(arg1, arg2, arg3, arg4);
        }
    }

    public static void ExecuteEvent<T, U, V, W>(object obj, string eventName, T arg1, U arg2, V arg3, W arg4)
    {
        Action<T, U, V, W> action = EventHandler.GetDelegate(obj, eventName) as Action<T, U, V, W>;
        if (action != null)
        {
            action(arg1, arg2, arg3, arg4);
        }
    }

    public static void UnregisterEvent(string eventName, Action handler)
    {
        EventHandler.UnregisterEvent(eventName, handler);
    }

    public static void UnregisterEvent(object obj, string eventName, Action handler)
    {
        EventHandler.UnregisterEvent(obj, eventName, handler);
    }

    public static void UnregisterEvent<T>(string eventName, Action<T> handler)
    {
        EventHandler.UnregisterEvent(eventName, handler);
    }

    public static void UnregisterEvent<T>(object obj, string eventName, Action<T> handler)
    {
        EventHandler.UnregisterEvent(obj, eventName, handler);
    }

    public static void UnregisterEvent<T, U>(string eventName, Action<T, U> handler)
    {
        EventHandler.UnregisterEvent(eventName, handler);
    }

    public static void UnregisterEvent<T, U>(object obj, string eventName, Action<T, U> handler)
    {
        EventHandler.UnregisterEvent(obj, eventName, handler);
    }

    public static void UnregisterEvent<T, U, V>(string eventName, Action<T, U, V> handler)
    {
        EventHandler.UnregisterEvent(eventName, handler);
    }

    public static void UnregisterEvent<T, U, V>(object obj, string eventName, Action<T, U, V> handler)
    {
        EventHandler.UnregisterEvent(obj, eventName, handler);
    }

    public static void UnregisterEvent<T, U, V, W>(string eventName, Action<T, U, V, W> handler)
    {
        EventHandler.UnregisterEvent(eventName, handler);
    }

    public static void UnregisterEvent<T, U, V, W>(object obj, string eventName, Action<T, U, V, W> handler)
    {
        EventHandler.UnregisterEvent(obj, eventName, handler);
    }
}
