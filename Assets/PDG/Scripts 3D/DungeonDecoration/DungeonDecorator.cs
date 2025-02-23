using System.Collections;
using System.Collections.Generic;
using UnityEditor.Animations.Rigging;
using UnityEngine;

public class DungeonDecorator : MonoBehaviour
{
    [Header("Wall Properties")]
    public Material wallMaterial;

    [Header("Start Room")]
    public Material StartRoomMat;

    [Header("End Room")]
    public Material EndRoomMat;

    [Header("Door")]
    public Material DoorMat;
  
    [Header("Floor Properties")]
    public Material floorMaterial;

    [Header("Ceiling Properties")]
    public Material ceilingMaterial;


    [Header("Room Styles")]
    [SerializeField]
    public List<RoomStyle> roomStyles;


    [Header("Corridor Styles")]
    [SerializeField]
    public List<RoomStyle> corridorStyles;

    [Header("Roof")]
    public GameObject roofObject;

    [Header("CornerBlock")]
    public GameObject cornerObject;

    [Header("WindowMesh")]
    public GameObject windowMesh;

    [Header("LightMesh")]
    public GameObject lightMesh;


}
