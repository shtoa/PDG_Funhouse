using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public class WallSpawner : MonoBehaviour
{
    public ObjectPool<WallAsset> _wallPool;
    public List<WallAsset> _currentInstances;
    //public List<WallAsset> _prevInstances;
    int curGenerationInstanceIndex = 0;
    public GameObject _wall;

    // Start is called before the first frame update
    void Start()
    {
        _wallPool = new ObjectPool<WallAsset>(CreateWall, OnTakeWallFromPool, OnReturnWallToPool, OnWallDestroy, true, 1000, 2000);
        _currentInstances = new List<WallAsset>();
    }

    public void ResetPrevInstances()
    {
        for (int i = _currentInstances.Count - 1; i >= 0; i--)
        {

            _wallPool.Release(_currentInstances[i]);
            
        }

    }

    private WallAsset CreateWall()
    {
        GameObject wall = MeshHelper.CreateCuboid(new Vector3Int(1,1,1), 1);

        wall.AddComponent<WallAsset>().setPool(_wallPool);
        return wall.GetComponent<WallAsset>();
    }


    private void OnTakeWallFromPool(WallAsset wall)
    {
        wall.gameObject.SetActive(true);
        _currentInstances.Add(wall);
    }

    private void OnReturnWallToPool(WallAsset wall)
    {
        wall.transform.parent = transform;
        wall.gameObject.SetActive(false);
        _currentInstances.Remove(wall);
    }

    private void OnWallDestroy(WallAsset wall)
    {
        _currentInstances.Remove(wall);
        Destroy(wall.gameObject);
    }

    public void newInstances()
    {
        //_prevInstances = new List<WallAsset>(_currentInstances);
        //_currentInstances = new List<WallAsset>();
        curGenerationInstanceIndex = _currentInstances.Count - 1;
    }
}
