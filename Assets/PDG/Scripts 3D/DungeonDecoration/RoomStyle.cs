using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RoomStyle : MonoBehaviour
{

    [Serializable]
    public struct materials
    {
        [SerializeField] public Material ceiling;
        [SerializeField] public Material floor;
        [SerializeField] public Material wall;
        [SerializeField] public Material ladder;

    }

    [SerializeField]
    public materials roomMaterials;

}
