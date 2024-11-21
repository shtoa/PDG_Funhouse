using dungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloorTriggers : MonoBehaviour
{
    // Start is called before the first frame update

    public dungeonGenerator.RoomType roomType;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("COLLIDING");
        if(other.gameObject.tag == "Player")
        {
            if(roomType == RoomType.Start)
            {
                GameMaster.gameState = GameMaster.GameState.Started;
            }
        }
    }


}
