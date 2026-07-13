using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EventBus :  Singleton<EventBus>
{
    Dictionary<string, List<Action>> list = new Dictionary<string, List<Action>>();
    public void AddListener(string key, Action value)
    {
        if (!list.ContainsKey(key))
        {
            list.Add(key, new List<Action>());
        }
        list[key].Add(value);
    }
    public void Notify(string key)
    {
        if(!list.ContainsKey(key)) {Debug.LogError("Not have that key"); return;}
        foreach(var it in list[key])
        {
            try
            {
                it?.Invoke();
            }
            catch(Exception e)
            {
                Debug.LogError("Invoke action fail! error: "+e.ToString());
            }
        }
    }
    public void RemoveListener(string key,Action value)
    {
        if (!list.ContainsKey(key))
        {
            list.Add(key,new List<Action>());
        }
        list[key].Remove(value);
    }
}