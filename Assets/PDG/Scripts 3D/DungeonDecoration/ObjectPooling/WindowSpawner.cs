using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
[ExecuteInEditMode]
public class WindowSpawner : MonoBehaviour
{
    public GameObject _window;

    [Serializable]
    public class WindowAssetPool : GenericAssetPool<WindowAsset>
    {
        public WindowAssetPool(GameObject asset, GameObject poolParent) : base(asset, poolParent)
        {
        }
    }

    public WindowAssetPool _windowPool;

    void OnEnable()
    {
        _windowPool = new WindowAssetPool(_window, this.gameObject);
    }

}
