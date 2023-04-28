using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScriptableObjectDB<T> : MonoBehaviour where T : ScriptableObject
{
    private static Dictionary<string, T> _objects;
    
    public static void Init()
    {
        _objects = new Dictionary<string, T>();

        var objectArray = Resources.LoadAll<T>("");

        foreach (var obj in objectArray)
        {
            if(_objects.ContainsKey(obj.name))
            {
                Debug.Log($"There are 2 pokemon with the name {obj.name}");
                continue;
            }

            _objects[obj.name] = obj;
        }
    }
    
    public static T GetObjectByName(string name)
    {
        if (!_objects.ContainsKey(name))
        {
            Debug.LogError($"Pokemon with name {name} not found in the database");
            return null;
        }
        
        return _objects[name];
    }
}
