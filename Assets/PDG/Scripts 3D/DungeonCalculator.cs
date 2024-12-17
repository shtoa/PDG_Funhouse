using System;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Autodesk.Fbx;
using UnityEngine.UIElements;

namespace dungeonGenerator {
    public class DungeonCalculator
    {
        // Dungeon Properties 
        private int dungeonWidth;
        private int dungeonLength;

        public List<Node> roomSpaces;
        public DungeonCalculator(BoundsInt dungeonDimensions)
        {

            // TODO: Further Refactor this
            this.dungeonWidth = dungeonDimensions.size.x;
            this.dungeonLength = dungeonDimensions.size.z;
        }

        /* <summary>
        
            Calculates the Dungeon Floor Bounds given the Dungeon Width and Length
        
        </summary>*/

        public List<Node> CalculateDungeon(int maxIterations, BoundsInt roomBoundsMin, Vector2 splitCenterDeviationPercent, int corridorWidth, int wallThickness, Vector2Int roomOffset)
        {
            // Calculate the Dungeon Floor Bounds:

            // FIX ME:
            var roomWidthMin = roomBoundsMin.size.x;
            var roomLengthMin = roomBoundsMin.size.z;
            
            #region 1. Space Partitioning

                // 1.1 Generate BSP graph based on minRoomWidth, minRoomLength and maxIterations
                BinarySpacePartitioner bsp = new BinarySpacePartitioner(dungeonWidth, dungeonLength); // initialize BSP class

                // Vector of Minimum Space need to accomodate given Room Size
                 
                // TODO: Add check if this value is impossible
                var minSpaceDim = new Vector2Int(
                    roomWidthMin + roomOffset.x + wallThickness*2,
                    roomLengthMin + roomOffset.y + wallThickness*2
                );

                var allNodeSpaces = bsp.PartitionSpace(maxIterations, minSpaceDim, splitCenterDeviationPercent);  // include roomOffset and wallThickness to have correct placement

                // 1.2 Extract the leaves, which represent the possible room positions (BSP Step May lead to bsp overrliance)
                var roomSpaces = GraphHelper.GetLeaves(bsp.RootNode);
                this.roomSpaces = roomSpaces;


            #endregion

            #region 2. Room Placement

                // TODO: Make this be passed into the generator
                var minRoomDim = new Vector2Int(
                    roomWidthMin,
                    roomLengthMin
                );

                // FIXME: Check if this value is correct
                var totalRoomOffset = roomOffset + Vector2Int.one * wallThickness * 2;
            
                // 2.1 Place rooms within the possible room bounds taking into account room offset
                RoomCalculator roomGenerator = new RoomCalculator(minRoomDim, totalRoomOffset, corridorWidth); // FIXME: Make more general remove corriodr width dependency
                var rooms = roomGenerator.PlaceRoomsInSpaces(roomSpaces);

            #endregion

            #region 3. Generate Corridors

            CorridorGenerator corridorGenerator = new CorridorGenerator();
                var corridorList = corridorGenerator.CreateCorridors(allNodeSpaces, corridorWidth, wallThickness, minRoomDim);
            
            #endregion

            // return a list of bounds on which the floor will be (Rooms and Corridors) 
            return new List<Node>(rooms).Concat(corridorList).ToList();
        }
    }
}
