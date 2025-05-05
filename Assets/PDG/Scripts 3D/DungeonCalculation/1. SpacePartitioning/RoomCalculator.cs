using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using dungeonGenerator;
using System;
using Random = UnityEngine.Random;
using log4net.Util;
using System.Runtime.Versioning;

namespace dungeonGenerator
{
    public class RoomCalculator
    {
        internal Vector3Int minRoomBounds;
        internal Vector3Int totalRoomOffset;
        internal float corridorWidth;


        /// <summary>
        /// Class to Calculate Room Placement 
        /// </summary>
        /// <param name="minRoomBounds"></param>
        /// <param name="totalRoomOffest"></param>
        /// <param name="corridorWidth"></param>

        public RoomCalculator(Vector3Int minRoomBounds, Vector3Int totalRoomOffest, float corridorWidth)
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
        /// 

        public List<SpaceNode> PlaceRoomsInSpaces(List<Node> roomSpaces, System.Random randomGenerator)

        {
            // list of rooms to return
            List<SpaceNode> rooms = new List<SpaceNode>();

            // loop over spaces placing rooms in each
            foreach(var roomSpace in roomSpaces)
            {
                Debug.Log($"Original size: {roomSpace.Bounds.size}");
                // TODO: Add check if totalRoomOffset Greater than room Size
                BoundsInt bounds = new BoundsInt(
                        roomSpace.Bounds.position,
                        roomSpace.Bounds.size - new Vector3Int(this.totalRoomOffset.x, this.totalRoomOffset.y, this.totalRoomOffset.z)
                    );
                roomSpace.Bounds = bounds;
                roomSpace.RoomType = RoomType.Room;

                // TODO: extend to be able to place anywhere within the given space
                #region add rooms randomly inside space

                var maxSize = roomSpace.Bounds.size; // - new Vector3Int(this.totalRoomOffset.x, 0, this.totalRoomOffset.y); be carefull with previous line
                var minSize = new Vector3Int(minRoomBounds.x, minRoomBounds.z, minRoomBounds.y);

                // randomize Size 
                //var size = new Vector3Int(Random.Range(minSize.x, maxSize.x), Random.Range(minSize.y, Mathf.Min(32,maxSize.y)), Random.Range(minSize.z, maxSize.z));


                Debug.Log($"MinSize values: {minSize.x},{minSize.y},{minSize.z}");
                Debug.Log($"MaxSize values: {maxSize.x},{maxSize.y},{maxSize.z}");
                var size = new Vector3Int(randomGenerator.Next(minSize.x, maxSize.x), randomGenerator.Next(minSize.y, (Mathf.Min(32, maxSize.y))), randomGenerator.Next(minSize.z, maxSize.z));


                // randomize Position
                var deltaSize = size - minSize;
                Debug.Log(deltaSize);


                //var position = roomSpace.Bounds.position + new Vector3Int(Random.Range(0, roomSpace.Bounds.size.x - size.x), 0, Random.Range(0, roomSpace.Bounds.size.z - size.z));


           
                var position = roomSpace.Bounds.position + new Vector3Int(randomGenerator.Next(0, (roomSpace.Bounds.size.x - size.x)), 0, randomGenerator.Next(0, roomSpace.Bounds.size.z - size.z));
   



                // Vector3Int.up*roomSpace.Bounds.position.y + Vector3Int.CeilToInt( new Vector3(roomSpace.Bounds.center.x, 0, roomSpace.Bounds.center.z) - new Vector3(size.x / 2f, 0, size.z / 2f)),


                BoundsInt testBounds = new BoundsInt(
                    position,
                    size
                );

                //BoundsInt testBounds = new BoundsInt();

                //testBounds = roomSpace.Bounds;
                //testBounds.size = size;

               //testBounds.size = new Vector3Int(testBounds.size.x, testBounds.size.y, testBounds.size.z);

               roomSpace.Bounds = testBounds;


                #endregion


                #region 3D Debug Rooms
                //GameObject g = GameObject.CreatePrimitive(PrimitiveType.Cube);
                //g.transform.SetParent(GameObject.Find("DungeonGen").transform, false);
                //g.transform.position = roomSpace.Bounds.center + GameObject.Find("DungeonGen").transform.position;
                //g.transform.localScale = roomSpace.Bounds.size;
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