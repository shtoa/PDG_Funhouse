using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace dungeonGenerator {
    public class DungeonCalculator
    {
        // Dungeon Properties 
        private int dungeonWidth;
        private int dungeonLength;

        public List<Node> roomSpaces;
        public DungeonCalculator(int dungeonWidth, int dungeonLength)
        {
            this.dungeonWidth = dungeonWidth;
            this.dungeonLength = dungeonLength;
        }

        /* <summary>
        
            Calculates the Dungeon Floor Bounds given the Dungeon Width and Length
        
        </summary>*/

        public List<Node> CalculateDungeon(int maxIterations, int roomWidthMin, int roomLengthMin, Vector2 splitCenterDeviationPercent, int corridorWidth, Vector2Int maxDeviation, int wallThickness, Vector2Int roomOffset)
        {
            // Calculate the Dungeon Floor Bounds:

            #region 1. Space Partitioning

                // 1.1 Generate BSP graph based on minRoomWidth, minRoomLength and maxIterations
                BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength); // initialize BSP class

                // Vector of Minimum Space need to accomodate given Room Size
                 
                // TODO: Add check if this value is impossible
                var minSpaceDim = new Vector2Int(
                    roomWidthMin + roomOffset.x + wallThickness,
                    roomLengthMin + roomOffset.y + wallThickness
                );

                var allNodeSpaces = bsp.PartitionSpace(maxIterations, minSpaceDim, splitCenterDeviationPercent);  // include roomOffset and wallThickness to have correct placement

                // 1.2 Extract the leaves, which represent the possible room positions (BSP Step May lead to bsp overrliance)
                var roomSpaces = GraphHelper.GetLeaves(bsp.RootNode);
                this.roomSpaces = roomSpaces;


            #endregion

            #region 2. Room Placement
               
                // 2.1 Place rooms within the possible room bounds taking into account room offset
                RoomCalculator roomGenerator = new RoomCalculator(maxIterations, roomWidthMin, roomLengthMin, wallThickness, roomOffset);
                var rooms = roomGenerator.PlaceRoomsInSpaces(roomSpaces);
            
            #endregion

            #region 3. Generate Corridors
            
                CorridorGenerator corridorGenerator = new CorridorGenerator();
                var corridorList = corridorGenerator.CreateCorridors(allNodeSpaces, corridorWidth, maxDeviation);
            
            #endregion

            // return a list of bounds on which the floor will be (Rooms and Corridors) 
            return new List<Node>(rooms).Concat(corridorList).ToList();
        }
    }
}
