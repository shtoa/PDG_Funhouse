using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WallAsset : MonoBehaviour
{
    private ObjectPool<WallAsset> _wallPool;

    public void setPool(ObjectPool<WallAsset> wallPool)
    {
        _wallPool = wallPool;
    }
}
