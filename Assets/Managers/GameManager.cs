using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GameManager : MonoBehaviour 
{

    public static GameManager Instance;
    public enum GameState
    {
        PreStart,
        Started,
        Ended,
    }

    public GameState gameState = GameState.PreStart;

    public Dictionary<CollectableType, int> numCollected = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);
    public Dictionary<CollectableType, int> total = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);

  

    private void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            Instance = this;    
        }

        numCollected = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);
        total = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);

    }

    private void Start()
    {
        gameState = GameState.PreStart;
    }

    private void Update()
    {
        // find alternative way to compare
        if (numCollected.Values.SequenceEqual(total.Values))
        {
            // need to do this after UI update
            GameManager.Instance.gameState = GameManager.GameState.Ended;
        }
    }

    private void OnEnable()
    {
        MeshCollectable.OnMeshCollected += IncrementItemsCollected;
    }

    private void OnDisable()
    {
        MeshCollectable.OnMeshCollected -= IncrementItemsCollected;
    }

    public void IncrementItemsCollected(CollectableType t)
    {
        numCollected[t]++;
    }

}
