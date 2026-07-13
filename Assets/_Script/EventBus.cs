using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<GameEventType, Delegate> list = new Dictionary<GameEventType, Delegate>();

    public static void AddListener(GameEventType key, Action value)
    {
        list[key] = Delegate.Combine(list.TryGetValue(key, out var del) ? del : null, value);
    }

    public static void AddListener<T>(GameEventType key, Action<T> value)
    {
        list[key] = Delegate.Combine(list.TryGetValue(key, out var del) ? del : null, value);
    }

    public static void AddListener<T1, T2>(GameEventType key, Action<T1, T2> value)
    {
        list[key] = Delegate.Combine(list.TryGetValue(key, out var del) ? del : null, value);
    }

    public static void Notify(GameEventType key)
    {
        if (!list.TryGetValue(key, out var del) || del == null) return;
        
        var invocationList = del.GetInvocationList();
        foreach (var callback in invocationList)
        {
            if (callback is Action action)
            {
                try
                {
                    action();
                }
                catch (Exception e)
                {
                    Debug.LogError($"Invoke action fail for event {key}! error: {e}");
                }
            }
        }
    }

    public static void Notify<T>(GameEventType key, T data)
    {
        if (!list.TryGetValue(key, out var del) || del == null) return;
        
        var invocationList = del.GetInvocationList();
        foreach (var callback in invocationList)
        {
            if (callback is Action<T> action)
            {
                try
                {
                    action(data);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Invoke action fail for event {key} with parameter {typeof(T)}! error: {e}");
                }
            }
        }
    }

    public static void Notify<T1, T2>(GameEventType key, T1 data1, T2 data2)
    {
        if (!list.TryGetValue(key, out var del) || del == null) return;
        
        var invocationList = del.GetInvocationList();
        foreach (var callback in invocationList)
        {
            if (callback is Action<T1, T2> action)
            {
                try
                {
                    action(data1, data2);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Invoke action fail for event {key} with parameters {typeof(T1)}, {typeof(T2)}! error: {e}");
                }
            }
        }
    }

    public static void RemoveListener(GameEventType key, Action value)
    {
        if (list.TryGetValue(key, out var del))
        {
            var newDel = Delegate.Remove(del, value);
            if (newDel == null) list.Remove(key);
            else list[key] = newDel;
        }
    }

    public static void RemoveListener<T>(GameEventType key, Action<T> value)
    {
        if (list.TryGetValue(key, out var del))
        {
            var newDel = Delegate.Remove(del, value);
            if (newDel == null) list.Remove(key);
            else list[key] = newDel;
        }
    }

    public static void RemoveListener<T1, T2>(GameEventType key, Action<T1, T2> value)
    {
        if (list.TryGetValue(key, out var del))
        {
            var newDel = Delegate.Remove(del, value);
            if (newDel == null) list.Remove(key);
            else list[key] = newDel;
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        list.Clear();
    }
}