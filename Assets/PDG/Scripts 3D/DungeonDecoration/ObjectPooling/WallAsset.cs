using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WallAsset : MonoBehaviour, IAssetPoolObject<WallAsset>
{
    private ObjectPool<WallAsset> _wallPool;

    public void SetPool(ObjectPool<WallAsset> wallPool)
    {
        _wallPool = wallPool;
    }
}
