using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]


public class SnapToGrid : MonoBehaviour
{
    private void Update()
    {

        if (Application.isPlaying == false)
        {
            transform.position = Vector3Int.FloorToInt(transform.position);
        }
    }

}
