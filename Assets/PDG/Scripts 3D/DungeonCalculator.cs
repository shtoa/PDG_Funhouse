using System;
using System.Collections.Generic;
using tutorialGenerator;
using UnityEngine;


using dungeonGenerator;
using System.Linq;

namespace dungeonGenerator {
    public class DungeonCalculator
    {
        private int dungeonWidth;
        private int dungeonLength;

        
        
        public DungeonCalculator(int dungeonWidth, int dungeonLength)
        {
            this.dungeonWidth = dungeonWidth;
            this.dungeonLength = dungeonLength;
        }

        /// <summary>
        /// Calculate the dungeon floor bounds that can be used to generate the dungeon meshes
        /// </summary>

        public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, Vector2 splitCenterDeviationPercent, int corridorWidth, Vector2Int maxDeviation, int wallThickness, Vector2Int roomOffset)
        {
            // 1. Generate BSP graph based on minRoomWidth, minRoomLength and maxIterations
            BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength);
            var allNodeSpaces = bsp.PartitionSpace(maxIterations, roomWidthMin + roomOffset.x + wallThickness, roomLengthMin + roomOffset.y + wallThickness, splitCenterDeviationPercent);

            // 2. Extract the leaves, which represent the possible room positions
            var roomSpaces = GraphHelper.GetLeaves(bsp.RootNode);

            // 3. Place rooms within the possible room bounds taking into account room offset
            RoomCalculator roomGenerator = new RoomCalculator(maxIterations, roomWidthMin, roomLengthMin, wallThickness, roomOffset);
            var rooms = roomGenerator.PlaceRoomsInSpaces(roomSpaces);

            // 4. Generate the corridors to connect the rooms
            CorridorGenerator corridorGenerator = new CorridorGenerator();
            var corridorList = corridorGenerator.CreateCorridors(allNodeSpaces, corridorWidth, maxDeviation);


            // 5. Pick starting and ending rooms
            var startAndEnd = GraphHelper.ChooseStartAndEnd(roomSpaces); // change the data structure here


            // get edge rooms  
            // MOVE THIS INTO SEPARATE FUNCTION
            var deadEnds = GraphHelper.GetLeaves(startAndEnd[0], false);
            foreach (var deadEnd in deadEnds)
            {
                if (deadEnd != startAndEnd[1])
                {
                    deadEnd.RoomType = RoomType.DeadEnd;
                }
            }

            // REFACTOR !!!
            GameManager.Instance.total[CollectableType.cylinder] = 1;
            GameManager.Instance.total[CollectableType.sphere] = deadEnds.Count - 1;
            GameManager.Instance.total[CollectableType.cube] = roomSpaces.Count - 2 - GameManager.Instance.total[CollectableType.sphere];

            // 6. Return a list of bounds on which the floor will be (Rooms and Corridors) 
            return new List<Node>(rooms).Concat(corridorList).ToList();
        }
    }
}
