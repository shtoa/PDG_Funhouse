using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
[ExecuteInEditMode]
public class LightSpawner : MonoBehaviour
{
    [Serializable]
    public class LightAssetPool : GenericAssetPool<LightAsset>
    {
        public LightAssetPool(GameObject asset, GameObject poolParent) : base(asset, poolParent)
        {
        }
    }

    public GameObject _light;
    public LightAssetPool _lightPool;

    // Start is called before the first frame update
    void OnEnable()
    {
        _lightPool = new LightAssetPool(_light, this.gameObject);
    }
}
