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
        internal Vector2Int minRoomBounds;
        internal Vector2Int totalRoomOffset;
        internal float corridorWidth;


        /// <summary>
        /// Class to Calculate Room Placement 
        /// </summary>
        /// <param name="minRoomBounds"></param>
        /// <param name="totalRoomOffest"></param>
        /// <param name="corridorWidth"></param>

        public RoomCalculator(Vector2Int minRoomBounds, Vector2Int totalRoomOffest, float corridorWidth)
        {
            this.minRoomBounds = minRoomBounds;
            this.totalRoomOffset = totalRoomOffest;
            this.corridorWidth = corridorWidth;
        }
        
        /// <summary>
        /// Place Rooms inside the available Space based on the offset.
        /// </summary>
        /// <param name="roomSpaces"></param>
        /// <returns>rooms</returns>

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
                roomSpace.RoomType = RoomType.Room;

                // TODO: extend to be able to place anywhere within the given space
                #region add rooms randomly inside space

               // var maxSize = roomSpace.Bounds.size; // - new Vector3Int(this.totalRoomOffset.x, 0, this.totalRoomOffset.y); be carefull with previous line
               // var minSize = new Vector3Int(minRoomBounds.x, 0, minRoomBounds.y);

               // // randomize Size 
               // var size = new Vector3Int(Random.Range(minSize.x, maxSize.x), 0, Random.Range(minSize.z, maxSize.z));

               // // randomize Position
               //var deltaSize = size - minSize;
               //Debug.Log(deltaSize);


               // var position = roomSpace.Bounds.position; // + new Vector3Int(Random.Range(-deltaSize.x / 4, deltaSize.x / 4), 0, Random.Range(-deltaSize.z / 4, deltaSize.z / 4));

               // BoundsInt testBounds = new BoundsInt(
               //     Vector3Int.CeilToInt(roomSpace.Bounds.center - new Vector3(size.x/2f, size.y/2f,size.z/2f)),
               //     size
               // );

               // roomSpace.Bounds = testBounds;


                #endregion

                rooms.Add((SpaceNode)roomSpace);

            }

            #region debug testing placement
            //// TESTING PLACEMENT
            //var rList = new List<Node>(rooms);
            //var rg = new RoomGenerator(rList, GameObject.Find("DungeonGen").gameObject);

            //foreach (var room in rList)
            //{
            //    rg.DrawFloor(room);

            //};
            #endregion

            return rooms;
        }
    }
}