using dungeonGenerator;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static dungeonGenerator.DungeonStatTrack;

public class RoomVisitChecker : MonoBehaviour
{
    public int RoomID;
    private void OnTriggerEnter(Collider other)
    {
        DungeonStatTrack.trackedPerformanceStats.roomIDsVisited.Add(RoomID);
        DungeonStatTrack.trackedPerformanceStats.timeStampsVisited.Add(DungeonStatTrack.GameTime); // new Dictionary<string, int>(){ { "RoomID", RoomID }, { "Time", 0 } }
    }
}
