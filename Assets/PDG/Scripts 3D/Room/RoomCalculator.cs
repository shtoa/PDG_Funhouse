using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using dungeonGenerator;
using System;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{
    public class RoomCalculator
    {
        private Vector2Int minRoomBounds;
        private Vector2Int totalRoomOffset;
        private float corridorWidth;

        public RoomCalculator(Vector2Int minRoomBounds, Vector2Int totalRoomOffest, float corridorWidth)
        {
            this.minRoomBounds = minRoomBounds;
            this.totalRoomOffset = totalRoomOffest;
            this.corridorWidth = corridorWidth;
        }

        public List<SpaceNode> PlaceRoomsInSpaces(List<Node> roomSpaces)

        {
            // list of rooms to return
            List<SpaceNode> rooms = new List<SpaceNode>();

            // loop over spaces placing rooms in each
            foreach(var roomSpace in roomSpaces)
            {
                // TODO: Add check if totalRoomOffset Greater than room Size
                BoundsInt bounds = new BoundsInt(
                    roomSpace.Bounds.position,
                    roomSpace.Bounds.size - new Vector3Int(this.totalRoomOffset.x, 0, this.totalRoomOffset.y)
                    );
                roomSpace.Bounds = bounds;

                // TODO: extend to be able to place anywhere within the given space
                #region add rooms randomly inside space

                //var maxSize = roomSpace.Bounds.size - new Vector3Int(this.totalRoomOffset.x, 0, this.totalRoomOffset.y);
                //var minSize = new Vector3Int(minRoomBounds.x, 0, minRoomBounds.y);

                //// randomize Size 
                //var size = new Vector3Int(Random.Range(minSize.x, maxSize.x), 0, Random.Range(minSize.z, maxSize.z));

                //// randomize Position
                //var deltaSize = size - minSize;
                //Debug.Log(deltaSize);


                //var position = roomSpace.Bounds.position + new Vector3Int(Random.Range(-deltaSize.x / 4, deltaSize.x / 4), 0, Random.Range(-deltaSize.z / 4, deltaSize.z / 4));

                //BoundsInt testBounds = new BoundsInt(
                //    position,
                //    size
                //);

                //roomSpace.Bounds = testBounds;


                #endregion

                rooms.Add((SpaceNode)roomSpace);

            }

            return rooms;
        }
    }
}