using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class TestCollectableUI : MonoBehaviour
{
    [SerializeField]
    public DungeonGenerator dungeonGenerator;

    [SerializeField]
    public GameObject player;

    private GameObject startRoom;
  
   
    
    // Start is called before the first frame update
    void Start()
    {

        GameMaster.gameState = GameMaster.GameState.PreStart;
        startRoom = dungeonGenerator.startRoom;


        GameMaster.cylinderCollected = 0;
        GameMaster.cubesCollected = 0;
        GameMaster.spheresCollected = 0;

    }

    // Update is called once per frame
    void Update()
    {
        
        if(GameMaster.cylinderCollected == 1 && GameMaster.cubesCollected == GameMaster.numberOfCubes && GameMaster.spheresCollected == GameMaster.numberOfSpheres



            )
        {
            GameMaster.gameState = GameMaster.GameState.Ended;
        }



        if (GameMaster.gameState == GameMaster.GameState.Started)
        {
            string text = $"Collect the Shapes \n - Cylinders: {GameMaster.cylinderCollected}/1 \n - Cubes: {GameMaster.cubesCollected}/{GameMaster.numberOfCubes} \n";
            if (GameMaster.numberOfSpheres > 0)
            {
                text = text + $" - Spheres: {GameMaster.spheresCollected}/{GameMaster.numberOfSpheres} \n";

               
                
            }

            GetComponent<TextMeshPro>().text = text;

        }
        else if (GameMaster.gameState == GameMaster.GameState.Ended)
        {
            GetComponent<TextMeshPro>().text = "Well Done All Shapes Collected!";
        }
        }
}


