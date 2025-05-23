using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using static Unity.Burst.Intrinsics.X86.Avx;
using static UnityEditor.Rendering.CameraUI;

public class TestCollectableUI : MonoBehaviour
{

    Dictionary<CollectableType, GameObject> Counters = new Dictionary<CollectableType, GameObject>();

    private bool areCountersInitialized = false;
    private float gameTime = 0;
    

    private void Update()
    {

        if (GameManager.Instance.gameState == GameManager.GameState.Started)
        {
            gameTime += Time.deltaTime;
            GetComponent<TextMeshPro>().text = GetFormatedTime(gameTime);
            if (!areCountersInitialized)
            {
                CreateCounters();
                CreateUICollectables();
                areCountersInitialized = true;

            }
            else
            {
                UpdateCounters();
            }
        }
        else if (GameManager.Instance.gameState == GameManager.GameState.Ended)

        {
            UpdateCounters();
            SetGameEndText();
        }

    }

    private void SetGameEndText()
    {
        GetComponent<TextMeshPro>().text = $"{GetFormatedTime(gameTime)} \n Well Done All Shapes Collected!";
    }

    private void UpdateCounters()
    {
        foreach (CollectableType collectableType in Enum.GetValues(typeof(CollectableType)))
        {
            updateCounter(Counters[collectableType], collectableType);
        }
    }

    // refactor create counbtes and draw UI Collectables
    private void CreateCounters()
    {
        foreach (CollectableType collectableType in Enum.GetValues(typeof(CollectableType)))
        {
            Counters.Add(collectableType, CreateCounter(collectableType.ToString(), new Vector2(20f+(int)collectableType * 100f, 10f)));
        }

    }

    private void CreateUICollectables()
    {

        foreach (CollectableType collectableType in Enum.GetValues(typeof(CollectableType)))
        {
            CreateUICollectable(collectableType);
        }

    }

    private void CreateUICollectable(CollectableType collectableType)
    {

        GameObject g = MeshCollectableCreator.Instance.GenerateCollectable(collectableType, Counters[collectableType].transform);
        g.transform.localScale = new Vector3(5, 5, 5);


        // need to make the width of the counters the same
        g.transform.localPosition = new Vector3(4.5f, 20, 3);
        g.layer = LayerMask.NameToLayer("UI");
        g.GetComponent<Collider>().enabled = false; // change this to make it more scalable

    }

    private GameObject CreateCounter(string name, Vector2 anchorPos)
    {
        GameObject counterObj = new GameObject(name + "Counter");
        counterObj.layer = LayerMask.NameToLayer("UI");

        counterObj.transform.localScale = Vector3.one * 5f;        
        
        // Add text object to the componenet 
        TextMeshPro tmp = counterObj.AddComponent<TextMeshPro>();
        tmp.autoSizeTextContainer = true;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.text = $"{name}:";

        // change the transform on the UI Screen 
        RectTransform rectTransform = counterObj.GetComponent<RectTransform>();

        rectTransform.pivot = Vector3.zero;
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.zero;
        rectTransform.anchoredPosition = anchorPos;
  

        tmp.fontMaterial = FontManager.MainFontMaterial;
        tmp.font = FontManager.TmpFontAssetMain;
        counterObj.transform.SetParent(transform.parent, false);

     

        Canvas.ForceUpdateCanvases();

        rectTransform.sizeDelta = Vector2.one * 10f;

        return counterObj;
    }

    private void updateCounter(GameObject counterObj, CollectableType collectableType)
    {
        TextMeshPro tmp = counterObj.GetComponent<TextMeshPro>();
        tmp.text = $"{collectableType.ToString()} \n {GameManager.Instance.numCollected[collectableType]} / {GameManager.Instance.total[collectableType]}";
        tmp.fontSize = 18;
        tmp.alignment = TextAlignmentOptions.Center;
    }

    private string GetFormatedTime(float time)
    {
        TimeSpan t = TimeSpan.FromSeconds(time);
        return $"<mspace=0.75em> {t.Minutes.ToString("d2")}:{t.Seconds.ToString("d2")}:{t.Milliseconds.ToString("d3")} </mspace>";
    }

}


