using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class WindowAsset : MonoBehaviour, IAssetPoolObject<WindowAsset> 
{

    private ObjectPool<WindowAsset> _windowPool;

    public void SetPool(ObjectPool<WindowAsset> windowPool)
    {
        _windowPool = windowPool;
    }
    
}
