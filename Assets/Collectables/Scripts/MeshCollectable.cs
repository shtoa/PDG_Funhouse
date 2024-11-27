using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class MeshCollectable : Collectable
{

    public CollectableType collectableType;
    public GameObject collectableInstance; 

    public static event Action<CollectableType> OnMeshCollected;

    private void Start()
    {
        collectableInstance = GetComponent<GameObject>();
    }

    public override void Collect()
    {
        Destroy(gameObject);
        OnMeshCollected?.Invoke(collectableType);
         
    }
}
