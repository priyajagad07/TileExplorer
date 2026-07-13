using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    // Dictionary to hold queues of our inactive game objects, keyed by the prefab's name
    private Dictionary<string, Queue<GameObject>> poolDictionary = new Dictionary<string, Queue<GameObject>>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject Spawn(GameObject prefab, Transform parent)
    {
        string key = prefab.name;

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
        }

        if (poolDictionary[key].Count > 0)
        {
            GameObject obj = poolDictionary[key].Dequeue();
            obj.SetActive(true);

            // THE FIX: Adding 'false' tells Unity to keep local UI scaling intact!
            obj.transform.SetParent(parent, false);
            return obj;
        }
        else
        {
            GameObject newObj = Instantiate(prefab, parent);
            newObj.name = prefab.name;
            return newObj;
        }
    }

    public void Despawn(GameObject obj)
    {
        obj.SetActive(false);

        // THE FIX: Move the tile OUT of the tileParent so game logic ignores it!
        // Adding 'false' ensures the UI RectTransform does not break.
        obj.transform.SetParent(this.transform, false);

        if (!poolDictionary.ContainsKey(obj.name))
        {
            poolDictionary[obj.name] = new Queue<GameObject>();
        }

        poolDictionary[obj.name].Enqueue(obj);
    }
}