using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;

public class WindowAsset : MonoBehaviour
{

    private ObjectPool<WindowAsset> _windowPool;

    public void setPool(ObjectPool<WindowAsset> windowPool)
    {
        _windowPool = windowPool;
    }
    
}
