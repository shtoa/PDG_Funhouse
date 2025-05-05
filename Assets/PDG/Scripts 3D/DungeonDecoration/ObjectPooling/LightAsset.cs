using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class LightAsset : MonoBehaviour, IAssetPoolObject<LightAsset>
{
    private ObjectPool<LightAsset> _lightPool;

    public void SetPool(ObjectPool<LightAsset> lightPool)
    {
        _lightPool = lightPool;
    }
}
