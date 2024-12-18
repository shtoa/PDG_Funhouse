using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.Build;
using UnityEngine;


[InitializeOnLoadAttribute]
[ExecuteAlways]
public class GameManager : MonoBehaviour 
{

    public static GameManager Instance {
        /// <summary>
        /// singleton logic from: <see href="https://stackoverflow.com/a/67717318"> stackOverflow: derHugo </see>
        /// </summary>
        get
        {
            // if instance exists return
            if(instance) return instance; 


            // if object exists in scene return object
            instance = FindObjectOfType<GameManager>();
            if (instance) return instance;

            // if object doesnt exist return new object
            instance = new GameObject("GameManager").AddComponent<GameManager>();
            return instance;
        }
    }

    private static GameManager instance;
    
    public enum GameState
    {
        PreStart,
        Started,
        Ended,
    }




    public GameState gameState = GameState.PreStart;

    public Dictionary<CollectableType, int> numCollected = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);
    public Dictionary<CollectableType, int> total = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);


    public void OnAfterAssemblyReload()
    {

        Debug.Log("AWAKENED");

        if(instance != null && instance != this)
        {
            Destroy(this.gameObject);
        } else
        {
            instance = this;    
        }

        numCollected = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);
        total = Enum.GetValues(typeof(CollectableType)).Cast<CollectableType>().ToDictionary(t => t, t => 0);

        gameState = GameState.PreStart;
    }

    //private void Start()
    //{
       
    //}

    private void Update()
    {
        // find alternative way to compare
        if (Application.isPlaying)
        {
            if (numCollected.Values.SequenceEqual(total.Values))
            {
                // need to do this after UI update
                GameManager.instance.gameState = GameManager.GameState.Ended;
            }
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
