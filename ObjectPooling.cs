using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ObjectPool
{
    private Queue<GameObject> m_PooledObjects;
    private GameObject m_Prefab;

    public ObjectPool(GameObject objectToPool, int capacity)
    {
        m_PooledObjects = new Queue<GameObject>(capacity);
        m_Prefab = objectToPool;

        for (int i = 0; i < capacity; i++)
        {
            GameObject poolObject = GameObject.Instantiate(objectToPool);
            ReturnObject(poolObject, null);
        }
    }

    public GameObject GetObject(Vector3 position, Quaternion rotation, Transform parent)
    {
        GameObject objectToReturn;

        if (m_PooledObjects.Count > 0)
        {
            objectToReturn = m_PooledObjects.Dequeue();
        }
        else
        {
            Debug.LogWarning("Pool not large enough, instantiating new object");

            objectToReturn = GameObject.Instantiate(m_Prefab);
        }

        objectToReturn.transform.position = position;
        objectToReturn.transform.rotation = rotation;
        objectToReturn.transform.localScale = Vector3.one;
        objectToReturn.transform.parent = null;
        objectToReturn.SetActive(true);

        return objectToReturn;
    }

    public bool ReturnObject(GameObject objectToReturn, Transform parent)
    {
        objectToReturn.transform.parent = parent;
        objectToReturn.SetActive(false);
        m_PooledObjects.Enqueue(objectToReturn);

        return true;
    }
}

[Serializable]
public struct PoolData
{
    public GameObject GameObject;
    public int InitialPoolSize;
}

public class ObjectPooling : MonoBehaviour
{
    [SerializeField] private PoolData[] m_PoolsToCreate;

    private Dictionary<GameObject, ObjectPool> m_PoolMap;
    private Dictionary<ObjectPool, GameObject> m_ReturnMap;
    private Dictionary<GameObject, ObjectPool> m_LeasedObjectsMap;

    private void Start()
    {
        m_PoolMap = new Dictionary<GameObject, ObjectPool>(m_PoolsToCreate.Length);
        m_ReturnMap = new Dictionary<ObjectPool, GameObject>(m_PoolsToCreate.Length);
        m_LeasedObjectsMap = new Dictionary<GameObject, ObjectPool>();

        for (int i = 0; i < m_PoolsToCreate.Length; i++)
        {
            GameObject objectToPool = m_PoolsToCreate[i].GameObject;

            m_PoolMap.Add(objectToPool, new ObjectPool(objectToPool, m_PoolsToCreate[i].InitialPoolSize));
            m_ReturnMap.Add(m_PoolMap[objectToPool], objectToPool);
        }
    }

    public GameObject GetObject(GameObject objectToGet, Vector3 position, Quaternion rotation, Transform parent = null)
    {
        ObjectPool objectPool = m_PoolMap[objectToGet];

        GameObject objectOut = objectPool.GetObject(position, rotation, parent);
        m_LeasedObjectsMap.Add(objectOut, objectPool);

        return objectOut;
    }

    public bool ReturnObject(GameObject objectToReturn, Transform parent = null)
    {
        ObjectPool objectPool = m_LeasedObjectsMap[objectToReturn];
        GameObject prefab = m_ReturnMap[objectPool];

        m_LeasedObjectsMap.Remove(objectToReturn);
        m_PoolMap[prefab].ReturnObject(objectToReturn, parent);

        return true;
    }
}
