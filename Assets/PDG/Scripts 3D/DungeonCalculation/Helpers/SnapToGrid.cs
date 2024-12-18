using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

[ExecuteAlways]


public class SnapToGrid : MonoBehaviour
{
    private void Update()
    {
        // if in editor mode snapp to grid
        if (Application.isPlaying == false)
        {
            // snap to the closest lowest int
            transform.position = Vector3Int.FloorToInt(transform.position);
        }
    }

}
