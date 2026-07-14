using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance;

    private Dictionary<string, Queue<GameObject>> poolDictionary =
        new Dictionary<string, Queue<GameObject>>();

    // Tracks objects that are CURRENTLY inside the pool.
    private HashSet<GameObject> pooledObjects =
        new HashSet<GameObject>();

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

        GameObject obj = null;

        // Keep checking until we find a valid pooled object.
        while (poolDictionary[key].Count > 0)
        {
            GameObject candidate =
                poolDictionary[key].Dequeue();

            if (candidate == null)
                continue;

            // If the queue contains a stale/duplicate reference,
            // skip it instead of spawning the same object again.
            if (!pooledObjects.Contains(candidate))
            {
                Debug.LogWarning(
                    $"POOL: Skipping stale duplicate reference " +
                    $"for '{candidate.name}'."
                );

                continue;
            }

            pooledObjects.Remove(candidate);

            obj = candidate;
            break;
        }

        if (obj == null)
        {
            obj = Instantiate(prefab);
            obj.name = prefab.name;
        }

        // Important: reset transform before activation if your
        // Tile.OnEnable() depends on transform/parent.
        obj.transform.SetParent(parent, false);

        obj.SetActive(true);

        return obj;
    }

    public void Despawn(GameObject obj)
    {
        if (obj == null)
            return;

        // Prevent the SAME object from entering the queue twice.
        if (pooledObjects.Contains(obj))
        {
            Debug.LogWarning(
                $"POOL: Tried to despawn '{obj.name}' twice. Ignoring duplicate."
            );
            return;
        }

        obj.transform.DOKill();

        obj.SetActive(false);
        obj.transform.SetParent(transform, false);

        string key = obj.name;

        if (!poolDictionary.ContainsKey(key))
        {
            poolDictionary[key] = new Queue<GameObject>();
        }

        pooledObjects.Add(obj);
        poolDictionary[key].Enqueue(obj);
    }

    public bool IsPooled(GameObject obj)
    {
        return obj != null && pooledObjects.Contains(obj);
    }
}