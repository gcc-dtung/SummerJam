using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

public class JsonDataService : IDataService
{
    public bool SaveData<T>(string RelativePath, T data, bool Encrypted)
    {
        string path = Application.persistentDataPath + RelativePath;
        try
        {
            if (File.Exists(path))
            {
                Debug.Log("Data exists. Deleting old file and writing a new one!");
                File.Delete(path);
            }
            else
            {
                Debug.Log("Writing for the first time!");
            }

            using FileStream stream = File.Create(path);
            stream.Close();
            File.WriteAllText(path, JsonConvert.SerializeObject(data));
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Unable to save data due to: {e.Message} {e.StackTrace}");
            return false;
        }
    }

    public T LoadData<T>(string RelativePath, bool Encrypted)
    {
        string path = Application.persistentDataPath + RelativePath;
        if (!File.Exists(path))
        {
            Debug.LogError($"Can't load file at {path}. File doesn't exist!");
            throw new FileNotFoundException($"{path} doesn't exist!");
        }

        try
        {
            T Data = JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
            return Data;
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to load data due to: {e.Message} {e.StackTrace}");
            throw e;
        }
    }
}