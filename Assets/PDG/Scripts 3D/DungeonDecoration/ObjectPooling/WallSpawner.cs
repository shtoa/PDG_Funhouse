using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
[ExecuteInEditMode]
public class WallSpawner : MonoBehaviour
{
    [Serializable]
    public class WallAssetPool : GenericAssetPool<WallAsset>
    {
        public WallAssetPool(GameObject asset, GameObject poolParent) : base(asset, poolParent)
        {
        }
    }

    public GameObject _wall;
    public WallAssetPool _wallPool;

    // Start is called before the first frame update
    void OnEnable()
    {
        _wallPool = new WallAssetPool(_wall,this.gameObject);
    }
}
