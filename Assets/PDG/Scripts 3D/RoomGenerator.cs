using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using dungeonGenerator;
using System;
using Random = UnityEngine.Random;

namespace dungeonGenerator
{
    public class RoomGenerator
    {
        private int maxIterations;
        private int roomWidthMin;
        private int roomLengthMin;
        private int wallThickness;

       
        public RoomGenerator(int maxIterations, int roomWidthMin, int roomLengthMin, int wallThickness)
        {
            this.maxIterations = maxIterations;
            this.roomWidthMin = roomWidthMin;
            this.roomLengthMin = roomLengthMin;
            this.wallThickness = wallThickness;
        }

        public List<SpaceNode> PlaceRoomsInSpaces(List<Node> roomSpaces)

        {
            List<SpaceNode> rooms = new List<SpaceNode>();

            int roomOffset = 3;

            foreach(var roomSpace in roomSpaces)
            {

                // extend to be able to place anywhere within the given space

                // add offset into the partitioning to account for the room sizes ... 

;
                BoundsInt bounds = new BoundsInt(roomSpace.Bounds.position,
                    roomSpace.Bounds.size - new Vector3Int(1, 0, 1) * (roomOffset+wallThickness)
                    );
                roomSpace.Bounds = bounds;

                //// try to make boxier rooms

                #region add rooms randomly inside space
                //Vector3Int newSize = new Vector3Int(
                //    Random.Range(roomWidthMin, roomSpace.Bounds.size.x) - roomOffset,
                //    roomSpace.Bounds.size.y,
                //    Random.Range(roomLengthMin, roomSpace.Bounds.size.z) - roomOffset
                //);

                //Vector3Int newPos = new Vector3Int(
                //   Random.Range(roomSpace.Bounds.position.x+roomOffset, roomSpace.Bounds.max.x-roomOffset-newSize.x),
                //   roomSpace.Bounds.position.y,
                //   Random.Range(roomSpace.Bounds.position.z+roomOffset, roomSpace.Bounds.max.z-roomOffset-newSize.z)
                //);

                //room.Bounds = new BoundsInt(newPos, newSize);
                #endregion

                rooms.Add((SpaceNode)roomSpace);

            }

            return rooms;
        }
    }
}