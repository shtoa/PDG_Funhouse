using Autodesk.Fbx;
using dungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class DoorInteraction : MonoBehaviour, IInteractable
{

    private DoorState doorState = DoorState.closed;
    private Vector3 closedPosition = Vector3.zero;
    private Vector3 prevPosition = Vector3.zero;
    

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

        if (doorState == DoorState.closing)
        {
            transform.position = closedPosition;
            doorState = DoorState.closed;

        } else if (doorState == DoorState.opening)
        {

            //if (animationTimer < 1f)
            //{
            //    animationTimer += 0.01f;
            //    transform.position = Vector3.Lerp(closedPosition, closedPosition + Vector3.forward * transform.localScale.z, animationTimer);
            //    prevPosition = transform.position;
            //} else if (animationTimer < 2f)
            //{
            //    animationTimer += 0.005f;
            //    transform.position = Vector3.Lerp(prevPosition, prevPosition + Vector3.left * transform.localScale.x, animationTimer-1f);
            //} else
            //{
            //    doorState = DoorState.open;
            //}

            transform.position = closedPosition + transform.up * 1f;
            doorState = DoorState.open;

         
        }


    }
}
