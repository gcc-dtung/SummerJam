using System;
using System.Collections.Generic;
using UnityEngine;

public static class EventBus
{
    private static readonly Dictionary<GameEventType, List<Delegate>> listeners =
        new Dictionary<GameEventType, List<Delegate>>();

    public static void AddListener(GameEventType key, Action value)
    {
        AddListenerInternal(key, value);
    }

    public static void AddListener<T>(GameEventType key, Action<T> value)
    {
        AddListenerInternal(key, value);
    }

    public static void AddListener<T1, T2>(GameEventType key, Action<T1, T2> value)
    {
        AddListenerInternal(key, value);
    }

    public static void Notify(GameEventType key)
    {
        if (!TryGetSnapshot(key, out Delegate[] callbacks)) return;
        
        foreach (Delegate callback in callbacks)
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
        if (!TryGetSnapshot(key, out Delegate[] callbacks)) return;
        
        foreach (Delegate callback in callbacks)
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
        if (!TryGetSnapshot(key, out Delegate[] callbacks)) return;
        
        foreach (Delegate callback in callbacks)
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
        RemoveListenerInternal(key, value);
    }

    public static void RemoveListener<T>(GameEventType key, Action<T> value)
    {
        RemoveListenerInternal(key, value);
    }

    public static void RemoveListener<T1, T2>(GameEventType key, Action<T1, T2> value)
    {
        RemoveListenerInternal(key, value);
    }

    private static void AddListenerInternal(GameEventType key, Delegate listener)
    {
        if (listener == null)
            return;

        if (!listeners.TryGetValue(key, out List<Delegate> callbacks))
        {
            callbacks = new List<Delegate>();
            listeners.Add(key, callbacks);
        }

        callbacks.Add(listener);
    }

    private static void RemoveListenerInternal(GameEventType key, Delegate listener)
    {
        if (listener == null || !listeners.TryGetValue(key, out List<Delegate> callbacks))
            return;

        for (int i = callbacks.Count - 1; i >= 0; i--)
        {
            if (!callbacks[i].Equals(listener))
                continue;

            callbacks.RemoveAt(i);
            break;
        }

        if (callbacks.Count == 0)
            listeners.Remove(key);
    }

    private static bool TryGetSnapshot(GameEventType key, out Delegate[] callbacks)
    {
        if (!listeners.TryGetValue(key, out List<Delegate> registered) || registered.Count == 0)
        {
            callbacks = null;
            return false;
        }

        callbacks = registered.ToArray();
        return true;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    public static void Clear()
    {
        listeners.Clear();
    }
}
