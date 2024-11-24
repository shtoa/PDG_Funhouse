using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class TestCollectableUI : MonoBehaviour
{
    [SerializeField]
    public DungeonGenerator dungeonGenerator;

    [SerializeField]
    public GameObject player;

    private GameObject startRoom;

    GameObject cylinderCounter;
    GameObject cubeCounter;
    GameObject sphereCounter;
    
    // Start is called before the first frame update
    void Start()
    {

        GameMaster.gameState = GameMaster.GameState.PreStart;
        startRoom = dungeonGenerator.startRoom;


        GameMaster.cylinderCollected = 0;
        GameMaster.cubesCollected = 0;
        GameMaster.spheresCollected = 0;


        cylinderCounter = createCounter("cylinders", GameMaster.cylinderCollected, GameMaster.numberOfCylinders);
        cubeCounter = createCounter("cubes", GameMaster.cubesCollected, GameMaster.numberOfCubes);
        sphereCounter = createCounter("spheres", GameMaster.spheresCollected, GameMaster.spheresCollected);

        cubeCounter.GetComponent<RectTransform>().localPosition = cubeCounter.GetComponent<RectTransform>().localPosition + new Vector3(150, 0, 0);
        sphereCounter.transform.localPosition = cubeCounter.transform.localPosition + new Vector3(130, 0, 0);

    }

    private GameObject createCounter(string name, int curCount, int outOff)
    {
        GameObject counterObj = new GameObject(name + "Counter");
        counterObj.AddComponent<TextMeshPro>();
        TextMeshPro tmp = counterObj.GetComponent<TextMeshPro>();
        counterObj.transform.localScale = Vector3.one * 5f;
        tmp.autoSizeTextContainer = true;
        tmp.alignment = TextAlignmentOptions.Left;
        tmp.text = $"{name}: {curCount} / {outOff}";

        
        counterObj.GetComponent<RectTransform>().pivot = Vector3.zero;
        counterObj.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        counterObj.GetComponent<RectTransform>().anchorMax = Vector2.zero;

        tmp.fontMaterial = FontManager.MainFontMaterial;
        tmp.font = FontManager.TmpFontAssetMain;
        counterObj.transform.SetParent(transform.parent, false);
        counterObj.GetComponent<RectTransform>().localPosition = new Vector3(-transform.parent.GetComponent<CanvasScaler>().referenceResolution.x/2f+10f, 0, 0);
        return counterObj;
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


