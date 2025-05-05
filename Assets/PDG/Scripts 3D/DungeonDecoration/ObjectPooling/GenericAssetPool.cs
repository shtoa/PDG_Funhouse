using dungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class GenericAssetPool<T> : IPoolManager where T : MonoBehaviour, IAssetPoolObject<T>
{
    [SerializeField]
    public ObjectPool<T> _pool;

    [SerializeField]
    public List<T> _currentInstances;

    public GameObject _asset;
    public GameObject _poolParent;

    // Create Asset Pool in Constructor
    public GenericAssetPool(GameObject asset, GameObject poolParent)
    {
        _asset = asset;
        _pool = new ObjectPool<T>(CreateAsset, OnTakeAssetFromPool, OnReturnAssetToPool, OnAssetDestroy, true, 1000, 2000);
        _currentInstances = new List<T>();
        _poolParent = poolParent;
    }

    public void ResetPrevInstances()
    {
        for (int i = _currentInstances.Count - 1; i >= 0; i--){

            _pool.Release(_currentInstances[i]);
        }
    }

    private T CreateAsset() 
    {
        GameObject asset = GameObject.Instantiate(_asset);

        asset.AddComponent<T>().SetPool(_pool);
        return asset.GetComponent<T>();
    }


    private void OnTakeAssetFromPool(T asset)
    {
        asset.gameObject.SetActive(true);
        _currentInstances.Add(asset);
    }

    private void OnReturnAssetToPool(T asset)
    {
        asset.transform.parent = this._poolParent.transform;
        asset.gameObject.SetActive(false);
        _currentInstances.Remove(asset);
    }

    private void OnAssetDestroy(T asset)
    {
        _currentInstances.Remove(asset);
        Object.DestroyImmediate(asset.gameObject);
    }

}

