using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
[ExecuteInEditMode]
public class WindowSpawner : MonoBehaviour
{

    //public ObjectPool<WindowAsset> _windowPool;
    //public List<WindowAsset> _currentInstances;
    ////public List<WindowAsset> _prevInstances;
    public GameObject _window;
    //int curGenerationInstanceIndex = 0;

    // Start is called before the first frame update

   public GenericAssetPool<WindowAsset> _windowPool;

    void OnEnable()
    {
        _windowPool = new GenericAssetPool<WindowAsset>(_window, this.gameObject);

        //_windowPool = new ObjectPool<WindowAsset>(CreateWindow, OnTakeWindowFromPool, OnReturnWindowToPool, OnWindowDestroy, true, 1000, 2000);
        //_currentInstances = new List<WindowAsset>();
    }

    //public void ResetPrevInstances()
    //{
    //    if (_currentInstances.Count > 0)
    //    {
    //        for (int i = _currentInstances.Count - 1; i >= 0; i--)
    //        {

    //            _windowPool.Release(_currentInstances[i]);

    //        }
    //    }

    //}

    //private WindowAsset CreateWindow()
    //{
    //    GameObject window = GameObject.Instantiate(_window);

    //    window.AddComponent<WindowAsset>().SetPool(_windowPool);
    //    return window.GetComponent<WindowAsset>();
    //}


    //private void OnTakeWindowFromPool(WindowAsset window)
    //{
    //    window.gameObject.SetActive(true);
    //    _currentInstances.Add(window);
    //}

    //private void OnReturnWindowToPool(WindowAsset window)
    //{
    //    window.transform.parent = transform;
    //    window.gameObject.SetActive(false);
    //    _currentInstances.Remove(window);
    //}

    //private void OnWindowDestroy(WindowAsset window)
    //{
    //    _currentInstances.Remove(window);
    //    DestroyImmediate(window.gameObject);
    //}

    //public void newInstances()
    //{
    //    //_prevInstances = new List<WindowAsset>(_currentInstances);
    //    //_currentInstances = new List<WindowAsset>();
    //    curGenerationInstanceIndex = _currentInstances.Count - 1;
    //}
}
