using Autodesk.Fbx;
using dungeonGenerator;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class DoorInteraction : MonoBehaviour, IInteractable
{

    private DoorState doorState = DoorState.closed;
    private Vector3 closedPosition = Vector3.zero;
    private Vector3 prevPosition = Vector3.zero;


    private AnimationCurve ezEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    GameObject helperTextObj;
    //bool isHovering = false;

    public enum DoorState
    {
        open, 
        closed,
        opening,
        closing
    }

    private float animationTimer = 0;
    public void Interact()
    {
        if (doorState == DoorState.closed)
        {
            doorState = DoorState.opening;
            animationTimer = 0;

        } else if (doorState == DoorState.open) {


            doorState = DoorState.closing;
            animationTimer = 0;

        }

    }

    void Start()
    {
        closedPosition = transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        //isHovering = false;

        if (doorState == DoorState.closing)
        {
            //transform.position = closedPosition;


            CloseDoor();

        }
        else if (doorState == DoorState.opening)
        {
            OpenDoor();

        }
        else if (doorState == DoorState.open)
        {
            CloseDoorAfterOpen(2f);
        }


    }

    private void CloseDoorAfterOpen(float secondsToWait)
    {
        animationTimer += Time.deltaTime;
        if(animationTimer > secondsToWait)
        {
            doorState = DoorState.closing;
            animationTimer = 0;
        }
    }

    private void CloseDoor()
    {
        if (animationTimer < 1f)
        {
            animationTimer += Time.deltaTime*2f;
            transform.position = Vector3.Lerp(closedPosition + transform.up * 1f, closedPosition, ezEase.Evaluate(animationTimer));
            prevPosition = transform.position;
        }
        else
        {
            doorState = DoorState.closed;
        }
    }

    private void OpenDoor()
    {
        if (animationTimer < 1f)
        {
            animationTimer += Time.deltaTime;
            transform.position = Vector3.Lerp(closedPosition, closedPosition + transform.up * 1f, ezEase.Evaluate(animationTimer));
            prevPosition = transform.position;
        }
        else
        {
            doorState = DoorState.open;
        }
    }

    public void OnEnter()
    {
        GetComponent<MeshRenderer>().material.SetInt("_isSelected", 1);
        //isHovering = true;

    }

    public void OnHover()
    {
        //Debug.Log("Hovering");

        if (transform.childCount == 0)
        {
            //AddHelperText("Press F to Interact with Door");
        }

        //helperTextObj.transform.LookAt(Camera.main.transform.position);
    }

    private void AddHelperText(string helperText)
    {
        helperTextObj = new GameObject();
        TextMeshPro tmp = helperTextObj.AddComponent<TextMeshPro>();
        tmp.text = helperText;
        tmp.fontSize = 12;
        tmp.alignment = TextAlignmentOptions.Center;
        tmp.verticalAlignment = VerticalAlignmentOptions.Middle;
        tmp.fontMaterial = FontManager.MainFontMaterial;
        tmp.font = FontManager.TmpFontAssetMain;
        

        RectTransform rect = helperTextObj.GetComponent<RectTransform>();
        helperTextObj.transform.SetParent(transform, false);
        rect.localScale = Vector3.one*0.1f;
        rect.localPosition = Vector3.zero + Vector3.forward*-1;

    }

    public void OnExit()
    {
        GetComponent<MeshRenderer>().material.SetInt("_isSelected", 0);
        //isHovering = true;

        //Destroy(helperTextObj);

        //Debug.Log("Exited");
    }
}
