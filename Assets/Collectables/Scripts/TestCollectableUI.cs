using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static Unity.Burst.Intrinsics.X86.Avx;

public class TestCollectableUI : MonoBehaviour
{
    [SerializeField]
    public DungeonGenerator dungeonGenerator;

    [SerializeField]
    public GameObject player;

    private GameObject startRoom;

    private bool areCountersInitialized = false;

    GameObject cylinderCounter;
    GameObject cubeCounter;
    GameObject sphereCounter;

    private float gameTime = 0;
    
    // Start is called before the first frame update
    void Start()
    {

        GameMaster.gameState = GameMaster.GameState.PreStart;
        startRoom = dungeonGenerator.startRoom;


        GameMaster.cylinderCollected = 0;
        GameMaster.cubesCollected = 0;
        GameMaster.spheresCollected = 0;

   


    }

    private void DrawUICollectables()
    {
        GenerateCollectableUI(PrimitiveType.Cylinder, new Vector2(80, 80));
        GenerateCollectableUI(PrimitiveType.Cube, new Vector2(80+110, 80));
        GenerateCollectableUI(PrimitiveType.Sphere, new Vector2(80+110*2, 80));

    }

    private void GenerateCollectableUI(PrimitiveType ptype, Vector3 anchoredPosition)
    {
        List<Material> mList = new List<Material>();
        mList.Add(DungeonGenerator.CollectableMaterial);
        mList.Add(DungeonGenerator.CollectableOutline);

        GameObject collectable = GameObject.CreatePrimitive(ptype);
        collectable.transform.SetParent(transform.parent, false);
        collectable.transform.localPosition = new Vector3(0, 0, 0);
        collectable.transform.localScale = Vector3.one * 0.25f;
        collectable.GetComponent<Collider>().enabled = false;
        RectTransform rectTransform = collectable.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.pivot = Vector2.one * 0.5f;
        rectTransform.anchoredPosition = anchoredPosition;
        rectTransform.localPosition = rectTransform.localPosition + new Vector3(0, 0, 1) * 20f;
        rectTransform.localScale = Vector3.one * 20f;
        collectable.GetComponent<MeshRenderer>().SetMaterials(mList);
        collectable.layer = LayerMask.NameToLayer("UI");
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
        counterObj.GetComponent<RectTransform>().anchoredPosition = new Vector2(20f, 10f);

        tmp.fontMaterial = FontManager.MainFontMaterial;
        tmp.font = FontManager.TmpFontAssetMain;
        counterObj.transform.SetParent(transform.parent, false);
        return counterObj;
    }

    private void updateCounter(GameObject counterObj,string name, int curCount, int outOff)
    {
        TextMeshPro tmp = counterObj.GetComponent<TextMeshPro>();
        tmp.text = $"{name}: {curCount} / {outOff}";
    }




    // Update is called once per frame
    void Update()
    {
        
      



        if (GameMaster.gameState == GameMaster.GameState.Started)
        {


            gameTime += Time.deltaTime;
            GetComponent<TextMeshPro>().text = GetFormatedTime(gameTime);
            if (!areCountersInitialized)
            {


                cylinderCounter = createCounter("cylinders", GameMaster.cylinderCollected, GameMaster.numberOfCylinders);
                cubeCounter = createCounter("cubes", GameMaster.cubesCollected, GameMaster.numberOfCubes);
                sphereCounter = createCounter("spheres", GameMaster.spheresCollected, GameMaster.numberOfCubes);


                // do not use scale use font size

                cubeCounter.GetComponent<RectTransform>().anchoredPosition = cylinderCounter.GetComponent<RectTransform>().anchoredPosition + new Vector2(cylinderCounter.GetComponent<RectTransform>().rect.size.x * 5f + 30f, cylinderCounter.GetComponent<RectTransform>().rect.min.y);
                sphereCounter.GetComponent<RectTransform>().anchoredPosition = cubeCounter.GetComponent<RectTransform>().anchoredPosition + new Vector2(cubeCounter.GetComponent<RectTransform>().rect.size.x * 5f + 10f, cubeCounter.GetComponent<RectTransform>().rect.min.y);

                DrawUICollectables();


                areCountersInitialized = true;

            }
            else
            {

                updateCounter(cylinderCounter, "cylinder", GameMaster.cylinderCollected, GameMaster.numberOfCylinders);
                updateCounter(cubeCounter, "cube", GameMaster.cubesCollected, GameMaster.numberOfCubes);
                updateCounter(sphereCounter, "sphere", GameMaster.spheresCollected, GameMaster.numberOfSpheres);

            }

        }
        else if (GameMaster.gameState == GameMaster.GameState.Ended)


        {

            GetComponent<TextMeshPro>().text = $"Well Done All Shapes Collected! \n in {GetFormatedTime(gameTime)}";
        }


        if (GameMaster.cylinderCollected == 1 && GameMaster.cubesCollected == GameMaster.numberOfCubes && GameMaster.spheresCollected == GameMaster.numberOfSpheres



          )
        {
            GameMaster.gameState = GameMaster.GameState.Ended;
        }

    }

    private string GetFormatedTime(float time)
    {

        TimeSpan t = TimeSpan.FromSeconds(time);
        return $"<mspace=0.75em> {t.Minutes.ToString("d2")}:{t.Seconds.ToString("d2")}:{t.Milliseconds.ToString("d3")} </mspace>";

    }
}


